using System;

namespace CRM.Features.Credits.ReceiptDetailBreakdownReport
{
    public class AppliedAdvance
    {
        public string AdvanceReceipt { get; set; }
        public DateTime AppliedAdvanceDate { get; set; }
        public decimal AppliedAdvanceAmount { get; set; }
        public string Invoice { get; set; }
    }
}
