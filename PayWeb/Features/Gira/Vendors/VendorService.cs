using CRM.Features.Gira.ExpensesDetails;
using CRM.Features.Gira.Historical;
using CRM.GeneralDTOs;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Threading.Tasks;

namespace CRM.Features.Gira.Vendors
{
    public class VendorService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VendorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<EntityResponse> SendEmailNewVendor()
        {
            try
            {
                bool isNewSequence = false;
                InvoiceSequence sequence = new();

                ExpenseDetail expenseDetail = _unitOfWorkGira.Repository<ExpenseDetail>().Query().Include(x => x.Status).Where(x => x.CompanyCode == detail.CompanyCode && x.InvoiceId == x.InvoiceId && x.VendAccount == detail.VendAccount && x.Status.Code != "R").FirstOrDefault();

                if (expenseDetail != null)
                {
                    return EntityResponse.CreateError("Se encontró un gasto con la misma factura y proveedor.");
                }

                ExpenseCategory category = _unitOfWorkGira.Repository<ExpenseCategory>().Query().Where(x => x.CompanyCode == detail.CompanyCode && x.Id == detail.ExpenseCategoryId).FirstOrDefault();

                /*var objects = ObjectDictionary.CreateObjectMap(new (string, object)[]
                {
                    (nameof(ExpenseCategory), category),
                    (nameof(ExpenseDetail), detail)
                });*/

                if ((category?.Name.ToLower()).Contains("alimentacion"))
                {
                    if (detail.MealId == null || detail.MealId == 0)
                    {
                        return EntityResponse.CreateError("No se pudo obtener el tipo de alimento. Favor validar que haya sido ingresado.");
                    }
                }
                else if ((category?.Name.ToLower()).Contains("combustible") && detail.CompanyCode == "IMGT")
                {
                    if (detail.FuelTypeId == null || detail.FuelTypeId == 0)
                    {
                        return EntityResponse.CreateError("No se pudo obtener el tipo de combustible. Favor validar que haya sido ingresado.");
                    }
                }

                Status status = _unitOfWorkGira.Repository<Status>().Query().Where(x => x.Code == "P").FirstOrDefault();
                detail.StatusId = status.Id;

                if (detail.InvoiceId == null || detail.InvoiceId.Replace(" ", "") == "")
                {
                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@CompanyCode", detail.CompanyCode),
                        new SqlParameter("@PersonalCode", detail.PersonalCode)
                    };

                    CostCenterByUser userInfo = _unitOfWork.Repository<CostCenterByUser>().GetSP<CostCenterByUser>("[Gira].[GetCostCenterDtosByUser]", parameters).FirstOrDefault();
                    sequence = _unitOfWorkGira.Repository<InvoiceSequence>().Query().Where(x => x.CompanyCode == detail.CompanyCode && x.Initials == userInfo.BusinessUnit).FirstOrDefault();

                    if (sequence == null)
                    {
                        InvoiceSequence newSequence = new()
                        {
                            CompanyCode = detail.CompanyCode,
                            Initials = userInfo.BusinessUnit,
                            SequenceNumber = 1,
                            CurrentSequence = $"{userInfo.BusinessUnit}{1:D4}"
                        };

                        /*_unitOfWorkGira.Repository<InvoiceSequence>().Add(newSequence);
                        await _unitOfWorkGira.SaveChangesAsync();*/
                        sequence = newSequence;
                    }
                    else
                    {
                        sequence.SequenceNumber++;
                        sequence.CurrentSequence = $"{userInfo.BusinessUnit}{sequence.SequenceNumber:D4}";
                        /*_unitOfWorkGira.Repository<InvoiceSequence>().Update(sequence);
                        await _unitOfWorkGira.SaveChangesAsync();*/
                    }

                    isNewSequence = true;
                    detail.InvoiceId = sequence.CurrentSequence;
                }

                string path = GetPath("GetGiraImg");

                if (string.IsNullOrEmpty(path))
                {
                    return EntityResponse.CreateError("No se pudo obtener la ruta en donde se guardará la información");
                }

                var fullPath = $"{path}/{detail.ImagePath}";

                detail.ImagePath = fullPath;

                _unitOfWorkGira.Repository<ExpenseDetail>().Add(detail);
                await _unitOfWorkGira.SaveChangesAsync();

                if (isNewSequence)
                {
                    if (sequence.SequenceNumber == 1)
                    {
                        _unitOfWorkGira.Repository<InvoiceSequence>().Add(sequence);
                    }
                    else
                    {
                        _unitOfWorkGira.Repository<InvoiceSequence>().Update(sequence);
                    }
                    await _unitOfWorkGira.SaveChangesAsync();
                }

                return EntityResponse.CreateOk(detail);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostExpenseDetail: " + ex.Message);
            }
        }
    }
}
