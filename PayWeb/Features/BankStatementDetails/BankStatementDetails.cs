using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Features.BankStatement;
using static CRM.Infrastructure.Enum.TransactionsType;

namespace CRM.Features.BankStatementDetails
{
    public class BankStatementDetails
    {
        public int BankStatementDetailId { get; set; }
        public int BankStatementId { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionCode { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public virtual CRM.Features.BankStatement.BankStatement BankStatement { get; set; }
    }
}
