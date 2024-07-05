using System;

namespace CRM.Features.Credits.ReceiptBreakdownReport
{
    public class AccountingWeek
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string WeekNumber { get; set; }
    }
}
