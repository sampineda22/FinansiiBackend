using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Features.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool State { get; set; }
        public string CompanyCode { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
