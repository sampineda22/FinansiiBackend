using System.Linq;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;
using System.Text;
using PayWeb.Common;
using System.Collections.Generic;
using PayWeb.Infrastructure.Core;
using Microsoft.Data.SqlClient;
using CRM.Common;
using ServiceReference1;
using System.ServiceModel;
using System;
using System.ServiceModel.Channels;
using CRM.Features.BankStatement;

namespace CRM.Features.BankStatementServiceAX
{
    public class BanskStatementServiceAXService 
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BankStatementAppService _bankStatementAppService;

        public BanskStatementServiceAXService(IUnitOfWork unitOfWork, BankStatementAppService bankStatementAppService)
        {
            _unitOfWork = unitOfWork;
            _bankStatementAppService = bankStatementAppService;
        }

        public async Task<EntityResponse> SendBankStatement(int BankStatementID)
        {
            BANKSTATEMENTHEAD HEADER = new BANKSTATEMENTHEAD();
            List<BANKSTATEMENTLINES> LIST = new List<BANKSTATEMENTLINES>();

            SqlParameter[] parameters =
            {
                new SqlParameter("@BankStatementId",BankStatementID)
            };

            var data =_unitOfWork.Repository<BankStatementServiceAX>().GetSP<BankStatementServiceAX>("IM_GetBankstatementLines",parameters).ToList();
            string TransactionCodeNull = "";
            data.ForEach(element =>
            {
                BANKSTATEMENTLINES LINE = new BANKSTATEMENTLINES();
                LINE.JOURNALNAMEID = element.JOURNALNAMEID;
                LINE.CURRENCYCODE = element.CURRENCYCODE;
                LINE.LEDGERJOURNALTRANSTXT = element.LEDGERJOURNALTRANSTXT;
                LINE.AMOUNTCURDEBIT = element.AMOUNTCURDEBIT;
                LINE.AMOUNTCURCREDIT = element.AMOUNTCURCREDIT;
                LINE.ACCOUNTNUM = element.ACCOUNTNUM;
                LINE.PAYMREFERENCE = element.PAYMREFERENCE;
                LINE.TRANSDATE = element.TRANSDATE.Day + "/"+element.TRANSDATE.Month+"/"+element.TRANSDATE.Year;
                LIST.Add(LINE);
                if (LINE.JOURNALNAMEID == null)
                {
                    TransactionCodeNull += (TransactionCodeNull != "" ? "," : "") +element.TransactionCode;
                }
            });

            if(TransactionCodeNull != "")
            {
                return EntityResponse.CreateError("Transacciones no configuradas: " + TransactionCodeNull);
            }

            HEADER.LINES = LIST.ToArray();

            string BankStatementLines = SerializationService.Serialize(HEADER);
            ServiceReference1.CallContext context = new ServiceReference1.CallContext { Company = data[0].CompanyId };
            var serviceClient = new M_BankStatementClient(GetBinding(),GetEndpointAddr());

            serviceClient.ClientCredentials.Windows.ClientCredential.UserName = "servicio_ax";
            serviceClient.ClientCredentials.Windows.ClientCredential.Password = "Int3r-M0d@.aX$3Rv";

            try
            {
                string dataValidation = string.Format("<INTEGRATION><COMPANY><CODE>{0}</CODE><USER>{1}</USER></COMPANY></INTEGRATION>", context.Company, "servicio_ax");
                IM_BankStatementInitRequest request = new IM_BankStatementInitRequest();
                request.CallContext = context;
                request._dataValidationXML = dataValidation;
                request._lineXML = BankStatementLines;
                var resp = serviceClient.initAsync(request);

                await _bankStatementAppService.UpdateStatus(BankStatementID, Infrastructure.Enum.BankStatementStatus.BankStatatementState.Processed);

                return EntityResponse.CreateOk(resp.Result.response);
            }
            catch (Exception ex)
            {
                return  EntityResponse.CreateError(ex.ToString());
            }

        }
        private NetTcpBinding GetBinding()
        {
            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.Name = "NetTcpBinding_IM_WMSCreateJournalServices";
            netTcpBinding.MaxBufferSize = int.MaxValue;
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;
            return netTcpBinding;
        }
        private EndpointAddress GetEndpointAddr()
        {

            string url = "net.tcp://gim-pro3-AOS:8201/DynamicsAx/Services/IM_BankStatementGP";
            string user = "sqladmin@intermoda.com.hn";

            var uri = new Uri(url);
            //var epid = new UpnEndpointIdentity(user);
            var addrHdrs = new AddressHeader[0];
            var endpointAddr = new EndpointAddress(uri, addrHdrs); //, epid, addrHdrs);
            return endpointAddr;
        }

    }
}
