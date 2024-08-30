using System;

namespace CRM.Features.Accounting.ProvidersReport
{
    public class ProviderReportDto
    {
        public DateTime Date { get; set; }
        public string Provider { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public decimal RealBalance { get; set; }
        public decimal Advance { get; set; }
    }
}
