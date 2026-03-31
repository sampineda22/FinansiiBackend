namespace CRM.Features.Gira.ExpensesSettings
{
    public class ExpenseAccountDto
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public int IdExpenseType { get; set; }
        public string ExpenseTypeName { get; set; }
        public int IdExpenseCategory { get; set; }
        public string ExpenseCategoryName { get; set; }
    }
}