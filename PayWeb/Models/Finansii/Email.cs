namespace CRM.Models.Finansii
{
    public class Email
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string EmailAddress { get; set; }
        public string? PersonalCode { get; set; }
        public string Code { get; set; }
        public bool Status { get; set; }
    }
}
