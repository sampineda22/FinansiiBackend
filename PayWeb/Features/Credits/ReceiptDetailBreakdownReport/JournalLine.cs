using System;

namespace CRM.Features.Credits.ReceiptDetailBreakdownReport
{
    public class JournalLine
    {
        public string JournalNum { get; set; }
        public string DocumentNum { get; set; }
        public string AccountNum { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime TransDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}
