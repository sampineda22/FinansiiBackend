using CRM.Features.Admin.Screen;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Admin.Users
{
    public class UserAppService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserAppService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public UserDto FindByUserId(string userid)
        {
            //userid = "dtsItm";
            return (from u in _unitOfWork.Repository<User>().Query()
                    where u.UserId == userid
                    select new UserDto
                    {
                        Id = u.Id,
                        UserId = u.UserId,
                        Password = u.Password,
                        State = u.State,
                        CreateDateTime = u.CreateDateTime,
                        CompanyCode = u.Cod_Empresa
                    }).FirstOrDefault();
        }

        public static bool IsValidUserCredentialsNew(string userId, string password)
        {
            /*userId = "dtsItm";
            password = "Intermoda2020";*/
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            using (var adContext = new PrincipalContext(ContextType.Domain, "intermoda.com.hn"))
            {
                return adContext.ValidateCredentials(userId, password);
            }

        }

        public bool IsValidUserCredentials(string userId, string password)
        {
            /*userId = "dtsItm";
            password = "Intermoda2020";*/
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            using (var adContext = new PrincipalContext(ContextType.Domain, "intermoda.com.hn"))
            {
                return adContext.ValidateCredentials(userId, password);
            }

        }

        public bool IsAnExistingUser(string userId)
        {
            //userId = "dtsItm";
            var user = FindByUserId(userId);
            if (user != null) 
            {
                return true;
            }
            return false;
        }
    }
}
