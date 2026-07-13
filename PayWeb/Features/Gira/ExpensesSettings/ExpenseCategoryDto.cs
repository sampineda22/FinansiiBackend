namespace CRM.Features.Gira.ExpensesSettings
{
    public class ExpenseCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int IdExpenseType { get; set; }
        public string TypeName { get; set; }
        public bool IsInvoiceRequired { get; set; }
        public bool IsDescriptionRequired { get; set; }
        public bool IsImageRequired { get; set; }
        public bool Status { get; set; }
        public string? VendAccount { get; set; }
        public string? VendCurrency { get; set; }
        public string CompanyCode { get; set; }
    }
}
