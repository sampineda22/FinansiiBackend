using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CRM.Features.Gira.Historical
{
    public class FuelType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MarkupCode { get; set; }
        public string CompanyCode { get; set; }

        [JsonIgnore]
        public virtual ICollection<ExpenseDetail> ExpenseDetails { get; set; }
    }
}