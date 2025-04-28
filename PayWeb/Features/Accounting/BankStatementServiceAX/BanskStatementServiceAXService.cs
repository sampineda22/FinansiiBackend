using System.Linq;
using System.Threading.Tasks;

using PayWeb.Common;
using System.Collections.Generic;
using PayWeb.Infrastructure.Core;
using Microsoft.Data.SqlClient;
using CRM.Common;
using ServiceReference1;
using System;
using CRM.Features.Accounting.BankStatement;
using CRM.Infrastructure;

namespace CRM.Features.Accounting.BankStatementServiceAX
{
    public class BanskStatementServiceAXService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BankStatementAppService _bankStatementAppService;
        private readonly AXEndpoint _axEndpoint;

        public BanskStatementServiceAXService(IUnitOfWork unitOfWork, BankStatementAppService bankStatementAppService, AXEndpoint axEndpoint)
        {
            _unitOfWork = unitOfWork;
            _bankStatementAppService = bankStatementAppService;
            _axEndpoint = axEndpoint;
        }

        public async Task<EntityResponse> SendBankStatement(string bankStatements)
        {
            List<int> bankStatementsIds = bankStatements.Split(',').Select(int.Parse).ToList();
            List<string> responses = new();
            List<string> errors = new();

            foreach (int BankStatementID in bankStatementsIds)
            {
                BANKSTATEMENTHEAD HEADER = new BANKSTATEMENTHEAD();
                List<BANKSTATEMENTLINES> LIST = new List<BANKSTATEMENTLINES>();
                
                BankStatement.BankStatement bankStatement = _unitOfWork.Repository<BankStatement.BankStatement>().Query().Where(x => x.BankStatementId == BankStatementID).FirstOrDefault();

                SqlParameter[] parameters =
                {
                  new SqlParameter("@BankStatementId",BankStatementID),
                  new SqlParameter("@CompanyCode",bankStatement.CompanyId)
                };

                var data = _unitOfWork.Repository<BankStatementServiceAX>().GetSP<BankStatementServiceAX>("IM_GetBankstatementLines", parameters).ToList();
                string TransactionCodeNull = "";
                data.ForEach(element =>
                {
                    BANKSTATEMENTLINES LINE = new BANKSTATEMENTLINES();
                    LINE.JOURNALNAMEID = element.JOURNALNAMEID;
                    LINE.CURRENCYCODE = element.CURRENCYCODE;
                    LINE.LEDGERJOURNALTRANSTXT = element.LEDGERJOURNALTRANSTXT.Replace(" ", "") == "" ? "Sin descripción." : element.LEDGERJOURNALTRANSTXT;
                    LINE.AMOUNTCURDEBIT = element.AMOUNTCURDEBIT;
                    LINE.AMOUNTCURCREDIT = element.AMOUNTCURCREDIT;
                    LINE.ACCOUNTNUM = element.ACCOUNTNUM;
                    LINE.PAYMREFERENCE = element.PAYMREFERENCE;
                    LINE.TRANSDATE = element.TRANSDATE.Day + "/" + element.TRANSDATE.Month + "/" + element.TRANSDATE.Year;
                    LINE.POSTJOURNAL = element.POSTJOURNAL;
                    LIST.Add(LINE);
                    if (LINE.JOURNALNAMEID == null)
                    {
                        TransactionCodeNull += (TransactionCodeNull != "" ? "," : "") + element.TransactionCode;
                    }
                });

                if(LIST.Count <= 0)
                {
                    errors.Add($"No se encontraron transacciones para exportar a AX de la cuenta {bankStatement.AccountId}");
                    continue;
                }

                if (TransactionCodeNull != "")
                {
                    errors.Add($"Transacciones no configuradas para la cuenta {bankStatement.AccountId}: {TransactionCodeNull}");
                    continue;
                }

                HEADER.LINES = LIST.ToArray();

                string BankStatementLines = HEADER.Serialize();
                CallContext context = new CallContext { Company = data[0].CompanyId };
                var serviceClient = new M_BankStatementClient(_axEndpoint.GetBinding(), _axEndpoint.GetEndpointAddr("IM_BankStatementGP"));

                //serviceClient.ClientCredentials.Windows.ClientCredential.UserName = "servicio_ax";
                //serviceClient.ClientCredentials.Windows.ClientCredential.Password = "Int3r-M0d@.aX$3Rv";

                serviceClient = (M_BankStatementClient)_axEndpoint.Service(serviceClient);

                try
                {
                    string dataValidation = string.Format("<INTEGRATION><COMPANY><CODE>{0}</CODE><USER>{1}</USER></COMPANY></INTEGRATION>", context.Company, "servicio_ax");
                    IM_BankStatementInitRequest request = new IM_BankStatementInitRequest();
                    request.CallContext = context;
                    request._dataValidationXML = dataValidation;
                    request._lineXML = BankStatementLines;
                    var resp = serviceClient.initAsync(request);

                    if (resp.Result.response.Contains("LD"))
                    {
                        await _bankStatementAppService.UpdateStatus(BankStatementID, Infrastructure.Enum.BankStatementStatus.BankStatatementState.Processed);
                        responses.Add($"{bankStatement.AccountId}: {resp.Result.response}.");
                    }
                    else
                    {
                        errors.Add($"{bankStatement.AccountId}: {resp.Result.response}.");
                    }
                }
                catch (Exception ex)
                {
                    return EntityResponse.CreateError(ex.ToString());
                }
            }

            if (errors.Count > 0)
            {
                string message = responses.Count > 0 ? $"Diarios creados: {string.Join(" ", responses)} Errores encontrados: {string.Join(" ", errors)}" : string.Join(" ", errors);
                return EntityResponse.CreateError(message);
            }

            return EntityResponse.CreateOk(string.Join(" ", responses));
        }
        /*private NetTcpBinding GetBinding()
        {
            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.Name = "NetTcpBinding_IM_WMSCreateJournalServices";
            netTcpBinding.MaxBufferSize = int.MaxValue;
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;
            return netTcpBinding;
        }
        private EndpointAddress GetEndpointAddr()
        {
            //string url = "net.tcp://gim-pro3-AOS:8201/DynamicsAx/Services/IM_BankStatementGP";
            string url = "net.tcp://gim-dev-aos:8201/DynamicsAx/Services/IM_BankStatementGP";
            string user = "sqladmin@intermoda.com.hn";

            var uri = new Uri(url);
            //var epid = new UpnEndpointIdentity(user);
            var addrHdrs = new AddressHeader[0];
            var endpointAddr = new EndpointAddress(uri, addrHdrs); //, epid, addrHdrs);
            return endpointAddr;
        }*/
    }
}
