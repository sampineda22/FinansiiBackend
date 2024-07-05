using System;

namespace CRM.Features.Credits.ReceiptDetailBreakdownReport
{
    public class FiscalWeek
    {
        public string Week { get;set; }
        public DateTime StartDate {get; set; }
        public DateTime EndDate { get; set; }
        public string RecId { get; set; }
    }
}
