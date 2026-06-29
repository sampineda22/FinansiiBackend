using CRM.GeneralDTOs;
using CRM.Models.General;
using Microsoft.Data.SqlClient;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
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
        public async Task<EntityResponse> SendEmailNewVendor(string companyCode)
        {
            try
            {
                using var httpClient = new HttpClient();
                SqlParameter[] parameters = { };
                string rutaImagen = "";

                EmailAccount account = _unitOfWork.Repository<EmailAccount>().GetSP<EmailAccount>("[dbo].[GetEmailAccount]", parameters).FirstOrDefault();
                RoutePath path = _unitOfWork.Repository<RoutePath>().Query().Where(x => x.Name == "Logos").FirstOrDefault();

                rutaImagen = $"{path.URL}/{companyCode}.jpg";

                byte[] imageBytes = await httpClient.GetByteArrayAsync(rutaImagen);

                using var imageStream = new MemoryStream(imageBytes);
                MailMessage message = new MailMessage
                {
                    From = new MailAddress(account.EmailAddress),
                    Subject = "Solicitud de Nuevo Proveedor"
                };

                message.To.Add("spineda@intermoda.com.hn");

                string htmlBody = @"
                <html>
                    <body style='text-align:center;'>
                        <img src='cid:LogoEmpresa' style='width:300px; height:100px;' />
                        <h2>Hola</h2>
                        <p>La siguiente imagen se cargó desde una URL:</p>
                        
                    </body>
                </html>";

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                    htmlBody,
                    null,
                    MediaTypeNames.Text.Html);

                LinkedResource imageResource = new LinkedResource(imageStream, "image/png")
                {
                    ContentId = "LogoEmpresa",
                    TransferEncoding = TransferEncoding.Base64
                };

                htmlView.LinkedResources.Add(imageResource);
                message.AlternateViews.Add(htmlView);

                using SmtpClient smtp = new SmtpClient("smtp.office365.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(account.EmailAddress, account.Password)
                };

                await smtp.SendMailAsync(message);
                
                return EntityResponse.CreateOk("");
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en SendEmailNewVendor: " + ex.Message);
            }
        }
    }
}
