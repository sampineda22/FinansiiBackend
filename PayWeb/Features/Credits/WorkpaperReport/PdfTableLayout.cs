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
    /// Motor comun para leer las tablas de los reportes generados con
    /// Aspose.Cells (Desglose de Recibos y Cedula).
    ///
    /// Los PDF no traen tabla: solo texto colocado en coordenadas. El proceso es:
    ///   1. Agrupar las letras en fragmentos de texto (celdas).
    ///   2. Agrupar los fragmentos en lineas por su coordenada Y.
    ///   3. Detectar la fila de encabezado y sus etiquetas.
    ///   4. Con las posiciones X de los datos, armar "bandas" verticales y
    ///      asociarlas a la etiqueta con la que mas solapan.
    ///
    /// Es la unica clase que depende de PdfPig: para cambiar de libreria basta
    /// con reemplazar ExtractLines.
    /// </summary>
    public static class PdfTableLayout
    {
        // --- tolerancias geometricas (en puntos)
        public const double LineTolerance = 4.0;    // misma linea visual
        public const double HeaderBandY = 10.0;     // renglones que forman el encabezado

        /// <summary>
        /// Holgura para unir dos fragmentos en un mismo bloque de columna. Las
        /// celdas de una misma columna casi siempre se solapan entre si (el texto
        /// comparte el borde izquierdo y los importes el derecho), asi que esta
        /// holgura debe ser minima: en la cedula, "Fecha de Vencimiento" y
        /// "Banco" quedan a 4.1 puntos y con un valor mayor se fusionaban.
        ///
        /// Pasarse de corto no hace dano: dos bloques de la misma columna acaban
        /// apuntando a la misma etiqueta y su contenido se vuelve a unir al armar
        /// la fila. Pasarse de largo si: se pierden columnas.
        /// </summary>
        public const double BlockGapMerge = 2.0;

        /// <summary>
        /// Hueco maximo entre dos letras para considerarlas del mismo texto.
        /// El PDF trae los espacios como glifos, asi que dentro de una celda el
        /// hueco es practicamente 0, mientras que entre columnas es de varios
        /// puntos. Un valor bajo es importante: con 8 se pegaban encabezados
        /// vecinos que quedan a solo 7.5 puntos uno del otro.
        /// </summary>
        public const double LetterGapMerge = 2.0;

        public static readonly Regex ReNumber = new Regex(@"^-?[\d,]+(\.\d+)?$", RegexOptions.Compiled);

        /// <summary>
        /// Celda que el PDF muestra como "###########": la columna quedo angosta
        /// en el Excel del que se genero el reporte, asi que el numero no se
        /// alcanza a ver y no hay forma de recuperarlo.
        /// </summary>
        public static readonly Regex ReUnreadable = new Regex(@"^#+$", RegexOptions.Compiled);

        #region tipos

        public class Item
        {
            public string Text = "";
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

        public class Line
        {
            public double Y;
            public List<Item> Items = new List<Item>();
            public string Text => string.Join(" ", Items.Select(i => i.Text));
        }

        public class Column
        {
            public string Name = "";
            public double X;
            public double X2;
            public double Y;
            public double Center => (X + X2) / 2.0;
        }

        public class Band
        {
            public double X;
            public double X2;
            public int ColIndex;
            public double Center => (X + X2) / 2.0;
        }

        #endregion

        #region extraccion

        /// <summary>Convierte el PDF en lineas de celdas con coordenadas, por pagina.</summary>
        public static List<List<Line>> ExtractLines(string pdfPath)
        {
            var pages = new List<List<Line>>();

            using (PdfDocument document = PdfDocument.Open(pdfPath))
            {
                foreach (Page page in document.GetPages())
                {
                    List<Item> runs = BuildRuns(page);

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
        /// Agrupa las letras de la pagina en fragmentos contiguos, respetando el
        /// orden en que vienen en el PDF.
        ///
        /// No se usa page.GetWords(): ese metodo reagrupa por cercania geometrica
        /// y, cuando un nombre de cliente muy largo se dibuja encima de las
        /// columnas siguientes, entrelaza los glifos de ambas
        /// ("...RESPO2N3S5A0B5ILIDAD..."). Recorriendo el content-stream cada
        /// celda queda intacta aunque se solape visualmente con otra.
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

        #endregion

        #region encabezado

        /// <summary>Indice de la linea que cumple con el predicado de encabezado.</summary>
        public static int FindHeaderIndex(List<Line> lines, Func<string, bool> isHeaderLine)
        {
            for (int i = 0; i < lines.Count; i++)
                if (isHeaderLine(lines[i].Text)) return i;
            return -1;
        }

        /// <summary>
        /// Etiquetas del encabezado con su extension horizontal. Las que vienen
        /// partidas en dos renglones ("Codigo" sobre "Cobrador", "Fecha de" sobre
        /// "Vencimiento") se fusionan por solape en X.
        /// </summary>
        public static List<Column> DetectHeader(List<Line> lines, Func<string, bool> isHeaderLine)
        {
            int idx = FindHeaderIndex(lines, isHeaderLine);
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
                // en X. Dos etiquetas del mismo renglon son siempre columnas
                // distintas, aunque queden pegadas.
                bool distintoRenglon = last != null && Math.Abs(p.Y - last.Y) > 1.0;
                bool solapaEnX = last != null && p.X < last.X2 && p.X2 > last.X;

                if (distintoRenglon && solapaEnX)
                {
                    last.X2 = Math.Max(last.X2, p.X2);
                    last.Name = last.Y >= p.Y ? $"{last.Name} {p.Text}" : $"{p.Text} {last.Name}";
                    last.Y = Math.Max(last.Y, p.Y);
                }
                else
                {
                    cols.Add(new Column { Name = p.Text, X = p.X, X2 = p.X2, Y = p.Y });
                }
            }
            return cols;
        }

        /// <summary>Ultima linea que forma parte del encabezado.</summary>
        public static int HeaderLastLine(List<Line> lines, int headerIndex)
        {
            double yBase = lines[headerIndex].Y;
            int fin = headerIndex;
            for (int i = 0; i < lines.Count; i++)
                if (Math.Abs(lines[i].Y - yBase) <= HeaderBandY && i > fin) fin = i;
            return fin;
        }

        #endregion

        #region bandas

        private static double Overlap(double ax, double ax2, double bx, double bx2)
            => Math.Max(0, Math.Min(ax2, bx2) - Math.Max(ax, bx));

        /// <summary>
        /// Arma las bandas verticales a partir de las posiciones reales de los
        /// datos y las asocia a la etiqueta con la que mas solapan. Asi se
        /// conservan tambien las columnas que el PDF trae vacias.
        /// </summary>
        public static List<Band> BuildBands(List<Column> cols, List<List<Item>> rows, List<string> notes)
        {
            List<Item> all = rows.SelectMany(r => r).OrderBy(i => i.X).ToList();

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
                notes?.Add(
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

        /// <summary>Reparte los fragmentos de una fila entre las columnas.</summary>
        public static string[] RowToCells(List<Item> items, List<Column> cols, List<Band> bands)
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

        /// <summary>Reparte los fragmentos por cercania al centro de cada encabezado.</summary>
        public static string[] NearestCells(List<Item> items, List<Column> header)
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

        #endregion

        #region utilidades

        /// <summary>Valor de la columna cuyo nombre coincide (sin acentos ni signos).</summary>
        public static string Get(string[] cells, List<Column> cols, string wanted)
        {
            int i = cols.FindIndex(c => Normalize(c.Name) == Normalize(wanted));
            return i >= 0 && i < cells.Length ? cells[i] : "";
        }

        public static int IndexOfColumn(List<Column> cols, string wanted)
            => cols.FindIndex(c => Normalize(c.Name) == Normalize(wanted));

        private static readonly Regex ReDiacritics = new Regex("[\\u0300-\\u036f]", RegexOptions.Compiled);

        /// <summary>Compara nombres de columna ignorando acentos, espacios y signos.</summary>
        public static string Normalize(string s)
        {
            if (s == null) return "";
            string d = ReDiacritics.Replace(s.ToLowerInvariant().Normalize(NormalizationForm.FormD), "");
            var sb = new StringBuilder();
            foreach (char c in d) if (char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.ToString();
        }

        public static decimal? ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            string t = text.Trim();
            if (!ReNumber.IsMatch(t)) return null;
            return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal d)
                ? d
                : (decimal?)null;
        }

        public static bool IsUnreadable(string text)
            => !string.IsNullOrEmpty(text) && ReUnreadable.IsMatch(text.Trim());

        #endregion
    }
}
