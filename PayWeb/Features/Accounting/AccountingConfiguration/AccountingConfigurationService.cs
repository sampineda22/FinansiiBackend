using CRM.Features.Accounting.BankConfiguration;
using CRM.Features.Accounting.BankStatementServiceAX;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Accounting.AccountingConfiguration
{
    public class AccountingConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountingConfigurationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public EntityResponse GetExceptionCodes(string companyCode)
        {
            try
            {
                List<ExceptionCode> codes = _unitOfWork.Repository<ExceptionCode>().Query().Where(x => x.CompanyCode == companyCode).ToList();

                if (codes == null)
                {
                    return EntityResponse.CreateError("No se pudieron obtener los códigos de excepción.");
                }

                return EntityResponse.CreateOk(codes);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo GetExceptionCodes: " + ex.ToString());
            }
        }

        public EntityResponse GetMT940Banks(string companyCode)
        {
            try
            {
                List <BankConfiguration.BankConfiguration> configurations = _unitOfWork.Repository<BankConfiguration.BankConfiguration>().Query().Where(x => x.CompanyId == companyCode).ToList();

                return EntityResponse.CreateOk(configurations);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo GetMT940Banks: " + ex.ToString());
            }
        }

        public async Task<EntityResponse> PostExceptionCode(ExceptionCode code)
        {
            EntityResponse response = new();

            try
            {
                ExceptionCode existingCode = _unitOfWork.Repository<ExceptionCode>().Query().Where(x => x.CompanyCode == code.CompanyCode && x.AccountId == code.AccountId && x.Code == code.Code).FirstOrDefault();

                if (existingCode != null)
                {
                    return EntityResponse.CreateError($"El código ya existe para la cuenta {existingCode.AccountId}. Validar los datos ingresados.");
                }

                _unitOfWork.Repository<ExceptionCode>().Add(code);
                await _unitOfWork.SaveChangesAsync();

                return EntityResponse.CreateOk(code);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo PostExceptionCode: " + ex.ToString());
            }
        }

        public async Task<EntityResponse> DeleteExceptionCode(string companyCode, string code, string accountId )
        {
            try
            {
                ExceptionCode existingCode = _unitOfWork.Repository<ExceptionCode>().Query().Where(x => x.CompanyCode == companyCode && x.AccountId == accountId && x.Code == code).FirstOrDefault();

                _unitOfWork.Repository<ExceptionCode>().Delete(existingCode);
                await _unitOfWork.SaveChangesAsync();
                return EntityResponse.CreateOk();

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        #region PaymentDates
        public EntityResponse GetPaymentDates(string companyCode)
        {
            EntityResponse response = new();

            try
            {
                List<PaymentDate> dates = _unitOfWork.Repository<PaymentDate>().Query().Where(x => x.CompanyCode == companyCode).ToList();

                if (dates == null)
                {
                    return EntityResponse.CreateError("No se pudieron obtener las fechas de pago.");
                }

                return EntityResponse.CreateOk(dates);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo GetPaymentDates: " + ex.ToString());
            }
        }

        public async Task<EntityResponse> PostPaymentDate(PaymentDate date)
        {
            EntityResponse response = new();

            try
            {
               PaymentDate paymentDate = _unitOfWork.Repository<PaymentDate>().Query().Where(x => x.CompanyCode == date.CompanyCode && x.Year == date.Year && x.Month == date.Month).FirstOrDefault();

                if(paymentDate != null)
                {
                    return EntityResponse.CreateError("Ya existe una parametrización con el mismo año y mes. Favor de validar los datos ingresados.");
                }

                _unitOfWork.Repository<PaymentDate>().Add(date);
                await _unitOfWork.SaveChangesAsync();

                return EntityResponse.CreateOk(date);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo PostPaymentDate: " + ex.ToString());
            }
        }

        public async Task<EntityResponse> DeleteDate(string companyCode, int year, int month)
        {
            try
            {
                PaymentDate paymentDate = _unitOfWork.Repository<PaymentDate>().Query().Where(x => x.CompanyCode == companyCode && x.Year == year && x.Month == month).FirstOrDefault();

                _unitOfWork.Repository<PaymentDate>().Delete(paymentDate);
                await _unitOfWork.SaveChangesAsync();
                return EntityResponse.CreateOk();

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }
        #endregion
    }
}
