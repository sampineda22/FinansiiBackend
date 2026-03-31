namespace CRM.Features.Gira.ExpensesSettings
{
    public class User
    {
        public int Id { get; set; }
        public string PersonalCode { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsTemporary { get; set; }
        public bool IsActive { get; set; }
        public string CompanyCode { get; set; }
    }
}