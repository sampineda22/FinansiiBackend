using Azure;
using CRM.Features.Admin.Users;
using CRM.Features.Gira.AXExpenses;
using CRM.Features.Gira.Historical;
using CRM.General;
using CRM.General.GeneralDTOs;
using CRM.Infrastructure.Core;
using CRM.Infrastructure.Enum;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ExpenseAccount = CRM.Features.Gira.ExpensesSettings.ExpenseAccount;
using Status = CRM.Features.Gira.Historical.Status;

namespace CRM.Features.Gira.Approve
{
    public class ApproveService
    {
        private readonly IUnitOfWorkGira _unitOfWorkGira;
        private readonly IUnitOfWork _unitOfWork;
        private readonly GeneralService _generalService;
        private readonly ProxyConnectionSettings _proxyConnectionSettings;
        private readonly IHttpClientFactory _factory;

        public ApproveService(IUnitOfWorkGira unitOfWorkGira, IUnitOfWork unitOfWork, IOptions<ProxyConnectionSettings> proxyConnectionSettings, GeneralService generalService, IHttpClientFactory factory)
        {
            _unitOfWorkGira = unitOfWorkGira;
            _unitOfWork = unitOfWork;
            _proxyConnectionSettings = proxyConnectionSettings.Value;
            _generalService = generalService;
            _factory = factory;
        }

        public async Task<EntityResponse> GetPendingApprovals(string companyCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@companyCode", companyCode),
                    new SqlParameter("@code", "P")
                };

                List<ExpenseDetailDto> details = _unitOfWork.Repository<ExpenseDetailDto>().GetSP<ExpenseDetailDto>("[Gira].[GetExpensesDetails]", parameters).ToList();

                return EntityResponse.CreateOk(details);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetPendingApprovals: " + ex.Message);
            }
        }

        public async Task<EntityResponse> UpdateStatus(string companyCode, int id, string rejectionMotive, string personalCode, string user)
        {
            EntityResponse entityResponse = new();
            string errorMessage = "";

            try
            {
                ExpenseDetail detail = _unitOfWorkGira.Repository<ExpenseDetail>().Query().Include(x => x.ExpenseCategory).ThenInclude(e => e.ExpenseType).Include(x => x.FuelType).Include(x => x.Status)
                                              .Where(x => x.CompanyCode == companyCode
                                                       && x.Id == id).FirstOrDefault();

                if (detail.InUse)
                {
                    return EntityResponse.CreateError("El detalle del gasto esta en uso. Favor esperar a que se deje de utilizar para poder aprobar/rechazar.");
                }

                ExpenseAccount expenseAccount = _unitOfWorkGira.Repository<ExpenseAccount>().Query().Where(x => x.IdExpenseCategory == detail.ExpenseCategoryId && x.CompanyCode == companyCode).FirstOrDefault();

                if (expenseAccount == null || expenseAccount?.AccountId == "")
                {
                    return EntityResponse.CreateError($"No se encontró una cuenta de contrapartida asignada a la categoria {detail.ExpenseCategory.Name}");
                }

                if (String.IsNullOrEmpty(rejectionMotive))
                {
                    List<Status> statuses = _unitOfWorkGira.Repository<Status>().Query().ToList();

                    detail.StatusId = statuses.Find(x => x.Code == ExpensesStatus.Status.APROBADO.ToString()).Id;
                    detail.PersonalCodeAdmin = personalCode;
                    detail.AXMessage = null;

                    EntityResponse response = CreateJournal(companyCode, detail.Id, user).Result;

                    if (!response.Ok)
                    {
                        //detail.StatusId = !response.Mensaje.Contains("LD-") ? statuses.Find(x => x.Code == ExpensesStatus.Status.PENDIENTEAX.ToString()).Id : detail.StatusId;
                        detail.AXMessage = response.Mensaje;
                        errorMessage = $"{response.Mensaje}";
                    }

                    if (response is EntityResponse<ExpenseDetail> genericResponse)
                    {
                        detail.JournalNum = genericResponse.Data.JournalNum;
                    }
                    else if (response is EntityResponse<string> genericResponse2)
                    {
                        detail.StatusId = statuses.Find(x => x.Code == ExpensesStatus.Status.PENDIENTEAX.ToString()).Id;
                        detail.AXMessage = genericResponse2.Data.ToString();
                        errorMessage = $"{genericResponse2.Data.ToString()}";
                    }
                }
                else if (rejectionMotive != "")
                {
                    Status status = _unitOfWorkGira.Repository<Status>().Query().Where(x => x.Code == ExpensesStatus.Status.RECHAZADO.ToString()).FirstOrDefault();

                    detail.StatusId = status.Id;
                    detail.PersonalCodeAdmin = personalCode;
                    detail.RejectionMotive = rejectionMotive;
                }

                _unitOfWorkGira.Repository<ExpenseDetail>().Update(detail);
                await _unitOfWorkGira.SaveChangesAsync();

                if(!String.IsNullOrEmpty(errorMessage))
                    return EntityResponse.CreateError(errorMessage);

                return EntityResponse.CreateOk(detail);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en UpdateStatus: " + ex.Message);
            }
        }

        public async Task<EntityResponse> CreateJournal(string companyCode, int id, string user)
        {
            try
            {
                ExpenseDetail detail = _unitOfWorkGira.Repository<ExpenseDetail>().Query().Include(x => x.ExpenseCategory).ThenInclude(e => e.ExpenseType).Include(x => x.FuelType).Include(x => x.Status)
                                              .Where(x => x.CompanyCode == companyCode && x.Id == id).FirstOrDefault();

                if (!String.IsNullOrEmpty(detail.JournalNum))
                {
                    return EntityResponse.CreateError($"El gasto se encuentra ingresado en el diario {detail.JournalNum}. Favor validar si el gasto ya existe en AX.");
                }

                string mealName = detail.MealId == null || detail?.MealId == 0 ? null : Enum.GetName(typeof(Meals.MealsType), detail.MealId);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@id", id),
                    new SqlParameter("@companyCode", companyCode),
                    new SqlParameter("@mealName", (object?)mealName ?? DBNull.Value)
                };

                ExpenseAXModel data = _unitOfWork.Repository<ExpenseAXModel>().GetSP<ExpenseAXModel>("[Gira].[CreateJournalLineForExpense]", parameters).FirstOrDefault();

                if (data == null)
                {
                    return EntityResponse.CreateError($"No se pudo obtener la linea del diario. Favor validar el sp [Gira].[CreateJournalLineForExpense] con el id: {id}");
                }

                data.USERID = data.USERID.ToUpper();

                /*var client = new RestClient();
                var request = new RestRequest($"{_proxyConnectionSettings.Url}api/Gira/GiraJournalLine/{companyCode}/{user}", Method.Post)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader("Content-type", "application/json; charset=utf-8");
                request.AddParameter("application/json", Newtonsoft.Json.JsonConvert.SerializeObject(data), ParameterType.RequestBody);
                var response = client.Execute(request);*/

                using var client = new HttpClient();

                var url = $"{_proxyConnectionSettings.Url}api/Gira/GiraJournalLine/{companyCode}/{user}";

                var response = await client.PostAsJsonAsync(url, data);

                var result = await response.Content.ReadAsStringAsync();

                if (result.Contains("ErrorSystem") || result.Contains("Error"))
                {
                    using JsonDocument doc = JsonDocument.Parse(result);
                    string message = doc.RootElement.GetProperty("Message").GetString();

                    return EntityResponse.CreateError(message);

                }else if (result.Contains("Cai"))
                {
                    var resp = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(result);
                    EntityResponse res = await UpdateCAI(detail, result);

                    if (!res.Ok)
                    {
                        return EntityResponse.CreateError(res.Mensaje);
                    }

                    return EntityResponse.CreateError($"{resp}. Se realizó la solicitud de actualización del CAI a los correspondientes. Favor esperar a que se actualicé");
                }
                else if (result.Contains("LD"))
                {
                    detail.JournalNum = result.Replace("\"", "");
                }
                else if (result.Contains("OK"))
                {
                    return EntityResponse.CreateOk("Se creó el diario pero no se pudo almacenar el número de diario.");
                }
                else
                {
                    return EntityResponse.CreateError($"{response.Content}");
                }
                    return EntityResponse.CreateOk(detail);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en CreateJournal: " + ex.Message);
            }
        }

        public async Task<EntityResponse> UpdateCAI(ExpenseDetail detail, string response)
        {
            try
            {
                SqlParameter[] parameters = { };
                byte[] imageBytes = null;
                string html = "";
                var attachments = new List<Attachment>();

                parameters = new SqlParameter[]
                 {
                     new SqlParameter("@companyCode", detail.CompanyCode),
                     new SqlParameter("@personalCode", detail.PersonalCode)
                 };

                AgentCurrency data = _unitOfWork.Repository<AgentCurrency>().GetSP<AgentCurrency>("[Gira].[GetRequesterAndCurrency]", parameters).FirstOrDefault();

                if (data.Email == null)
                {
                    return EntityResponse.CreateError("Error en método UpdateCAI: No se encontró la información del asesor y de la moneda.");
                }

                parameters = new SqlParameter[]
                {
                    new SqlParameter("@personalCode", detail.PersonalCode),
                    new SqlParameter("@companyCode", detail.CompanyCode),
                    new SqlParameter("@email", data.Email)
                };

                StringResponse emails = _unitOfWork.Repository<StringResponse>().GetSP<StringResponse>("[Gira].[GetEmailsForCAI]", parameters).FirstOrDefault();

                if(detail.CompanyCode == "IMGT")
                {
                    html = $@"
                    <html>
                        <body style='text-align:center;'>
                            <img src='cid:LogoEmpresa' style='width:300px; height:100px;' />
                            <h2>Actualización de Proveedor</h2>
                            <p><b>Código de Proveedor: </b>{detail.VendAccount}</p>
                            <p><b>No. de Seria: </b>{detail.SeriesNum}</p>
                            <p><b>No. Factura: </b>{detail.InvoiceId}</p>
                            <p><b>Grupo: </b>Comercio Nacional</p>
                            <p><b>Divisa: </b>{data.Currency}</p>
                            <p><b>Fecha de Documento: </b>{detail.InvoiceDate}</p>
                            <p><b>Descripción: </b>{response}</p>
                            <p><b>Solicitante: </b>{data.Name}</p>
                            <p><b>Correo de Solicitante: </b>{data.Email}</p>
                        </body>
                    </html>";
                }
                else
                {
                    html = $@"
                    <html>
                        <body style='text-align:center;'>
                            <img src='cid:LogoEmpresa' style='width:300px; height:100px;' />
                            <h2>Actualización de Proveedor</h2>
                            <p><b>Código de Proveedor: </b>{detail.VendAccount}</p>
                            <p><b>No. Factura: </b>{detail.InvoiceId}</p>
                            <p><b>Grupo: </b>Comercio Nacional</p>
                            <p><b>Divisa: </b>{data.Currency}</p>
                            <p><b>Fecha de Documento: </b>{detail.InvoiceDate}</p>
                            <p><b>Descripción: </b>{response}</p>
                            <p><b>Solicitante: </b>{data.Name}</p>
                            <p><b>Correo de Solicitante: </b>{data.Email}</p>
                        </body>
                    </html>";
                }

                if (!string.IsNullOrWhiteSpace(detail.ImagePath))
                {
                    using (WebClient webClient = new WebClient())
                    {
                        imageBytes = webClient.DownloadData(detail.ImagePath);

                        var stream = new MemoryStream(imageBytes);

                        attachments.Add(new Attachment(stream, $"Actualización CAI - {detail.VendAccount}")
                        {
                            ContentType =
                            {
                                MediaType = "image/jpeg"
                            }
                        });
                    }
                }

                emails.Value = "spineda@intermoda.com.hn,gmeza@intermoda.com.hn";
                return await _generalService.SendEmail(detail.CompanyCode,
                                                       "Solicitud de Actualización de CAI",
                                                       html,
                                                       emails.Value.Split(',').ToList(),
                                                       true,
                                                       attachments);
            }
            catch(Exception ex)
            {
                return EntityResponse.CreateError("Error en método UpdateCAI: " + ex.Message);
            }
        }
    }
}
