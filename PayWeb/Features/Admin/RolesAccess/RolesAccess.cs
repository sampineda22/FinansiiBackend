using System;

namespace CRM.Features.Admin.RolesAccess
{
    public class RolesAccess
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public int RolId { get; set; }
        public int ActionId { get; set; }
        public DateTime CreationDate { get; set; }
        public string UserCreation { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set; }
    }
}
