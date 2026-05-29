using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CRM.Features.Gira.Historical
{
    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        [JsonIgnore]
        public virtual ICollection<ExpenseDetail> ExpenseDetails { get; set; }
    }
}