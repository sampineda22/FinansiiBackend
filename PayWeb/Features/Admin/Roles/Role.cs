using System;

namespace CRM.Features.Admin.Roles
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreationUser { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedUser { get; set; }
    }
}