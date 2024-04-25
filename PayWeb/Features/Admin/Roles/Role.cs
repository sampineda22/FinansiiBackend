using System;

namespace CRM.Features.Admin.Roles
{
    public class Role
    {
        public string CompanyCode { get; set; }
        public int RoleId { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreationUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }
}
