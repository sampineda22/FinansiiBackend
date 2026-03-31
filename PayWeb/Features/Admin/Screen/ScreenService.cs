using Microsoft.Data.SqlClient;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Admin.Screen
{
    public class ScreenService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScreenService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<EntityResponse> getScreensByUser(string userId)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@userId", userId)
                };

                List<UserScreens> screens = _unitOfWork.Repository<UserScreens>().GetSP<UserScreens>("[Finansii].[GetScreensByUser]", parameters).ToList();

                /*if (screens.Count <= 0)
                {
                    return EntityResponse.CreateError("Error en método getScreensByUser: No se encontraron las pantallas del usuario.");
                }*/

                return EntityResponse.CreateOk(screens);
            }
            catch(Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }
    }
}
