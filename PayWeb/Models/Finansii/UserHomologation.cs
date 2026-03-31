using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models.Finansii
{
    public class UserHomologation
    {
        public string CompanyCode { get; set; }
        public string PersonalCode { get; set; }
        public string AXCode { get; set; }
        public string CRMCode { get; set; }
    }
}
