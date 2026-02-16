using CRM.Infrastructure.Enum;

namespace CRM.Features.Accounting.BankStatementServiceAX
{
    public class ExceptionCode
    {
        public string CompanyCode { get; set; }
        public string AccountId { get; set; }
        public string Code { get; set; }
        public int TransactionType { get; set; }
    }
}