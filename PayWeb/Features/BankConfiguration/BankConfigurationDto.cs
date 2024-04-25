using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CRM.Infrastructure.Enum.Banks;

namespace CRM.Features.BankConfiguration
{
    public class BankConfigurationDto
    {
        public int BankConfigurationId { get; set; }
        public string CompanyId { get; set; }
        public Bank Bank { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FileRoute { get; set; }
        public string LocalFileRoute { get; set; }
        public string FileName { get; set; }
    }
}
