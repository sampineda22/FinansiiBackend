using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.BankConfiguration
{
    public class BankConfigurationAppService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BankConfigurationAppService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public async Task<EntityResponse> AddBankConfigurationAsync(BankConfigurationDto bankConfigurationDto)
        {
            if (bankConfigurationDto == null)
            {
                return EntityResponse.CreateError("Los datos para crear la configuracion de cuentas no son validos.");
            }

            BankConfiguration bankConfiguration = new BankConfiguration
            {
                BankConfigurationId = bankConfigurationDto.BankConfigurationId,
                CompanyId = bankConfigurationDto.CompanyId,
                AccountId = bankConfigurationDto.AccountId,
                AccountNumber = bankConfigurationDto.AccountNumber,
                Host = bankConfigurationDto.Host,
                Port = bankConfigurationDto.Port,
                UserName = bankConfigurationDto.UserName,
                Password = bankConfigurationDto.Password,
                FileRoute = bankConfigurationDto.FileRoute,
                LocalFileRoute = bankConfigurationDto.LocalFileRoute,
                FileName = bankConfigurationDto.FileName
            };

            _unitOfWork.Repository<BankConfiguration>().Add(bankConfiguration);
            await _unitOfWork.SaveChangesAsync();
            bankConfigurationDto.BankConfigurationId = bankConfiguration.BankConfigurationId;
            return EntityResponse.CreateOk();
        }
        public async Task<List<BankConfigurationDto>> GetAll()
        {
            List<BankConfigurationDto> bankConfigurations = await (from u in _unitOfWork.Repository<BankConfiguration>().Query()
                                                           select new BankConfigurationDto
                                                           {
                                                               BankConfigurationId = u.BankConfigurationId,
                                                               CompanyId = u.CompanyId,
                                                               Bank = u.Bank,
                                                               AccountId = u.AccountId,
                                                               AccountNumber = u.AccountNumber,
                                                               Host = u.Host,
                                                               Port = u.Port,
                                                               UserName = u.UserName,
                                                               Password = u.Password,
                                                               FileRoute = u.FileRoute,
                                                               LocalFileRoute = u.LocalFileRoute,
                                                               FileName = u.FileName
                                                           }).ToListAsync();
            return bankConfigurations;
        }
        public BankConfigurationDto FindById(int id)
        {
            return (from u in _unitOfWork.Repository<BankConfiguration>().Query()
                    where u.BankConfigurationId == id
                    select new BankConfigurationDto
                    {
                        BankConfigurationId = u.BankConfigurationId,
                        CompanyId = u.CompanyId,
                        Bank = u.Bank,
                        AccountId = u.AccountId,
                        AccountNumber = u.AccountNumber,
                        Host = u.Host,
                        Port = u.Port,
                        UserName = u.UserName,
                        Password = u.Password,
                        FileRoute = u.FileRoute,
                        LocalFileRoute = u.LocalFileRoute,
                        FileName = u.FileName
                    }).FirstOrDefault();
        }
        public BankConfigurationDto FindByAccountId(string accountId)
        {
            return (from u in _unitOfWork.Repository<BankConfiguration>().Query()
                    where u.AccountId.Equals(accountId)
                    select new BankConfigurationDto
                    {
                        BankConfigurationId = u.BankConfigurationId,
                        CompanyId = u.CompanyId,
                        Bank = u.Bank,
                        AccountId = u.AccountId,
                        AccountNumber = u.AccountNumber,
                        Host = u.Host,
                        Port = u.Port,
                        UserName = u.UserName,
                        Password = u.Password,
                        FileRoute = u.FileRoute,
                        LocalFileRoute = u.LocalFileRoute,
                        FileName = u.FileName
                    }).FirstOrDefault();
        }
    }
}
