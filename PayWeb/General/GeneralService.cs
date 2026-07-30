using CRM.Features.Gira.Approve;
using CRM.General.GeneralDTOs;
using CRM.Infrastructure.Core;
using CRM.Models.General;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace CRM.General
{
    public class GeneralService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMCoreConnectionSettings _coreConnectionSettings;

        public GeneralService(IUnitOfWork unitOfWork, IOptions<IMCoreConnectionSettings> coreConnectionSettings)
        {
            _unitOfWork = unitOfWork;
            _coreConnectionSettings = coreConnectionSettings.Value;
        }
        
        public async Task<EntityResponse> SendEmail(string companyCode, string subject, string htmlBody, List<string> recipients, bool useCompanyLogo , List<Attachment> attachments = null)
        {
            try
            {
                using var httpClient = new HttpClient();

                SqlParameter[] parameters = { };

                EmailAccount account = _unitOfWork.Repository<EmailAccount>().GetSP<EmailAccount>("[dbo].[GetEmailAccount]", parameters).FirstOrDefault();
                
                MailMessage message = new()
                {
                    From = new MailAddress(account.EmailAddress),
                    Subject = subject
                };

                foreach (var email in recipients)
                    message.To.Add(email);

                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                        message.Attachments.Add(attachment);
                }

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                    htmlBody,
                    null,
                    MediaTypeNames.Text.Html);

                if(useCompanyLogo)
                {
                    RoutePath path = _unitOfWork.Repository<RoutePath>().Query().FirstOrDefault(x => x.Name == "Logos");

                    string rutaImagen = $"{path.URL}/{companyCode}.jpg";

                    byte[] imageBytes = null;

                    using var response = await httpClient.GetAsync(rutaImagen);

                    if (response.IsSuccessStatusCode)
                        imageBytes = await response.Content.ReadAsByteArrayAsync();

                    if (imageBytes != null)
                    {
                        LinkedResource logo = new(new MemoryStream(imageBytes), "image/png");

                        logo.ContentId = "LogoEmpresa";
                        logo.TransferEncoding = TransferEncoding.Base64;

                        htmlView.LinkedResources.Add(logo);
                    }
                }

                message.AlternateViews.Add(htmlView);
                using SmtpClient smtp = new("smtp.office365.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(
                        account.EmailAddress,
                        account.Password)
                };

                await smtp.SendMailAsync(message);

                return EntityResponse.CreateOk();
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public async Task<EntityResponse> EnviarNotificacionAsync(NotificationDto notification)
        {
            try
            {
                using HttpClient client = new HttpClient();

                /*var request = new NotificationDto
                {
                    Users = new List<string>
                {
                        "spineda"
                },
                    Title = "TEST DE NOTIF.",
                    Body = "PRUEBA DE NOTIFICACIONES",
                    Category = "expense",
                    Data = new Dictionary<string, string>
                    {
                        { "ExpenseId", "101" },
                        { "CompanyCode", "IMHN" },
                        { "ApprovedBy", "ADMIN" },
                        { "Status", "Approved" }
                    }
                };*/

                string json = System.Text.Json.JsonSerializer.Serialize(/*request*/ notification);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{_coreConnectionSettings.Url}Notifications/Send";

                HttpResponseMessage response = await client.PostAsync(url, content);

                string result = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                return EntityResponse.CreateOk(result);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en método EnviarNotificacionAsync: {ex.Message}");
            }
        }
    }
}
