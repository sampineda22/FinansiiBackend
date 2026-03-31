using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CRM.Features.Gira.ExpensesSettings
{
    public class ExpenseType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Journal { get; set; }
        public string CompanyCode { get; set; }
        public bool State { get; set; }

        [JsonIgnore]
        public virtual ICollection<ExpenseCategory> ExpensesCategories { get; set; }

        [JsonIgnore]
        public virtual ICollection<ExpenseAccount> ExpensesAccounts { get; set; }
    }
}