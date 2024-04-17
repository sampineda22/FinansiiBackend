using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Enum
{
    public class BankStatementStatus
    {
        public enum BankStatatementState 
        {
            Pending,
            Processed,
            Cancelled
        }

    }
}
