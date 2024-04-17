﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CRM.Infrastructure.Enum.BankStatementStatus;

namespace CRM.Features.BankStatement
{
    public class BankStatement
    {
        public int BankStatementId { get; set; }
        public string Account { get; set; }
        public string Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public BankStatatementState Status { get; set; }
    }
}
