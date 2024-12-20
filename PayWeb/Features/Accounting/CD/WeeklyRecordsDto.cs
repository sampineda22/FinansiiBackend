using System;

namespace CRM.Features.Accounting.CD
{
    public class WeeklyRecordsDto
    {
        public int Week { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DatesRange { get; set; }
        public string Currency { get;set; }
        public decimal AmountInCurrency { get; set; }
        public decimal Amount { get; set; }
        public string Journal { get; set; }
    }
}
