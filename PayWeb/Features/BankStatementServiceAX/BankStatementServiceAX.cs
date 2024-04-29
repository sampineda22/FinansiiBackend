using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.BankStatementServiceAX
{
    public class BankStatementServiceAX
    {
        public string JOURNALNAMEID { get; set; }
        public string CURRENCYCODE { get; set; }
        public string LEDGERJOURNALTRANSTXT { get; set; }
        public decimal AMOUNTCURDEBIT { get; set; }
        public decimal AMOUNTCURCREDIT { get; set; }
        public string ACCOUNTNUM { get; set; }
        public string CompanyId { get; set; }

    }
}
