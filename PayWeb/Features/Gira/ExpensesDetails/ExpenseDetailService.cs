using CRM.Features.Gira.Historical;
using CRM.GeneralDTOs;
using CRM.Infrastructure.Core;
using CRM.Models.General;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using ExpenseCategory = CRM.Features.Gira.ExpensesSettings.ExpenseCategory;
using Status = CRM.Features.Gira.Historical.Status;

namespace CRM.Features.Gira.ExpensesDetails
{
    public class ExpenseDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkGira _unitOfWorkGira;

        public ExpenseDetailService(IUnitOfWork unitOfWork, IUnitOfWorkGira unitOfWorkGira)
        {
            _unitOfWork = unitOfWork;
            _unitOfWorkGira = unitOfWorkGira;
        }
        public async Task<EntityResponse> PostExpenseDetail(ExpenseDetail detail)
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
                    if(detail.MealId == null || detail.MealId == 0)
                    {
                        return EntityResponse.CreateError("No se pudo obtener el tipo de alimento. Favor validar que haya sido ingresado.");
                    }
                }else if((category?.Name.ToLower()).Contains("combustible") && detail.CompanyCode == "IMGT")
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

                    if(sequence == null)
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

                if(isNewSequence)
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

        public string GetPath(string name)
        {
            try
            {
                RoutePath path = _unitOfWork.Repository<RoutePath>().Query().Where(x => x.Name == name).FirstOrDefault();
                return path.URL;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /*public string ValidateRulesByRequiredField<T>(object conditionType, object requireType, string companyCode, string projectCode)
        {
            try
            {
                List<Validation> validations = _unitOfWork.Repository<Validation>().Query().Where(x => x.CompanyCode == companyCode && x.ProjectCode == projectCode).ToList();

                foreach (Validation validation in validations)
                {
                    objects.TryGetValue(validation.ConditionType, out var conditionObject);
                    objects.TryGetValue(validation.RequiredType, out var requiredObject);

                    object conditionObject = validation.ConditionType switch
                    {
                        "Category" => category,
                        "Detail" => detail,
                        _ => null
                    };

                    object requiredObject = validation.RequiredType switch
                    {
                        "Category" => category,
                        "Detail" => detail,
                        _ => null
                    };

                    var conditionProperty = typeof(T).GetProperty(conditionField);
                    var requiredProperty = typeof(T).GetProperty(requiredField);

                    var conditionFieldValue = conditionProperty.GetValue(data)?.ToString();

                    if (conditionFieldValue.Contains(conditionValue, StringComparison.OrdinalIgnoreCase))
                    {
                        var requiredFieldValue = requiredProperty.GetValue(data)?.ToString();

                        if (string.IsNullOrEmpty(requiredFieldValue) || requiredFieldValue == "0")
                        {
                            return $"El campo {requiredField} es requerido.";
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }*/
    }
}
