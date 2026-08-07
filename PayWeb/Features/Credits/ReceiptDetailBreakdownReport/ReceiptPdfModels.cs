using System;
using System.Collections.Generic;

namespace Finansii.Reports.PdfImport
{
    /// <summary>
    /// Una fila de detalle del reporte "Desglose Detalle de Recibos", tal como
    /// viene en el PDF. Se usa un tipo propio en lugar de ReceiptDetailBreakdown
    /// para no depender de los tipos exactos de ese modelo; si se prefiere, el
    /// mapeo a ReceiptDetailBreakdown es directo campo a campo.
    /// </summary>
    public class ReceiptPdfRow
    {
        /// <summary>Numero de recibo ya arrastrado hacia abajo (nunca vacio).</summary>
        public string ReceiptNumber { get; set; } = "";

        /// <summary>Numero de recibo tal cual esta en el PDF: vacio en las filas de continuacion.</summary>
        public string ReceiptNumberRaw { get; set; } = "";

        public string DocumentNumber { get; set; } = "";
        public string FELDocument { get; set; } = "";
        public string ProductType { get; set; } = "";

        /// <summary>Fecha como texto del PDF, p. ej. "07-ene.-2026".</summary>
        public string DateText { get; set; } = "";

        /// <summary>Fecha interpretada. Null si no se pudo parsear.</summary>
        public DateTime? Date { get; set; }

        public string State { get; set; } = "";
        public string ClientAccount { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string DebitCollectorCode { get; set; } = "";
        public string CurrencyCode { get; set; } = "";

        public decimal? ReceiptAmountInCurrency { get; set; }
        public decimal? ReceiptAmount { get; set; }
        public decimal? CanceledReceiptAmount { get; set; }
        public decimal? CashAmount { get; set; }
        public decimal? Total { get; set; }
    }

    /// <summary>Fila de la tabla secundaria "Anticipos Asignados".</summary>
    public class ReceiptPdfAdvanceRow
    {
        public string AdvanceReceipt { get; set; } = "";
        public decimal? AppliedAdvanceAmount { get; set; }
        public string Invoice { get; set; } = "";
    }

    /// <summary>Fila de totales que trae el propio PDF.</summary>
    public class ReceiptPdfTotals
    {
        public decimal? ReceiptAmountInCurrency { get; set; }
        public decimal? ReceiptAmount { get; set; }
        public decimal? CanceledReceiptAmount { get; set; }
        public decimal? CashAmount { get; set; }
        public decimal? Total { get; set; }
    }

    /// <summary>Contenido completo de un PDF de desglose de recibos.</summary>
    public class ReceiptPdfReport
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";

        /// <summary>Codigo del asesor sacado del nombre del archivo ("...-002792.pdf" -> "002792").</summary>
        public string PersonalCode { get; set; } = "";

        public string CompanyName { get; set; } = "";
        public string Year { get; set; } = "";
        public string WeekNumber { get; set; } = "";

        /// <summary>Rango tal cual va en la celda C6, p. ej. "05-01-2026 al 11-01-2026".</summary>
        public string DateRange { get; set; } = "";

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string AgentName { get; set; } = "";
        public string RegisteredBy { get; set; } = "";

        public int PageCount { get; set; }

        /// <summary>Nombres de columna detectados en el encabezado del PDF.</summary>
        public List<string> DetectedColumns { get; set; } = new List<string>();

        public List<ReceiptPdfRow> Details { get; set; } = new List<ReceiptPdfRow>();
        public List<ReceiptPdfAdvanceRow> Advances { get; set; } = new List<ReceiptPdfAdvanceRow>();

        /// <summary>Totales del PDF. Null si el PDF no trae fila de totales.</summary>
        public ReceiptPdfTotals Totals { get; set; }

        /// <summary>Avisos que valen revisar: columnas raras, cuadres que no dan, etc.</summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Notas informativas del proceso de lectura, no son problemas. La mas
        /// comun es el ajuste automatico de columnas cuando un nombre de cliente
        /// se desborda sobre la columna siguiente.
        /// </summary>
        public List<string> Notes { get; set; } = new List<string>();
    }
}
