using System;
using System.Drawing;
using System.Runtime.Intrinsics.Arm;

namespace CRM.Features.Credits.WorkpaperReport
{
    public class WorkPaper
    {
        public string ReceiptNumber { get; set; }
        public DateTime ProcessDate { get; set; }
        public string State { get; set; }
        public string Workpaper { get; set; }
        public string ClientAccount { get; set; }
        public string ClientName { get; set; }
        public string DebtCollector { get; set; }
        public string CurrencyCode { get; set; }
        public decimal ReceiptAmountInCurrency { get; set; }
        public decimal ReceiptAmount { get; set; }
        //public decimal CancelledReceiptAmount { get; set; }
        public string PaymentMethod { get; set; }
        public decimal CashAmount { get; set; }
        public decimal TransferAmount { get; set; }
        public decimal DeductedAmount { get; set; }
        public decimal CheckAmount { get; set; }
        public decimal PostdatedCheckAmount { get; set; }
        public string CheckNumber { get; set; }
        public string CheckDueDate { get; set; }
        public string BankName { get; set; }
        public string Voucher { get; set; }
    }
}
