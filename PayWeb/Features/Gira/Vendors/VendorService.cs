using CRM.Features.Gira.AXExpenses;
using CRM.General;
using CRM.General.GeneralDTOs;
using CRM.Infrastructure.Core;
using Microsoft.Data.SqlClient;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CRM.Features.Gira.Vendors
{
    public class VendorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkGira _unitOfWorkGira;
        private readonly GeneralService _generalService;

        public VendorService(IUnitOfWork unitOfWork, IUnitOfWorkGira unitOfWorkGira, GeneralService generalService)
        {
            _unitOfWork = unitOfWork;
            _unitOfWorkGira = unitOfWorkGira;
            _generalService = generalService;
        }
        public async Task<EntityResponse> SendEmailNewVendor(string companyCode, VendorRequest vendorRequest)
        {
            try
            {
                SqlParameter[] parameters = { };
                var attachments = new List<Attachment>();

                EVACompany company = _unitOfWorkGira.Repository<EVACompany>().GetSP<EVACompany>("[Gira].[GetEVAEmpresas]", parameters).Where(x => x.EmpresaId == companyCode).FirstOrDefault();
                company.DocumentoFiscal = string.IsNullOrEmpty(company.DocumentoFiscal) ? "Documento Fiscal" : company.DocumentoFiscal;

                parameters = new SqlParameter[]
                {
                  new SqlParameter("@companyCode",companyCode),
                  new SqlParameter("@personalCode",vendorRequest.RequesterCode)
                };
                AgentCurrency user = _unitOfWork.Repository<AgentCurrency>().GetSP<AgentCurrency>("[Gira].[GetRequesterAndCurrency]", parameters).FirstOrDefault();

                if (user.Email == null)
                {
                    return EntityResponse.CreateError("Error en método SendEmailNewVendor: No se encontró la información del usuario.");
                }

                parameters = new SqlParameter[]
                {
                    new SqlParameter("@personalCode", vendorRequest.RequesterCode),
                    new SqlParameter("@companyCode", companyCode),
                    new SqlParameter("@email", user.Email)
                };
                StringResponse emails = _unitOfWork.Repository<StringResponse>().GetSP<StringResponse>("[Gira].[GetEmailsForCAI]", parameters).FirstOrDefault();

                emails.Value = "spineda@intermoda.com.hn,gmeza@intermoda.com.hn";
                string html = $@"
                <html>
                    <body style='text-align:center;'>
                        <img src='cid:LogoEmpresa' style='width:300px; height:100px;' />
                        <h2>Solicitud de Proveedor: {vendorRequest.VendorName}</h2>
                        <p><b>{company.DocumentoFiscal}: </b>{vendorRequest.RTN}</p>
                        <p><b>Grupo: </b>Comercio Nacional</p>
                        <p><b>Divisa: </b>{user.Currency}</p>
                        <p><b>Solicitante: </b>{user.Name}</p>
                        <p><b>Correo de Solicitante: </b>{user.Email}</p>
                        <p><b>Detalles: </b>{vendorRequest.Description}</p>
                        <!--<p><b>Nombre de Proveedor: </b>{vendorRequest.VendorName}</p>-->
                    </body>
                </html>";

                if (vendorRequest.InvoiceImage != null)
                {
                    var stream = new MemoryStream(vendorRequest.InvoiceImage);

                    attachments.Add(new Attachment(stream, $"Solicitud Proveedor {vendorRequest.VendorName}")
                    {
                        ContentType =
                    {
                        MediaType = "image/jpeg"
                    }
                    });
                }

                return await _generalService.SendEmail(companyCode,
                                                       "Solicitud de Nuevo Proveedor",
                                                       html,
                                                       emails.Value.Split(',').ToList(),
                                                       true,
                                                       attachments);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en SendEmailNewVendor: " + ex.Message);
            }
        }
    }
}
