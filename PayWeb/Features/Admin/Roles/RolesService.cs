using CRM.Features.BankStatement;
using Microsoft.EntityFrameworkCore;
using PayWeb.Infrastructure.Core;
using System.Collections.Generic;
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

        public async Task<List<Roles>> GetAllRoles()
        {
            List<Roles> roles2 = await (from u in _unitOfWork.Repository<Roles>().Query()
                                        select new Roles
                                        {
                                            RoleId = u.RoleId,
                                            CompanyCode = u.CompanyCode,
                                            Description = u.Description,
                                            CreationDate = u.CreationDate,
                                            CreationUser = u.CreationUser,
                                            UpdateDate = u.UpdateDate,
                                            UpdateUser = u.UpdateUser
                                        }).ToListAsync();

            List<Roles> roles = _unitOfWork.Repository<Roles>().Query().Where(x => x.RoleId == 1).ToList();

            return roles;
        }
    }
}