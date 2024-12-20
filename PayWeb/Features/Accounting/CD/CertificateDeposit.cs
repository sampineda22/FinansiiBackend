using CRM.Infrastructure.Enum;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using static CRM.Infrastructure.Enum.Banks;
using static CRM.Infrastructure.Enum.Currencies;

namespace CRM.Features.Accounting.CD
{
    public class CertificateDeposit
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string Bank { get; set; }
        public string CDNumber { get; set; }
        public string Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public decimal RatePercentage { get; set; }
        public decimal DailyIncome { get; set; }
        public bool isEnabled { get; set; }
        public string? RenovationCertificate { get; set; }
        public string? Comment { get; set; }
        public bool isCapitalizable { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreationUser { get; set; }
        public DateTime? ModificationDate { get; set; }
        public ICollection<WeeklyRecord> WeeklyRecords { get; set; }
    }
}
