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

                string html = $@"
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
                    Solicitud de Creación de Proveedor
                    </h2>

                    <table width='100%' cellpadding='8' cellspacing='0'
                    style='border-collapse:collapse;font-size:14px;'>

                    <tr style='background:#f7f7f7'>
                    <td width='35%'><b>Nombre del proveedor</b></td>
                    <td>{vendorRequest.VendorName}</td>
                    </tr>

                    <tr>
                    <td><b>{company.DocumentoFiscal}</b></td>
                    <td>{vendorRequest.RTN}</td>
                    </tr>

                    <tr style='background:#f7f7f7'>
                    <td><b>Grupo</b></td>
                    <td>Comercio Nacional</td>
                    </tr>

                    <tr>
                    <td><b>Divisa</b></td>
                    <td>{user.Currency}</td>
                    </tr>

                    <tr style='background:#f7f7f7'>
                    <td><b>Solicitante</b></td>
                    <td>{user.Name}</td>
                    </tr>

                    <tr>
                    <td><b>Correo del solicitante</b></td>
                    <td>{user.Email}</td>
                    </tr>

                    <tr style='background:#fff3cd'>
                    <td><b>Descripción</b></td>
                    <td style='color:#003366;font-weight:bold;'>
                    {vendorRequest.Description}
                    </td>
                    </tr>

                    </table>

                    </td>
                    </tr>

                    <tr>
                    <td align='center'
                    style='background:#003366;color:white;padding:15px;font-size:12px;'>

                    Este correo fue generado automáticamente.<br/>

                    </td>
                    </tr>

                    </table>

                    </td>
                    </tr>
                    </table>

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
