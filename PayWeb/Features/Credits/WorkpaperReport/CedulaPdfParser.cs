using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Finansii.Reports.PdfImport.PdfTableLayout;

namespace Finansii.Reports.PdfImport
{
    /// <summary>
    /// Lee el desglose del PDF "Reporte de Cedula". No lee el "Resumen de
    /// Cuentas": la lectura se detiene ahi.
    /// </summary>
    public static class CedulaPdfParser
    {
        /// <summary>Columnas del desglose, en el orden del reporte (B..S del template).</summary>
        public static readonly string[] Columns =
        {
            "# Recibo", "Fecha", "Estado", "Cédula", "Numero de Cliente", "Cliente",
            "Código Cobrador", "Divisa", "Valor en Divisa", "Valor Recibo",
            "Forma de Pago", "Efectivo", "Transferencia", "Deducción",
            "Cheque Dia", "Cheque Posfechado", "Fecha de Vencimiento", "Banco"
        };

        private static readonly Regex ReDateRange = new Regex(@"(\d{2}-\d{2}-\d{4})\s*al\s*(\d{2}-\d{2}-\d{4})", RegexOptions.Compiled);
        private static readonly Regex ReYear = new Regex(@"A\u00f1o:\s*(\d{4})", RegexOptions.Compiled);
        private static readonly Regex ReWeek = new Regex(@"Semana:\s*(\d+)", RegexOptions.Compiled);
        private static readonly Regex ReAgent = new Regex(@"Asesor:\s*(.+?)\s*$", RegexOptions.Compiled);

        /// <summary>Formatos de fecha que aparecen en este reporte.</summary>
        private static readonly string[] DateFormats = { "M/d/yyyy", "d/M/yyyy", "MM/dd/yyyy", "dd/MM/yyyy", "dd-MM-yyyy" };

        /// <summary>La fila de encabezado del desglose.</summary>
        private static bool IsHeaderLine(string text)
        {
            string n = Normalize(text);
            return n.Contains(Normalize("# Recibo"))
                && n.Contains(Normalize("Cédula"))
                && n.Contains(Normalize("Forma de Pago"));
        }

        /// <summary>Donde termina el desglose y empieza lo que no se lee.</summary>
        private static bool IsEndOfDetail(string text)
        {
            string n = Normalize(text);
            return n.Contains(Normalize("Resumen de Cuentas"))
                || n.Contains(Normalize("Información de la Cuenta"))
                || n.Contains(Normalize("Elaborado por"))
                || n.Contains(Normalize("Revisado Por"));
        }

        public static CedulaPdfReport Parse(string pdfPath)
        {
            if (string.IsNullOrWhiteSpace(pdfPath)) throw new ArgumentNullException(nameof(pdfPath));
            if (!File.Exists(pdfPath)) throw new FileNotFoundException("No existe el PDF.", pdfPath);

            var report = new CedulaPdfReport
            {
                FilePath = pdfPath,
                FileName = Path.GetFileName(pdfPath),
                // el asesor es el nombre de la carpeta que contiene el PDF
                Asesor = new DirectoryInfo(Path.GetDirectoryName(pdfPath)).Name,
                PersonalCode = ExtractPersonalCode(Path.GetFileNameWithoutExtension(pdfPath))
            };

            List<List<Line>> pages = ExtractLines(pdfPath);
            report.PageCount = pages.Count;
            if (pages.Count == 0 || pages[0].Count == 0)
                throw new InvalidOperationException("El PDF no tiene texto legible.");

            ReadHeaderBlock(pages[0], report);

            List<Column> cols = DetectHeader(pages[0], IsHeaderLine);
            if (cols == null)
                throw new InvalidOperationException("No se encontro la fila de encabezado del desglose.");

            report.DetectedColumns = cols.Select(c => c.Name).ToList();
            if (cols.Count != Columns.Length)
                report.Warnings.Add(
                    $"El PDF trae {cols.Count} columnas y se esperaban {Columns.Length}. " +
                    "Puede ser otra plantilla de reporte.");

            // frontera izquierda del bloque de importes: en la fila de totales no
            // hay nada a la izquierda de este punto
            int iMoney = IndexOfColumn(cols, "Valor en Divisa");
            if (iMoney < 0) iMoney = Math.Max(0, cols.Count - 10);
            double xMoney = iMoney > 0 ? (cols[iMoney - 1].X2 + cols[iMoney].X) / 2.0 : cols[iMoney].X;

            var detailLines = new List<List<Item>>();
            var totalLines = new List<List<Item>>();

            for (int p = 0; p < pages.Count; p++)
            {
                List<Line> lines = pages[p];

                // solo la primera pagina trae encabezado; las demas son continuacion
                int headerIdx = FindHeaderIndex(lines, IsHeaderLine);
                int from = headerIdx >= 0 ? HeaderLastLine(lines, headerIdx) + 1 : 0;
                if (p > 0 && headerIdx >= 0)
                    report.Notes.Add($"La pagina {p + 1} trae su propio encabezado.");

                for (int i = from; i < lines.Count; i++)
                {
                    Line l = lines[i];
                    if (IsEndOfDetail(l.Text)) break;

                    // La fila de totales se reconoce por posicion: no tiene nada en
                    // las columnas de la izquierda. No se exige que todo sea
                    // numerico porque algunos reportes la muestran como "###".
                    bool hasLeft = l.Items.Any(it => it.Center < xMoney);
                    if (!hasLeft) totalLines.Add(l.Items);
                    else if (l.Items.Count >= 4) detailLines.Add(l.Items);
                }
            }

            if (detailLines.Count == 0)
                throw new InvalidOperationException("No se encontraron filas de detalle.");

            // las bandas se calculan con detalle Y totales: los importes totales
            // son mas anchos y caerian fuera de las bandas si solo se usara el detalle
            var forBands = new List<List<Item>>(detailLines);
            forBands.AddRange(totalLines);
            List<Band> bands = BuildBands(cols, forBands, report.Notes);

            string current = "";
            var unreadable = new List<string>();
            int rowNumber = 0;

            foreach (List<Item> items in detailLines)
            {
                string[] cells = RowToCells(items, cols, bands);
                rowNumber++;

                for (int ci = 0; ci < cols.Count; ci++)
                    if (ci < cells.Length && IsUnreadable(cells[ci]))
                        unreadable.Add($"fila {rowNumber} / {cols[ci].Name}");

                var row = new CedulaPdfRow
                {
                    Asesor = report.Asesor,
                    ReceiptNumberRaw = Get(cells, cols, "# Recibo"),
                    ProcessDateText = Get(cells, cols, "Fecha"),
                    State = Get(cells, cols, "Estado"),
                    Workpaper = Get(cells, cols, "Cédula"),
                    ClientAccount = Get(cells, cols, "Numero de Cliente"),
                    ClientName = Get(cells, cols, "Cliente"),
                    DebtCollector = Get(cells, cols, "Código Cobrador"),
                    CurrencyCode = Get(cells, cols, "Divisa"),
                    ReceiptAmountInCurrency = ParseDecimal(Get(cells, cols, "Valor en Divisa")),
                    ReceiptAmount = ParseDecimal(Get(cells, cols, "Valor Recibo")),
                    PaymentMethod = Get(cells, cols, "Forma de Pago"),
                    CashAmount = ParseDecimal(Get(cells, cols, "Efectivo")),
                    TransferAmount = ParseDecimal(Get(cells, cols, "Transferencia")),
                    DeductedAmount = ParseDecimal(Get(cells, cols, "Deducción")),
                    CheckAmount = ParseDecimal(Get(cells, cols, "Cheque Dia")),
                    PostdatedCheckAmount = ParseDecimal(Get(cells, cols, "Cheque Posfechado")),
                    CheckDueDateText = Get(cells, cols, "Fecha de Vencimiento"),
                    BankName = Get(cells, cols, "Banco")
                };

                // el numero de recibo viene vacio cuando se repite
                if (!string.IsNullOrEmpty(row.ReceiptNumberRaw)) current = row.ReceiptNumberRaw;
                row.ReceiptNumber = current;

                row.ProcessDate = ParseDate(row.ProcessDateText);
                row.CheckDueDate = ParseDate(row.CheckDueDateText);

                report.Details.Add(row);
            }

            if (totalLines.Count > 0)
            {
                string[] cells = RowToCells(totalLines[totalLines.Count - 1], cols, bands);

                for (int ci = 0; ci < cols.Count; ci++)
                    if (ci < cells.Length && IsUnreadable(cells[ci]))
                        unreadable.Add($"fila de totales / {cols[ci].Name}");

                report.Totals = new CedulaPdfTotals
                {
                    ReceiptAmountInCurrency = ParseDecimal(Get(cells, cols, "Valor en Divisa")),
                    ReceiptAmount = ParseDecimal(Get(cells, cols, "Valor Recibo")),
                    CashAmount = ParseDecimal(Get(cells, cols, "Efectivo")),
                    TransferAmount = ParseDecimal(Get(cells, cols, "Transferencia")),
                    DeductedAmount = ParseDecimal(Get(cells, cols, "Deducción")),
                    CheckAmount = ParseDecimal(Get(cells, cols, "Cheque Dia")),
                    PostdatedCheckAmount = ParseDecimal(Get(cells, cols, "Cheque Posfechado"))
                };
            }
            else
            {
                report.Warnings.Add("El PDF no trae fila de totales; no se pudo validar el cuadre.");
            }

            if (unreadable.Count > 0)
            {
                int show = Math.Min(unreadable.Count, 5);
                report.Warnings.Add(
                    $"El PDF muestra \"###\" en {unreadable.Count} celda(s) porque la columna quedo " +
                    "angosta en el reporte original, y el numero no se puede recuperar; esas celdas " +
                    $"van vacias: {string.Join(", ", unreadable.Take(show))}" +
                    (unreadable.Count > show ? ", ..." : "") + ".");
            }

            // la carpeta manda para la columna Asesor, pero si el PDF dice otra
            // cosa vale la pena saberlo: pasa cuando un reporte se guardo en la
            // carpeta equivocada
            if (!string.IsNullOrWhiteSpace(report.AgentNameInPdf) &&
                !string.Equals(report.AgentNameInPdf.Trim(), report.Asesor.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                report.Warnings.Add(
                    $"La carpeta se llama \"{report.Asesor}\" pero el PDF dice que el asesor es " +
                    $"\"{report.AgentNameInPdf}\". En la columna Asesor se puso el nombre de la carpeta.");
            }

            ValidateTotals(report);
            return report;
        }

        /// <summary>Compara la suma del detalle contra la fila de totales del PDF.</summary>
        public static void ValidateTotals(CedulaPdfReport report)
        {
            if (report.Totals == null) return;

            void Check(string name, decimal? declared, Func<CedulaPdfRow, decimal?> pick)
            {
                if (declared == null) return;
                decimal sum = report.Details.Sum(d => pick(d) ?? 0m);
                if (Math.Abs(sum - declared.Value) >= 0.02m)
                    report.Warnings.Add(
                        $"El cuadre de \"{name}\" no coincide: suma del detalle {sum:N2} " +
                        $"vs total del PDF {declared.Value:N2}.");
            }

            Check("Valor en Divisa", report.Totals.ReceiptAmountInCurrency, d => d.ReceiptAmountInCurrency);
            Check("Valor Recibo", report.Totals.ReceiptAmount, d => d.ReceiptAmount);
            Check("Efectivo", report.Totals.CashAmount, d => d.CashAmount);
            Check("Transferencia", report.Totals.TransferAmount, d => d.TransferAmount);
            Check("Deducción", report.Totals.DeductedAmount, d => d.DeductedAmount);
            Check("Cheque Dia", report.Totals.CheckAmount, d => d.CheckAmount);
            Check("Cheque Posfechado", report.Totals.PostdatedCheckAmount, d => d.PostdatedCheckAmount);
        }

        private static void ReadHeaderBlock(List<Line> lines, CedulaPdfReport report)
        {
            int take = Math.Min(12, lines.Count);
            for (int i = 0; i < take; i++)
            {
                string t = lines[i].Text;
                Match m;

                if ((m = ReYear.Match(t)).Success) report.Year = m.Groups[1].Value;
                if ((m = ReWeek.Match(t)).Success) report.WeekNumber = m.Groups[1].Value;
                if ((m = ReDateRange.Match(t)).Success)
                    report.DateRange = $"{m.Groups[1].Value} al {m.Groups[2].Value}";
                if ((m = ReAgent.Match(t)).Success) report.AgentNameInPdf = m.Groups[1].Value.Trim();

                if (report.CompanyName.Length == 0 &&
                    lines[i].Items.Count == 1 &&
                    Normalize(t) != Normalize("Cédula") &&
                    Regex.IsMatch(t, @"^[A-Z0-9\.\,\s&\-]{6,}$"))
                {
                    report.CompanyName = t;
                }
            }
        }

        private static DateTime? ParseDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            return DateTime.TryParseExact(text.Trim(), DateFormats, CultureInfo.InvariantCulture,
                                          DateTimeStyles.None, out DateTime d)
                ? d
                : (DateTime?)null;
        }

        /// <summary>"Reporte de Cedula -002792" -> "002792".</summary>
        private static string ExtractPersonalCode(string fileNameWithoutExtension)
        {
            Match m = Regex.Match(fileNameWithoutExtension ?? "", @"-\s*([A-Za-z0-9]+)\s*$");
            return m.Success ? m.Groups[1].Value : "";
        }
    }
}
