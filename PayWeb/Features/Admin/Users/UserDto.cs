using System;

namespace CRM.Features.Admin.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool State { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string CompanyCode { get; set; }
        public string PersonalCode { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string FirstLastName { get; set; }
		public string SecondLastName { get; set; }
        public string Email { get; set; }
        public string CategoryCode { get; set; }
        public string PositionCode { get; set; }
    }
}