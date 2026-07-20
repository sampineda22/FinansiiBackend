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
using System.Net.Mime;
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

        public ApproveService(IUnitOfWorkGira unitOfWorkGira, IUnitOfWork unitOfWork, IOptions<ProxyConnectionSettings> proxyConnectionSettings, GeneralService generalService)
        {
            _unitOfWorkGira = unitOfWorkGira;
            _unitOfWork = unitOfWork;
            _proxyConnectionSettings = proxyConnectionSettings.Value;
            _generalService = generalService;
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

                List<Status> statuses = _unitOfWorkGira.Repository<Status>().Query().ToList();

                if (String.IsNullOrEmpty(rejectionMotive))
                {
                    detail.PersonalCodeAdmin = personalCode;
                    detail.AXMessage = null;

                    EntityResponse response = CreateJournal(companyCode, detail.Id, user).Result;

                    if (!response.Ok)
                    {
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
                    detail.StatusId = statuses.Find(x => x.Code == ExpensesStatus.Status.RECHAZADO.ToString()).Id;
                    detail.PersonalCodeAdmin = personalCode;
                    detail.RejectionMotive = rejectionMotive;
                }

                if (String.IsNullOrEmpty(errorMessage))
                    detail.StatusId = statuses.Find(x => x.Code == ExpensesStatus.Status.APROBADO.ToString()).Id;

                _unitOfWorkGira.Repository<ExpenseDetail>().Update(detail);
                await _unitOfWorkGira.SaveChangesAsync();

                if(!String.IsNullOrEmpty(errorMessage))
                {
                    return EntityResponse.CreateError(errorMessage);
                }

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

                JOURNALLINE data = _unitOfWork.Repository<JOURNALLINE>().GetSP<JOURNALLINE>("[Gira].[CreateJournalLineForExpense]", parameters).FirstOrDefault();

                if (data == null)
                {
                    return EntityResponse.CreateError($"No se pudo obtener la linea del diario. Favor validar el sp [Gira].[CreateJournalLineForExpense] con el id: {id}");
                }

                data.USERID = data.USERID.ToUpper();

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
                    EntityResponse res = await UpdateCAI(detail, result);

                    if (!res.Ok)
                    {
                        return EntityResponse.CreateError(res.Mensaje);
                    }

                    return EntityResponse.CreateError($"{result}. Se realizó la solicitud de actualización del CAI a los correspondientes. Favor esperar a que se actualice.");
                }
                else if (result.Contains("LD"))
                {
                    detail.JournalNum = result.Replace("\"", "");
                }
                else if (result.Contains("OK"))
                {
                    return EntityResponse.CreateOk("Se creó el diario pero no se pudo almacenar el número de diario.");

                } else if (result.Contains("\"errors\""))
                {
                    var json = JsonDocument.Parse(result);

                    if (json.RootElement.TryGetProperty("errors", out JsonElement errors))
                    {
                        string jsonError = "";
                        foreach (var error in errors.EnumerateObject())
                        {
                            string campo = error.Name;
                            string mensaje = error.Value[0].GetString();

                            jsonError = $"{campo}: {mensaje}";
                        }
                        return EntityResponse.CreateError(jsonError);
                    }
                }
                else
                {
                    return EntityResponse.CreateError(result);
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
                string html = "", htmlGroup = "", htmlSerie = "";
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
                
                if(detail.CompanyCode != "IMCR")
                {
                    htmlGroup = $@"
                    <tr>
                    <td><b>Grupo</b></td>
                    <td>Comercio Nacional</td>
                    </tr>";

                }else if(detail.CompanyCode == "IMGT")
                {
                    htmlSerie = $@"
                    <tr>
                    <td><b>No. Serie</b></td>
                    <td>{detail.SeriesNum}</td>
                    </tr>";
                }

                html = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                    <meta charset='UTF-8'>
                    </head>

                    <body style='margin:0;padding:30px;background:#f4f6f9;font-family:Segoe UI,Arial,sans-serif;'>

                    <table width='100%' cellspacing='0' cellpadding='0'>
                    <tr>
                    <td align='center'>

                    <table width='650' cellspacing='0' cellpadding='0'
                    style='background:#ffffff;border-radius:8px;border:1px solid #dddddd;'>

                    <tr>
                    <td align='center' style='padding:30px;'>

                    <img src='cid:LogoEmpresa'
                         style='max-width:220px;height:auto;' />

                    </td>
                    </tr>

                    <tr>
                    <td style='padding:35px;'>

                    <h2 style='margin-top:0;color:#003366;'>
                    Actualización de Proveedor
                    </h2>

                    <table width='100%' cellpadding='8' cellspacing='0'
                    style='border-collapse:collapse;font-size:14px;'>

                    <tr style='background:#f7f7f7'>
                    <td width='35%'><b>Código de proveedor</b></td>
                    <td>{detail.VendAccount}</td>
                    </tr>

                    {htmlSerie}

                    <tr style='background:#f7f7f7'>
                    <td><b>No. Factura</b></td>
                    <td>{detail.InvoiceId}</td>
                    </tr>

                    {htmlGroup}

                    <tr style='background:#f7f7f7'>
                    <td><b>Divisa</b></td>
                    <td>{data.Currency}</td>
                    </tr>

                    <tr>
                    <td><b>Fecha Documento</b></td>
                    <td>{detail.InvoiceDate:dd/MM/yyyy}</td>
                    </tr>

                    <tr style='background:#fff3cd'>
                    <td><b>Descripción</b></td>
                    <td style='color:#b02a37;font-weight:bold;'>
                    {response}
                    </td>
                    </tr>

                    <tr>
                    <td><b>Solicitante</b></td>
                    <td>{data.Name}</td>
                    </tr>

                    <tr style='background:#f7f7f7'>
                    <td><b>Correo</b></td>
                    <td>{data.Email}</td>
                    </tr>

                    </table>

                    </td>
                    </tr>

                    <tr>
                    <td align='center'
                    style='background:#003366;color:white;padding:15px;font-size:12px;'>

                    Este correo fue generado automáticamente.<br/>
                    Favor no responder.

                    </td>
                    </tr>

                    </table>

                    </td>
                    </tr>
                    </table>

                    </body>
                    </html>";

                if (!string.IsNullOrWhiteSpace(detail.ImagePath))
                {
                    using HttpClient httpClient = new();
                    using HttpResponseMessage responseImage = await httpClient.GetAsync(detail.ImagePath);

                    responseImage.EnsureSuccessStatusCode();

                    imageBytes = await responseImage.Content.ReadAsByteArrayAsync();

                    string mediaType = responseImage.Content.Headers.ContentType?.MediaType
                                       ?? "application/octet-stream";

                    string extension = mediaType switch
                    {
                        "image/jpeg" => ".jpg",
                        "image/png" => ".png",
                        "image/gif" => ".gif",
                        "image/bmp" => ".bmp",
                        _ => Path.GetExtension(detail.ImagePath)
                    };

                    MemoryStream stream = new(imageBytes);
                    stream.Position = 0;

                    Attachment attachment = new(
                        stream,
                        $"ActualizacionCAI_{detail.VendAccount}{extension}",
                        mediaType);

                    attachment.TransferEncoding = TransferEncoding.Base64;

                    attachments.Add(attachment);
                }

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
