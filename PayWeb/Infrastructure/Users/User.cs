using PayWebRepository.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Features.Users
{
    public class User
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool State { get; set; }
        public DateTime CreateDateTime { get; set; }
        public string Cod_Empresa { get; set; }
        public string PersonalCode { get; set; }
        public User()
        {

        }

        public User(string userId, string password, string companyCode)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new DomainValidationException("El usuario es requerido.");
            }
            UserId = userId;
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new DomainValidationException("La clave es requerida.");
            }
            Password = password;
            
        }
    }
}
