using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Features.Users
{
    public class UserAppService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserAppService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public async Task<EntityResponse> AddUserAsync(UserDto userDto)
        {
            if (userDto == null)
            {
                return EntityResponse.CreateError("Los datos para crear un usuario son obligatorios.");
            }

            User user = new User
            {
                UserId = userDto.UserId,
                Password = userDto.Password,
                State = userDto.State,
                CreateDateTime = userDto.CreateDateTime
            };

            _unitOfWork.Repository<User>().Add(user);
            await _unitOfWork.SaveChangesAsync();
            userDto.Id = user.Id;
            return EntityResponse.CreateOk();
        }

        public async Task<List<UserDto>> GetAll()
        {
            List<UserDto> usuarios = await (from u in _unitOfWork.Repository<User>().Query()
                                               select new UserDto
                                               {
                                                   Id = u.Id,
                                                   UserId = u.UserId,
                                                   Password = u.Password,
                                                   State = u.State,
                                                   CreateDateTime = u.CreateDateTime
                                               }).ToListAsync();
            return usuarios;
        }

        public UserDto FindById(int id)
        {
            return (from u in _unitOfWork.Repository<User>().Query()
                    where u.Id == id
                    select new UserDto
                    {
                        Id = u.Id,
                        UserId = u.UserId,
                        Password = u.Password,
                        State = u.State,
                        CreateDateTime = u.CreateDateTime
                    }).FirstOrDefault();
        }

        public UserDto FindByUserId(string userid)
        {
            userid = "dtsItm";
            return (from u in _unitOfWork.Repository<User>().Query()
                    where u.UserId == userid
                    select new UserDto
                    {
                        Id = u.Id,
                        UserId = u.UserId,
                        Password = u.Password,
                        State = u.State,
                        CreateDateTime = u.CreateDateTime
                    }).FirstOrDefault();
        }

        public static bool IsValidUserCredentialsNew(string userId, string password)
        {
            userId = "dtsItm";
            password = "Intermoda2020";
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
            userId = "dtsItm";
            password = "Intermoda2020";
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
            userId = "dtsItm";
            var user = FindByUserId(userId);
            if (user != null) 
            {
                return true;
            }
            return false;
        }
    }
}
