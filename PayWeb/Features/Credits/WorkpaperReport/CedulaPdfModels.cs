using System;
using System.Collections.Generic;

namespace Finansii.Reports.PdfImport
{
    /// <summary>
    /// Una fila del desglose del "Reporte de Cedula". Los nombres siguen los del
    /// modelo WorkPaper que usa CreateWorkpaperReports, para que el mapeo sea directo.
    /// </summary>
    public class CedulaPdfRow
    {
        /// <summary>Nombre de la carpeta que contiene el PDF.</summary>
        public string Asesor { get; set; } = "";

        /// <summary>Numero de recibo ya arrastrado hacia abajo (nunca vacio).</summary>
        public string ReceiptNumber { get; set; } = "";

        /// <summary>Numero de recibo tal cual esta en el PDF: vacio si se repite.</summary>
        public string ReceiptNumberRaw { get; set; } = "";

        /// <summary>Fecha como texto del PDF, p. ej. "1/7/2026".</summary>
        public string ProcessDateText { get; set; } = "";
        public DateTime? ProcessDate { get; set; }

        public string State { get; set; } = "";
        public string Workpaper { get; set; } = "";        // Cedula
        public string ClientAccount { get; set; } = "";    // Numero de Cliente
        public string ClientName { get; set; } = "";       // Cliente
        public string DebtCollector { get; set; } = "";    // Codigo Cobrador
        public string CurrencyCode { get; set; } = "";     // Divisa

        public decimal? ReceiptAmountInCurrency { get; set; }
        public decimal? ReceiptAmount { get; set; }

        public string PaymentMethod { get; set; } = "";    // Forma de Pago

        public decimal? CashAmount { get; set; }           // Efectivo
        public decimal? TransferAmount { get; set; }       // Transferencia
        public decimal? DeductedAmount { get; set; }       // Deduccion
        public decimal? CheckAmount { get; set; }          // Cheque Dia
        public decimal? PostdatedCheckAmount { get; set; } // Cheque Posfechado

        /// <summary>Fecha de Vencimiento como texto del PDF.</summary>
        public string CheckDueDateText { get; set; } = "";
        public DateTime? CheckDueDate { get; set; }

        public string BankName { get; set; } = "";         // Banco
    }

    /// <summary>Fila de totales que trae el propio PDF.</summary>
    public class CedulaPdfTotals
    {
        public decimal? ReceiptAmountInCurrency { get; set; }
        public decimal? ReceiptAmount { get; set; }
        public decimal? CashAmount { get; set; }
        public decimal? TransferAmount { get; set; }
        public decimal? DeductedAmount { get; set; }
        public decimal? CheckAmount { get; set; }
        public decimal? PostdatedCheckAmount { get; set; }
    }

    /// <summary>Contenido del desglose de un "Reporte de Cedula".</summary>
    public class CedulaPdfReport
    {
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";

        /// <summary>Nombre de la carpeta que contiene el PDF: es el asesor.</summary>
        public string Asesor { get; set; } = "";

        /// <summary>Codigo del asesor sacado del nombre del archivo.</summary>
        public string PersonalCode { get; set; } = "";

        public string CompanyName { get; set; } = "";
        public string Year { get; set; } = "";
        public string WeekNumber { get; set; } = "";
        public string DateRange { get; set; } = "";

        /// <summary>Asesor segun el propio PDF; puede no coincidir con la carpeta.</summary>
        public string AgentNameInPdf { get; set; } = "";

        public int PageCount { get; set; }
        public List<string> DetectedColumns { get; set; } = new List<string>();

        public List<CedulaPdfRow> Details { get; set; } = new List<CedulaPdfRow>();

        /// <summary>Totales del PDF. Null si no trae fila de totales.</summary>
        public CedulaPdfTotals Totals { get; set; }

        /// <summary>Avisos que vale la pena revisar.</summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>Notas informativas del proceso de lectura, no son problemas.</summary>
        public List<string> Notes { get; set; } = new List<string>();
    }
}
