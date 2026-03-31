namespace CRM.Features.Gira.ExpensesSettings
{
    public class ExpenseAccount
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public int IdExpenseType { get; set; }
        public int IdExpenseCategory { get; set; }
        public string CompanyCode { get; set; }

        public virtual ExpenseType ExpenseType { get; set; }
        public virtual ExpenseCategory ExpenseCategory { get; set; }
    }
}