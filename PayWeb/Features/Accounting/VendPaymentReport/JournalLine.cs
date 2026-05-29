namespace CRM.Features.Accounting.VendPaymentReport
{
    public class JournalLine
    {
        public int LineNum { get; set; }
        public string Voucher {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Debit { get; set; }
        public string PaymentStatus { get; set; }
        public string OffSetLedgerDimension { get; set; }
    }
}