using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Finansii.Reports.PdfImport
{
    /// <summary>
    /// Lee el PDF "Desglose Detalle de Recibos" (plantilla Aspose.Cells) y lo
    /// devuelve como datos. Es la unica clase que sabe de PDFs: si se cambia de
    /// libreria, solo hay que reemplazar ExtractLines.
    ///
    /// Como funciona: el PDF no tiene tabla real, solo texto posicionado. Se
    /// agrupan las palabras en lineas por su coordenada Y, se detecta la fila de
    /// encabezado, y con las posiciones X de los datos se arman "bandas"
    /// verticales que se asocian a cada etiqueta del encabezado. Asi se
    /// conservan tambien las columnas que el PDF trae vacias.
    /// </summary>
    public static class ReceiptPdfParser
    {
        // --- tolerancias geometricas (en puntos; la pagina mide ~1637 x 792)
        private const double LineTolerance = 4.0;    // misma linea visual
        private const double BlockGapMerge = 6.0;    // celdas de una misma columna
        private const double HeaderBandY = 10.0;     // renglones que forman el encabezado

        /// <summary>
        /// Hueco maximo entre dos letras para considerarlas del mismo texto.
        /// El PDF trae los espacios como glifos, asi que dentro de una celda el
        /// hueco entre letras es practicamente 0, mientras que entre columnas es
        /// de varios puntos. Un valor bajo es importante: con 8 se pegaban los
        /// encabezados "Valor en divisa" y "Valor Recibo", que en algunos
        /// reportes quedan a solo 7.5 puntos uno del otro.
        /// </summary>
        private const double LetterGapMerge = 2.0;

        private static readonly Regex ReNumber = new Regex(@"^-?[\d,]+(\.\d+)?$", RegexOptions.Compiled);
        private static readonly Regex ReDateText = new Regex(@"^(\d{1,2})-([A-Za-z\u00C0-\u017F]{3,4})\.?-(\d{4})$", RegexOptions.Compiled);
        private static readonly Regex ReDateRange = new Regex(@"(\d{2}-\d{2}-\d{4})\s*al\s*(\d{2}-\d{2}-\d{4})", RegexOptions.Compiled);
        private static readonly Regex ReYear = new Regex(@"A\u00f1o:\s*(\d{4})", RegexOptions.Compiled);
        private static readonly Regex ReWeek = new Regex(@"Semana:\s*(\d+)", RegexOptions.Compiled);
        private static readonly Regex ReAgent = new Regex(@"Asesor:\s*(.+?)\s*$", RegexOptions.Compiled);

        private static readonly Dictionary<string, int> Months = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "ene", 1 }, { "feb", 2 }, { "mar", 3 }, { "abr", 4 }, { "may", 5 }, { "jun", 6 },
            { "jul", 7 }, { "ago", 8 }, { "sep", 9 }, { "set", 9 }, { "sept", 9 },
            { "oct", 10 }, { "nov", 11 }, { "dic", 12 }
        };

        /// <summary>Nombres y orden de las columnas de la tabla de detalle.</summary>
        private static readonly string[] Columns =
        {
            "# Recibo", "# de Documento", "# de Documento FEL", "Tipo de Producto",
            "Fecha", "Estado", "Numero de Cliente", "Cliente", "Codigo Cobrador",
            "Divisa", "Valor en divisa", "Valor Recibo", "Valor Recibo Anulado",
            "Valor", "Total"
        };

        #region tipos internos

        private class Item
        {
            public string Text;
            public double X;
            public double X2;
            public double Y;
            public double Center => (X + X2) / 2.0;
            public bool IsNumber => ReNumber.IsMatch(Text);

            /// <summary>
            /// Borde por el que se decide la columna. Aspose alinea el texto a la
            /// izquierda y los importes a la derecha, asi que el borde fijo es el
            /// que manda: usar el centro falla con nombres de cliente muy largos,
            /// que se desbordan sobre la columna siguiente.
            /// </summary>
            public double Anchor => IsNumber ? X2 - 0.5 : X + 0.5;
        }

        private class Line
        {
            public double Y;
            public List<Item> Items = new List<Item>();
            public string Text => string.Join(" ", Items.Select(i => i.Text));
        }

        private class Column
        {
            public string Name;
            public double X;
            public double X2;
            public double Y;
            public double Center => (X + X2) / 2.0;
        }

        private class Band
        {
            public double X;
            public double X2;
            public int ColIndex;
            public double Center => (X + X2) / 2.0;
        }

        #endregion

        // ------------------------------------------------------------------ API

        public static ReceiptPdfReport Parse(string pdfPath)
        {
            if (string.IsNullOrWhiteSpace(pdfPath)) throw new ArgumentNullException(nameof(pdfPath));
            if (!File.Exists(pdfPath)) throw new FileNotFoundException("No existe el PDF.", pdfPath);

            var report = new ReceiptPdfReport
            {
                FilePath = pdfPath,
                FileName = Path.GetFileName(pdfPath),
                PersonalCode = ExtractPersonalCode(Path.GetFileNameWithoutExtension(pdfPath))
            };

            List<List<Line>> pages = ExtractLines(pdfPath);
            report.PageCount = pages.Count;
            if (pages.Count == 0 || pages[0].Count == 0)
                throw new InvalidOperationException("El PDF no tiene texto legible.");

            ReadHeaderBlock(pages[0], report);
            report.RegisteredBy = ReadRegisteredBy(pages[pages.Count - 1]);

            List<Column> cols = DetectHeader(pages[0]);
            if (cols == null)
                throw new InvalidOperationException("No se encontro la fila de encabezado de la tabla.");

            report.DetectedColumns = cols.Select(c => c.Name).ToList();
            if (cols.Count != Columns.Length)
            {
                report.Warnings.Add(
                    $"El PDF trae {cols.Count} columnas y se esperaban {Columns.Length}. " +
                    "Puede ser otra plantilla de reporte.");
            }

            // frontera izquierda del bloque de importes: en la fila de totales no
            // hay nada a la izquierda de este punto
            int iMoney = cols.FindIndex(c => c.Name.StartsWith("Valor", StringComparison.OrdinalIgnoreCase)
                                          || c.Name.StartsWith("Total", StringComparison.OrdinalIgnoreCase));
            if (iMoney < 0) iMoney = Math.Max(0, cols.Count - 5);
            double xMoney = iMoney > 0 ? (cols[iMoney - 1].X2 + cols[iMoney].X) / 2.0 : cols[iMoney].X;

            var detailLines = new List<List<Item>>();
            var totalLines = new List<List<Item>>();
            var advanceLines = new List<List<Item>>();
            List<Column> advanceHeader = null;

            for (int p = 0; p < pages.Count; p++)
            {
                List<Line> lines = pages[p];

                // solo la primera pagina trae encabezado; las demas son continuacion
                List<Column> own = p == 0 ? cols : DetectHeader(lines);
                int from = 0;
                if (own != null)
                {
                    int headerIdx = FindHeaderIndex(lines);
                    double yBase = lines[headerIdx].Y;
                    for (int i = 0; i < lines.Count; i++)
                        if (Math.Abs(lines[i].Y - yBase) <= HeaderBandY && i >= from) from = i + 1;
                    if (p > 0) report.Warnings.Add($"La pagina {p + 1} trae su propio encabezado.");
                }

                bool inAdvances = false;
                for (int i = from; i < lines.Count; i++)
                {
                    Line l = lines[i];
                    string t = l.Text;

                    if (t.IndexOf("Anticipos Asignados", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        inAdvances = true;
                        continue;
                    }
                    if (t.StartsWith("Registrado por", StringComparison.OrdinalIgnoreCase)) break;

                    if (inAdvances)
                    {
                        if (t.IndexOf("# Recibo", StringComparison.OrdinalIgnoreCase) >= 0 &&
                            t.IndexOf("Monto Aplicado", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (advanceHeader == null)
                                advanceHeader = l.Items
                                    .Select(it => new Column { Name = it.Text, X = it.X, X2 = it.X2, Y = it.Y })
                                    .ToList();
                            continue;
                        }
                        // el valor de "Registrado por" queda suelto arriba de su
                        // etiqueta: una fila real de anticipos trae al menos 2 celdas
                        if (l.Items.Count >= 2) advanceLines.Add(l.Items);
                        continue;
                    }

                    bool allNumbers = l.Items.All(it => it.IsNumber);
                    bool hasLeft = l.Items.Any(it => it.Center < xMoney);
                    if (allNumbers && !hasLeft) totalLines.Add(l.Items);
                    else if (l.Items.Count >= 4) detailLines.Add(l.Items);
                }
            }

            if (detailLines.Count == 0)
                throw new InvalidOperationException("No se encontraron filas de detalle.");

            // las bandas se calculan con detalle Y totales: los importes totales
            // son mas anchos y caerian fuera de las bandas si solo se usara el detalle
            var forBands = new List<List<Item>>(detailLines);
            forBands.AddRange(totalLines);
            // el ajuste de columnas es informativo, no un problema: va a Notes
            List<Band> bands = BuildBands(cols, forBands, report.Notes);

            // --- detalle
            string current = "";
            foreach (List<Item> items in detailLines)
            {
                string[] cells = RowToCells(items, cols, bands);
                var row = new ReceiptPdfRow
                {
                    ReceiptNumberRaw = Get(cells, cols, "# Recibo"),
                    DocumentNumber = Get(cells, cols, "# de Documento"),
                    FELDocument = Get(cells, cols, "# de Documento FEL"),
                    ProductType = Get(cells, cols, "Tipo de Producto"),
                    DateText = Get(cells, cols, "Fecha"),
                    State = Get(cells, cols, "Estado"),
                    ClientAccount = Get(cells, cols, "Numero de Cliente"),
                    ClientName = Get(cells, cols, "Cliente"),
                    DebitCollectorCode = Get(cells, cols, "Codigo Cobrador"),
                    CurrencyCode = Get(cells, cols, "Divisa"),
                    ReceiptAmountInCurrency = ParseDecimal(Get(cells, cols, "Valor en divisa")),
                    ReceiptAmount = ParseDecimal(Get(cells, cols, "Valor Recibo")),
                    CanceledReceiptAmount = ParseDecimal(Get(cells, cols, "Valor Recibo Anulado")),
                    CashAmount = ParseDecimal(Get(cells, cols, "Valor")),
                    Total = ParseDecimal(Get(cells, cols, "Total"))
                };

                // el numero de recibo viene vacio en las filas de continuacion
                if (!string.IsNullOrEmpty(row.ReceiptNumberRaw)) current = row.ReceiptNumberRaw;
                row.ReceiptNumber = current;
                row.Date = ParseSpanishDate(row.DateText);
                report.Details.Add(row);
            }

            // --- totales del PDF
            if (totalLines.Count > 0)
            {
                string[] cells = RowToCells(totalLines[totalLines.Count - 1], cols, bands);
                report.Totals = new ReceiptPdfTotals
                {
                    ReceiptAmountInCurrency = ParseDecimal(Get(cells, cols, "Valor en divisa")),
                    ReceiptAmount = ParseDecimal(Get(cells, cols, "Valor Recibo")),
                    CanceledReceiptAmount = ParseDecimal(Get(cells, cols, "Valor Recibo Anulado")),
                    CashAmount = ParseDecimal(Get(cells, cols, "Valor")),
                    Total = ParseDecimal(Get(cells, cols, "Total"))
                };
            }
            else
            {
                report.Warnings.Add("El PDF no trae fila de totales; no se pudo validar el cuadre.");
            }

            // --- anticipos
            foreach (List<Item> items in advanceLines)
            {
                string[] cells = advanceHeader == null
                    ? items.Select(i => i.Text).ToArray()
                    : NearestCells(items, advanceHeader);
                report.Advances.Add(new ReceiptPdfAdvanceRow
                {
                    AdvanceReceipt = cells.Length > 0 ? cells[0] : "",
                    AppliedAdvanceAmount = cells.Length > 1 ? ParseDecimal(cells[1]) : null,
                    Invoice = cells.Length > 2 ? cells[2] : ""
                });
            }

            ValidateTotals(report);
            return report;
        }

        /// <summary>Compara la suma del detalle contra la fila de totales del PDF.</summary>
        public static void ValidateTotals(ReceiptPdfReport report)
        {
            if (report.Totals == null) return;

            void Check(string name, decimal? declared, Func<ReceiptPdfRow, decimal?> pick)
            {
                if (declared == null) return;
                decimal sum = report.Details.Sum(d => pick(d) ?? 0m);
                if (Math.Abs(sum - declared.Value) >= 0.02m)
                {
                    report.Warnings.Add(
                        $"El cuadre de \"{name}\" no coincide: suma del detalle {sum:N2} " +
                        $"vs total del PDF {declared.Value:N2}.");
                }
            }

            Check("Valor en divisa", report.Totals.ReceiptAmountInCurrency, d => d.ReceiptAmountInCurrency);
            Check("Valor Recibo", report.Totals.ReceiptAmount, d => d.ReceiptAmount);
            Check("Valor Recibo Anulado", report.Totals.CanceledReceiptAmount, d => d.CanceledReceiptAmount);
            Check("Valor", report.Totals.CashAmount, d => d.CashAmount);
            Check("Total", report.Totals.Total, d => d.Total);
        }

        // -------------------------------------------------- extraccion con PdfPig

        /// <summary>
        /// Convierte el PDF en lineas de celdas con coordenadas. Es el unico
        /// punto que depende de PdfPig.
        ///
        /// No se usa page.GetWords(): ese metodo reagrupa las letras por cercania
        /// geometrica y, cuando un nombre de cliente muy largo se dibuja encima de
        /// las columnas siguientes, entrelaza los glifos de ambas ("...RESPO2N3S5A0B5ILIDAD...").
        /// Aqui se recorren las letras en el orden del content-stream, que es el
        /// orden en que el generador las escribio, de modo que cada celda queda
        /// intacta aunque se solape visualmente con otra.
        /// </summary>
        private static List<List<Line>> ExtractLines(string pdfPath)
        {
            var pages = new List<List<Line>>();

            using (PdfDocument document = PdfDocument.Open(pdfPath))
            {
                foreach (Page page in document.GetPages())
                {
                    var runs = BuildRuns(page);

                    // agrupar por Y -> lineas
                    var lines = new List<Line>();
                    foreach (Item it in runs.OrderByDescending(i => i.Y).ThenBy(i => i.X))
                    {
                        Line ln = lines.FirstOrDefault(l => Math.Abs(l.Y - it.Y) < LineTolerance);
                        if (ln == null)
                        {
                            ln = new Line { Y = it.Y };
                            lines.Add(ln);
                        }
                        ln.Items.Add(it);
                    }

                    foreach (Line ln in lines) ln.Items = ln.Items.OrderBy(i => i.X).ToList();

                    // descartar la marca de agua de la version de evaluacion de Aspose
                    lines.RemoveAll(l =>
                        l.Items.Count == 0 ||
                        l.Text.IndexOf("Aspose", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        l.Text.IndexOf("Evaluation Only", StringComparison.OrdinalIgnoreCase) >= 0);

                    pages.Add(lines.OrderByDescending(l => l.Y).ToList());
                }
            }

            return pages;
        }

        /// <summary>
        /// Agrupa las letras de la pagina en fragmentos de texto contiguos,
        /// respetando el orden en que vienen en el PDF. Se corta el fragmento
        /// cuando cambia de renglon o cuando la letra siguiente no continua
        /// donde termino la anterior.
        /// </summary>
        private static List<Item> BuildRuns(Page page)
        {
            var runs = new List<Item>();
            Item current = null;
            double prevRight = 0, prevY = 0;

            foreach (Letter letter in page.Letters)
            {
                string ch = letter.Value;
                if (string.IsNullOrEmpty(ch)) continue;

                // se usa la linea base, no GlyphRectangle: el rectangulo del glifo
                // cambia de alto segun la letra (las descendentes como "p" o "g"
                // bajan, el guion queda arriba) y eso partiria el fragmento
                double left = letter.StartBaseLine.X;
                double right = letter.EndBaseLine.X;
                double y = letter.StartBaseLine.Y;

                bool continues = current != null
                                 && Math.Abs(y - prevY) < 1.5            // mismo renglon
                                 && left - prevRight <= LetterGapMerge   // sigue de cerca
                                 && left >= prevRight - 2.0;             // y no salta hacia atras

                if (continues)
                {
                    current.Text += ch;
                    current.X2 = Math.Max(current.X2, right);
                }
                else
                {
                    current = new Item { Text = ch, X = left, X2 = right, Y = y };
                    runs.Add(current);
                }

                prevRight = right;
                prevY = y;
            }

            foreach (Item r in runs) r.Text = r.Text.Trim();
            return runs.Where(r => r.Text.Length > 0).ToList();
        }

        // ------------------------------------------------------------ encabezado

        private static int FindHeaderIndex(List<Line> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                string t = lines[i].Text;
                if (t.Contains("# Recibo") && t.Contains("# de Documento") && t.Contains("Total"))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Etiquetas del encabezado con su extension horizontal. Las que vienen
        /// partidas en dos renglones ("Codigo" / "Cobrador") se fusionan por solape en X.
        /// </summary>
        private static List<Column> DetectHeader(List<Line> lines)
        {
            int idx = FindHeaderIndex(lines);
            if (idx < 0) return null;

            double yBase = lines[idx].Y;
            var pieces = new List<Item>();
            foreach (Line l in lines)
                if (Math.Abs(l.Y - yBase) <= HeaderBandY) pieces.AddRange(l.Items);

            var cols = new List<Column>();
            foreach (Item p in pieces.OrderBy(i => i.X))
            {
                Column last = cols.Count > 0 ? cols[cols.Count - 1] : null;

                // solo se fusionan etiquetas de renglones distintos que se solapan
                // en X ("Codigo" sobre "Cobrador"). Dos etiquetas del mismo renglon
                // son siempre columnas distintas, aunque queden pegadas.
                bool distintoRenglon = last != null && Math.Abs(p.Y - last.Y) > 1.0;
                bool solapaEnX = last != null && p.X < last.X2 && p.X2 > last.X;

                if (distintoRenglon && solapaEnX)
                {
                    last.X2 = Math.Max(last.X2, p.X2);
                    last.Name = last.Y >= p.Y ? last.Name + " " + p.Text : p.Text + " " + last.Name;
                    last.Y = Math.Max(last.Y, p.Y);
                }
                else
                {
                    cols.Add(new Column { Name = p.Text, X = p.X, X2 = p.X2, Y = p.Y });
                }
            }
            return cols;
        }

        // ---------------------------------------------------------------- bandas

        private static double Overlap(double ax, double ax2, double bx, double bx2)
            => Math.Max(0, Math.Min(ax2, bx2) - Math.Max(ax, bx));

        /// <summary>
        /// Arma las bandas verticales a partir de las posiciones reales de los
        /// datos y las asocia a la etiqueta con la que mas solapan.
        /// </summary>
        private static List<Band> BuildBands(List<Column> cols, List<List<Item>> rows, List<string> notes)
        {
            var all = rows.SelectMany(r => r).OrderBy(i => i.X).ToList();

            var blocks = new List<Band>();
            foreach (Item it in all)
            {
                Band last = blocks.Count > 0 ? blocks[blocks.Count - 1] : null;
                if (last != null && it.X <= last.X2 + BlockGapMerge) last.X2 = Math.Max(last.X2, it.X2);
                else blocks.Add(new Band { X = it.X, X2 = it.X2, ColIndex = -1 });
            }

            var bands = new List<Band>();
            foreach (Band bl in blocks)
            {
                var touched = new List<int>();
                for (int ci = 0; ci < cols.Count; ci++)
                    if (Overlap(bl.X, bl.X2, cols[ci].X, cols[ci].X2) > 0) touched.Add(ci);

                if (touched.Count == 0)
                {
                    int best = 0;
                    double dist = double.MaxValue;
                    for (int ci = 0; ci < cols.Count; ci++)
                    {
                        double d = Math.Abs(cols[ci].Center - bl.Center);
                        if (d < dist) { dist = d; best = ci; }
                    }
                    bands.Add(new Band { X = bl.X, X2 = bl.X2, ColIndex = best });
                    continue;
                }

                if (touched.Count == 1)
                {
                    bands.Add(new Band { X = bl.X, X2 = bl.X2, ColIndex = touched[0] });
                    continue;
                }

                // el bloque abarca varias columnas (valores muy anchos que se
                // juntaron): partirlo en el punto medio entre encabezados
                notes.Add(
                    $"Un bloque de datos abarcaba {touched.Count} columnas " +
                    $"({string.Join(" / ", touched.Select(i => cols[i].Name))}); se dividio automaticamente.");

                double start = bl.X;
                for (int k = 0; k < touched.Count; k++)
                {
                    int ci = touched[k];
                    double end = k == touched.Count - 1
                        ? bl.X2
                        : (cols[ci].X2 + cols[touched[k + 1]].X) / 2.0;
                    bands.Add(new Band { X = start, X2 = end, ColIndex = ci });
                    start = end;
                }
            }

            return bands.OrderBy(b => b.X).ToList();
        }

        private static string[] RowToCells(List<Item> items, List<Column> cols, List<Band> bands)
        {
            var cells = new List<string>[cols.Count];
            for (int i = 0; i < cells.Length; i++) cells[i] = new List<string>();

            foreach (Item it in items)
            {
                double a = it.Anchor;
                Band b = bands.FirstOrDefault(bb => a >= bb.X - 1 && a <= bb.X2 + 1);
                if (b == null)
                {
                    double dist = double.MaxValue;
                    foreach (Band bb in bands)
                    {
                        double d = a < bb.X ? bb.X - a : (a > bb.X2 ? a - bb.X2 : 0);
                        if (d < dist) { dist = d; b = bb; }
                    }
                }
                if (b != null && b.ColIndex >= 0 && b.ColIndex < cells.Length)
                    cells[b.ColIndex].Add(it.Text);
            }

            return cells.Select(c => string.Join(" ", c).Trim()).ToArray();
        }

        private static string[] NearestCells(List<Item> items, List<Column> header)
        {
            var cells = new List<string>[header.Count];
            for (int i = 0; i < cells.Length; i++) cells[i] = new List<string>();

            foreach (Item it in items)
            {
                int best = 0;
                double dist = double.MaxValue;
                for (int ci = 0; ci < header.Count; ci++)
                {
                    double d = Math.Abs(header[ci].Center - it.Center);
                    if (d < dist) { dist = d; best = ci; }
                }
                cells[best].Add(it.Text);
            }
            return cells.Select(c => string.Join(" ", c).Trim()).ToArray();
        }

        // -------------------------------------------------------- bloque superior

        private static void ReadHeaderBlock(List<Line> lines, ReceiptPdfReport report)
        {
            int take = Math.Min(12, lines.Count);
            for (int i = 0; i < take; i++)
            {
                string t = lines[i].Text;

                Match m = ReYear.Match(t);
                if (m.Success) report.Year = m.Groups[1].Value;

                m = ReWeek.Match(t);
                if (m.Success) report.WeekNumber = m.Groups[1].Value;

                m = ReDateRange.Match(t);
                if (m.Success)
                {
                    report.DateRange = $"{m.Groups[1].Value} al {m.Groups[2].Value}";
                    report.StartDate = ParseDayMonthYear(m.Groups[1].Value);
                    report.EndDate = ParseDayMonthYear(m.Groups[2].Value);
                }

                m = ReAgent.Match(t);
                if (m.Success) report.AgentName = m.Groups[1].Value.Trim();

                // el nombre de la empresa va solo, debajo del titulo
                if (report.CompanyName.Length == 0 &&
                    lines[i].Items.Count == 1 &&
                    t.IndexOf("Desglose", StringComparison.OrdinalIgnoreCase) < 0 &&
                    Regex.IsMatch(t, @"^[A-Z0-9\.\,\s&\-]{6,}$"))
                {
                    report.CompanyName = t;
                }
            }
        }

        /// <summary>En la plantilla el valor de "Registrado por" va en el renglon de arriba.</summary>
        private static string ReadRegisteredBy(List<Line> lines)
        {
            int i = lines.FindIndex(l => l.Text.StartsWith("Registrado por", StringComparison.OrdinalIgnoreCase));
            if (i < 0) return "";

            string inline = Regex.Replace(lines[i].Text, @"^Registrado por:?\s*", "").Trim();
            if (inline.Length > 0) return inline;
            return i > 0 ? lines[i - 1].Text.Trim() : "";
        }

        // ------------------------------------------------------------- utilidades

        private static string Get(string[] cells, List<Column> cols, string wanted)
        {
            int i = cols.FindIndex(c => Normalize(c.Name) == Normalize(wanted));
            return i >= 0 && i < cells.Length ? cells[i] : "";
        }

        private static string Normalize(string s)
        {
            if (s == null) return "";
            string d = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        private static decimal? ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            string t = text.Trim();
            if (!ReNumber.IsMatch(t)) return null;
            return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d)
                ? d
                : (decimal?)null;
        }

        /// <summary>Interpreta "07-ene.-2026" (mes abreviado en espanol).</summary>
        private static DateTime? ParseSpanishDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            Match m = ReDateText.Match(text.Trim());
            if (!m.Success) return null;

            string month = m.Groups[2].Value.TrimEnd('.');
            if (!Months.TryGetValue(month, out int mm)) return null;

            if (!int.TryParse(m.Groups[1].Value, out int dd)) return null;
            if (!int.TryParse(m.Groups[3].Value, out int yyyy)) return null;

            try { return new DateTime(yyyy, mm, dd); }
            catch { return null; }
        }

        private static DateTime? ParseDayMonthYear(string text)
            => DateTime.TryParseExact(text, "dd-MM-yyyy", CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, out DateTime d)
                ? d
                : (DateTime?)null;

        /// <summary>"Reporte detalle de recibos -002792" -> "002792".</summary>
        private static string ExtractPersonalCode(string fileNameWithoutExtension)
        {
            Match m = Regex.Match(fileNameWithoutExtension ?? "", @"-\s*([A-Za-z0-9]+)\s*$");
            return m.Success ? m.Groups[1].Value : "";
        }
    }
}
