namespace CRM.Models.Finansii
{
    public class Email
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string PersonalCode { get; set; }
        public string ProjectCode { get; set; }
        public bool Status { get; set; }
    }
}
