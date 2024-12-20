namespace CRM.Features.Accounting.CD
{
    public class WeeklyRecord
    {
        public int Id { get; set; }
        public int CertificateId { get; set; }
        public decimal AmountInCurrency { get; set; }
        public decimal Amount { get; set; }
        public int Week { get; set; }
        public string Journal { get; set; }
        public CertificateDeposit CertificateDeposit { get; set; }
    }
}
