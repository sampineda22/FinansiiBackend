namespace CRM.Features.Gira.ExpensesDetails
{
    public class InvoiceSequence
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string Initials { get; set; }
        public int SequenceNumber { get; set; }
        public string CurrentSequence { get; set; }
    }
}