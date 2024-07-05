using System;

namespace CRM.Features.Credits.ReceiptDetailBreakdownReport
{
    public class ReceiptDetailBreakdown
    {
        public string ReceiptNumber { get; set; }
        public string DocumentNumber { get; set;}
        public string FELDocument { get; set;}
        public string ProductType { get; set;}
        public DateTime Date { get; set;}
        public string State { get; set;}
        public string ClientAccount { get; set;}
        public string ClientName { get; set;}
        public string DebitCollectorCode { get; set;}
        public string CurrencyCode { get; set;}
        public decimal ReceiptAmountInCurrency { get; set;}
        public decimal ReceiptAmount { get; set;}
        public decimal CashAmount { get;set;}
        public decimal CanceledReceiptAmount { get; set;}
        public decimal Total { get; set;}
    }
}
