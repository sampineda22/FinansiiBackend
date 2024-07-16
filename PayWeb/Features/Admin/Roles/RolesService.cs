using CRM.Features.Accounting.BankStatement;
using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Admin.Roles
{
    public class RolesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RolesService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public async Task<List<Role>> GetAllRoles()
        {
            List<Role> roles = _unitOfWork.Repository<Role>().Query().ToList();

            return roles;
        }

        public async Task<EntityResponse> PostRole(RoleDto roleDto, string companyCode, string user)
        {

            try
            {
                Role role = _unitOfWork.Repository<Role>().Query().Where(x => x.Description.ToLower().Replace(" ", "") == roleDto.Description.ToLower().Replace(" ", "")).FirstOrDefault();

                if (role == null)
                {
                    role = new()
                    {
                        CompanyCode = companyCode,
                        Description = roleDto.Description,
                        CreationDate = DateTime.Now,
                        CreationUser = user
                    };

                    _unitOfWork.Repository<Role>().Add(role);
                    await _unitOfWork.SaveChangesAsync();
                    return EntityResponse.CreateOk();
                }

                return EntityResponse.CreateError("Se encontró un rol con el mismo nombre");

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }

        }

        public async Task<EntityResponse> DeleteRole(int roleId)
        {
            try
            {
                Role role = _unitOfWork.Repository<Role>().Query().Where(x => x.RoleId == roleId).First();

                _unitOfWork.Repository<Role>().Delete(role);
                await _unitOfWork.SaveChangesAsync();
                return EntityResponse.CreateOk();
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }
    }
}