using System;

namespace CRM.Features.Admin
{
    public class Actions
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreationUser { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedUser { get; set;}
    }
}
