using CRM.Features.Gira.Historical;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CRM.Features.Gira.ExpensesSettings
{
    public class ExpenseCategory
    {
        public int Id { get; set; }
        public int IdExpenseType { get; set; }
        public string Name { get; set; }
        public bool IsInvoiceRequired { get; set; }
        public bool IsDescriptionRequired { get; set; }
        public bool IsImageRequired { get; set; }
        public bool Status { get; set; }
        public string? VendAccount { get; set; }
        public string CompanyCode { get; set; }

        public virtual ExpenseType ExpenseType { get; set; }

        [JsonIgnore]
        public virtual ICollection<ExpenseAccount> ExpensesAccounts { get; set; }

        [JsonIgnore]
        public virtual ICollection<ExpenseDetail> ExpenseDetails { get; set; }
    }
}