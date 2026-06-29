using CRM.Features.Gira.Historical;
using CRM.GeneralDTOs;
using CRM.Infrastructure.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Gira.ExpensesSettings
{
    public class ExpensesSettingsService
    {
        private readonly IUnitOfWorkGira _unitOfWorkGira;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<User> _passwordHasher;

        public ExpensesSettingsService(IUnitOfWorkGira unitOfWorkGira, IUnitOfWork unitOfWork)
        {
            _unitOfWorkGira = unitOfWorkGira;
            _unitOfWork = unitOfWork;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<EntityResponse> GetExpensesType(string companyCode)
        {
            try
            {
                List<ExpenseType> types = _unitOfWorkGira.Repository<ExpenseType>().Query().Where(x => x.CompanyCode == companyCode).ToList();

                return EntityResponse.CreateOk(types);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetExpensesType: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetExpensesCategories(string companyCode)
        {
            try
            {
                List<ExpenseCategory> types = _unitOfWorkGira.Repository<ExpenseCategory>().Query().Include(x => x.ExpenseType).Where(x => x.CompanyCode == companyCode).ToList();

                return EntityResponse.CreateOk(types);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetExpensesCategories: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetExpensesAccounts(string companyCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@CompanyCode", companyCode)
                };

                List<ExpenseAccountDto> dto = _unitOfWork.Repository<ExpenseAccountDto>().GetSP<ExpenseAccountDto>("[Gira].[GetExpensesAccounts]", parameters).ToList();

                return EntityResponse.CreateOk(dto);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetExpensesAccounts: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetActiveCostCenters(string companyCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@CompanyCode", companyCode)
                };

                List<CostCenterDto> dto = _unitOfWork.Repository<CostCenterDto>().GetSP<CostCenterDto>("[Gira].[GetActiveCostCenters]", parameters).ToList();

                return EntityResponse.CreateOk(dto);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetActiveCostCenters: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetMainAccounts()
        {
            try
            {
                SqlParameter[] parameters = { };

                List<MainAccountDto> dto = _unitOfWork.Repository<MainAccountDto>().GetSP<MainAccountDto>("[Gira].[GetMainAccounts]", parameters).ToList();

                return EntityResponse.CreateOk(dto);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetMainAccounts: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetUsers(string companyCode)
        {
            try
            {
                SqlParameter[] parameters = 
                {
                    new SqlParameter("@CompanyCode", companyCode)
                };

                List<UserDto> users = _unitOfWork.Repository<UserDto>().GetSP<UserDto>("[Gira].[GetUsers]", parameters).ToList();

                return EntityResponse.CreateOk(users);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetUsers: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetEmployees(string companyCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@CompanyCode", companyCode),
                    new SqlParameter("@CategoryCode", ""),
                    new SqlParameter("@PositionCode", "")
                };

                List<Employee> employees = _unitOfWork.Repository<Employee>().GetSP<Employee>("[Finansii].[GetEmployeesByPosition]", parameters).ToList();

                return EntityResponse.CreateOk(employees);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetMainAccounts: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetFuelTypes(string companyCode)
        {
            try
            {
                List<FuelType> types = _unitOfWorkGira.Repository<FuelType>().Query().Where(x => x.CompanyCode == companyCode).ToList();
                return EntityResponse.CreateOk(types);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetFuelTypes: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostStatus(ExpenseType expenseType)
        {
            List<ExpenseCategory> categories = _unitOfWorkGira.Repository<ExpenseCategory>().Query().Where(x => x.CompanyCode == expenseType.CompanyCode && x.IdExpenseType == expenseType.Id && x.Status == true).ToList();

            if (categories.Count > 0 && expenseType.State == false)
            {
                return EntityResponse.CreateError($"No se puede desactivar el tipo de gasto ya que se encuentra asignado a {categories.Count} categoria(s)");
            }

            try
            {
                _unitOfWorkGira.Repository<ExpenseType>().Update(expenseType);
                await _unitOfWorkGira.SaveChangesAsync();
                return EntityResponse.CreateOk(expenseType);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostStatus: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostStatusCategory(ExpenseCategory expenseCategory)
        {
            try
            {
                _unitOfWorkGira.Repository<ExpenseCategory>().Update(expenseCategory);
                await _unitOfWorkGira.SaveChangesAsync();
                return EntityResponse.CreateOk(expenseCategory);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostStatusCategory: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostPutExpenseType(ExpenseType expenseType, string companyCode)
        {
            try
            {
                ExpenseType type = _unitOfWorkGira.Repository<ExpenseType>().Query().Where(x => x.CompanyCode == expenseType.CompanyCode && x.Name == expenseType.Name && x.Journal == expenseType.Journal && x.Id != expenseType.Id).FirstOrDefault();

                if (type != null)
                {
                    return EntityResponse.CreateError("Se encontró una configuración con las mismas caracteristicas. Validar que la información ingresada sea la correcta");
                }

                expenseType.Journal = expenseType.Journal.Replace(" ", "");

                if (expenseType.Id == 0)
                {
                    expenseType.State = true;
                    expenseType.CompanyCode = companyCode;

                    _unitOfWorkGira.Repository<ExpenseType>().Add(expenseType);
                    await _unitOfWorkGira.SaveChangesAsync();
                    return EntityResponse.CreateOk(expenseType);
                }

                _unitOfWorkGira.Repository<ExpenseType>().Update(expenseType);
                await _unitOfWorkGira.SaveChangesAsync();
                return EntityResponse.CreateOk(expenseType);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostPutExpenseType: " + ex.Message);
            }
        }

        public async Task<EntityResponse> GetTaxGroups(string companyCode)
        {
            try
            {
                List<TaxGroup> groups = _unitOfWorkGira.Repository<TaxGroup>().Query().Where(x => x.CompanyCode == companyCode).ToList();

                return EntityResponse.CreateOk(groups);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetExpensesCategories: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostPutExpenseCategory(ExpenseCategory expenseCategory, string companyCode)
        {
            try
            {
                ExpenseCategory category = _unitOfWorkGira.Repository<ExpenseCategory>().Query().Where(x => x.CompanyCode == expenseCategory.CompanyCode && x.Name == expenseCategory.Name
                                                                                                         && x.VendAccount == expenseCategory.VendAccount
                                                                                                         && x.IsInvoiceRequired == expenseCategory.IsInvoiceRequired && x.IsImageRequired == expenseCategory.IsImageRequired
                                                                                                         && x.IsDescriptionRequired == expenseCategory.IsDescriptionRequired && x.Id != expenseCategory.Id).FirstOrDefault();

                if (category != null)
                {
                    return EntityResponse.CreateError("Se encontró una configuración con las mismas caracteristicas. Validar que la información ingresada sea la correcta");
                }

                if (expenseCategory.Id == 0)
                {
                    expenseCategory.Status = true;
                    expenseCategory.CompanyCode = companyCode;

                    _unitOfWorkGira.Repository<ExpenseCategory>().Add(expenseCategory);
                    await _unitOfWorkGira.SaveChangesAsync();
                    return EntityResponse.CreateOk(expenseCategory);
                }

                _unitOfWorkGira.Repository<ExpenseCategory>().Update(expenseCategory);
                await _unitOfWorkGira.SaveChangesAsync();
                return EntityResponse.CreateOk(expenseCategory);

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostPutExpenseCategory: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostPutExpenseAccount(ExpenseAccount expense, string companyCode)
        {
            try
            {
                ExpenseAccount e = _unitOfWorkGira.Repository<ExpenseAccount>().Query().AsNoTracking().Where(x => x.CompanyCode == companyCode && x.IdExpenseType == expense.IdExpenseType
                                                                                                         && x.IdExpenseCategory == expense.IdExpenseCategory && x.AccountId == expense.AccountId
                                                                                                         && x.Id != expense.Id).FirstOrDefault();

                if (e != null)
                {
                    SqlParameter[] parameters = { };

                    List<MainAccountDto> accountDtos = _unitOfWork.Repository<MainAccountDto>().GetSP<MainAccountDto>("[Gira].[GetMainAccounts]", parameters).ToList();
                    string mainAccountName = accountDtos.Find(x => x.MainAccountId == expense.AccountId).Name;

                    ExpenseType type = _unitOfWorkGira.Repository<ExpenseType>().Query().AsNoTracking().Where(x => x.CompanyCode == companyCode && x.Id == expense.IdExpenseType).FirstOrDefault();

                    return EntityResponse.CreateError($"Ya existe una asignación de la cuenta {mainAccountName} al tipo de gasto {type.Name}.");
                }

                expense.CompanyCode = companyCode;

                if (expense.Id != 0)
                {
                    _unitOfWorkGira.Repository<ExpenseAccount>().Update(expense);
                    await _unitOfWorkGira.SaveChangesAsync();
                    return EntityResponse.CreateOk(expense);
                }

                _unitOfWorkGira.Repository<ExpenseAccount>().Add(expense);
                await _unitOfWorkGira.SaveChangesAsync();

                return EntityResponse.CreateOk(expense);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostPutExpenseAccount: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostPutTaxGroup(TaxGroup taxGroup, string companyCode)
        {
            try
            {
                taxGroup.CompanyCode = companyCode;

                List<TaxGroup> taxGroups = _unitOfWorkGira.Repository<TaxGroup>().Query().AsNoTracking().Where(x => x.CompanyCode == companyCode
                                                                                                                 && x.GrupoImpuestoExento == taxGroup.GrupoImpuestoExento
                                                                                                                 && x.GrupoImpuestoGravado == taxGroup.GrupoImpuestoGravado
                                                                                                                 && x.GrupoImpuestoArticuloGravado == taxGroup.GrupoImpuestoArticuloGravado
                                                                                                                 && x.GrupoImpuestoArticuloExento == taxGroup.GrupoImpuestoArticuloExento
                                                                                                                 && x.Id != taxGroup.Id).ToList();

                if (taxGroups.Count > 0)
                {
                    return EntityResponse.CreateError("Se encontró un grupo de impuestos con la misma configuración. Favor de validar que la configuración deseada no exista.");
                }

                taxGroup.CompanyCode = companyCode;

                if (taxGroup.Id != 0)
                {
                    _unitOfWorkGira.Repository<TaxGroup>().Update(taxGroup);
                    await _unitOfWorkGira.SaveChangesAsync();
                    return EntityResponse.CreateOk(taxGroup);
                }

                _unitOfWorkGira.Repository<TaxGroup>().Add(taxGroup);
                await _unitOfWorkGira.SaveChangesAsync();
                return EntityResponse.CreateOk(taxGroup);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostPutTaxGroup: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostPutUser(User user, string companyCode)
        {
            try
            {
                List<User> users = _unitOfWorkGira.Repository<User>().Query().Where(x => x.CompanyCode == companyCode && x.Username == user.Username && x.IsActive== true && x.Id != user.Id).ToList();
                string temporaryPassword = user.PasswordHash;

                if (users.Count > 0)
                {
                    return EntityResponse.CreateError($"El username ya esta siendo utilizado. Favor ingresar uno diferente");
                }

                users = _unitOfWorkGira.Repository<User>().Query().Where(x => x.CompanyCode == companyCode && x.PersonalCode == user.PersonalCode && x.IsActive == true && x.Id != user.Id).ToList();

                if (users.Count > 0)
                {
                    return EntityResponse.CreateError($"El colaborador ya contiene un username.");
                }

                user.CompanyCode = companyCode;

                if(user.Id != 0)
                {
                    _unitOfWorkGira.Repository<User>().Update(user);
                    await _unitOfWorkGira.SaveChangesAsync();
                    return EntityResponse.CreateOk(user);
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, temporaryPassword);

                _unitOfWorkGira.Repository<User>().Add(user);
                await _unitOfWorkGira.SaveChangesAsync();
                return EntityResponse.CreateOk(user);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostPutUser: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostUserState(User user)
        {
            try
            {
                try
                {
                    _unitOfWorkGira.Repository<User>().Update(user);
                    await _unitOfWorkGira.SaveChangesAsync();
                    return EntityResponse.CreateOk(user);
                }
                catch (Exception ex)
                {
                    return EntityResponse.CreateError("Error en PostUserState: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en PostUserState: " + ex.Message);
            }
        }

        public async Task<EntityResponse> ResetPassword(int id, string newPassword, string companyCode)
        {
            try
            {
                try
                {
                    User user = _unitOfWorkGira.Repository<User>().Query().Where(x => x.CompanyCode == companyCode && x.IsActive == true && x.Id == id).FirstOrDefault();
                    string newHash = _passwordHasher.HashPassword(user, newPassword);

                    user.PasswordHash = newHash;

                    _unitOfWorkGira.Repository<User>().Update(user);
                    await _unitOfWorkGira.SaveChangesAsync();
                    return EntityResponse.CreateOk(user);
                }
                catch (Exception ex)
                {
                    return EntityResponse.CreateError("Error en ResetPassword: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en ResetPassword: " + ex.Message);
            }
        }

        public async Task<EntityResponse> DeleteExpenseAccount(int id, string companyCode)
        {
            try
            {
                ExpenseAccount expense = _unitOfWorkGira.Repository<ExpenseAccount>().Query().Where(x => x.Id == id).FirstOrDefault();

                _unitOfWorkGira.Repository<ExpenseAccount>().Delete(expense);
                await _unitOfWorkGira.SaveChangesAsync();
                return EntityResponse.CreateOk();
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en DeleteExpenseAccount: " + ex.Message);
            }
        }
    }
}
