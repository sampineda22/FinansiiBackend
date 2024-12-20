namespace CRM.Features.Accounting.CD
{
    public class Certificate
    {
        public int ID { get;set; }
        public string CERTIFICATENUMBER { get; set; }
        public string LEDGERDIMENSION { get; set; }
        public string TRANSDATE { get; set; }
        public string JOURNALDATE { get; set; }
        public string LEDGERJOURNALTRANSTXT { get; set; }
        public string CURRENCYCODE { get; set; }
        public decimal AMOUNTINCURRENCY { get; set; }
        public decimal AMOUNTCURDEBIT { get; set; }
        public decimal AMOUNTCURCREDIT { get; set; }
    }
}
