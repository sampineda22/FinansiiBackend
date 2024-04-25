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

namespace CRM.Features.BankStatementServiceAX
{
    public class BanskStatementServiceAXService 
    {
        private readonly IUnitOfWork _unitOfWork;

        public BanskStatementServiceAXService(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
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

            data.ForEach(element =>
            {
                BANKSTATEMENTLINES LINE = new BANKSTATEMENTLINES();
                LINE.JOURNALNAMEID = element.JOURNALNAMEID;
                LINE.CURRENCYCODE = element.CURRENCYCODE;
                LINE.LEDGERJOURNALTRANSTXT = element.LEDGERJOURNALTRANSTXT;
                LINE.AMOUNTCURDEBIT = element.AMOUNTCURDEBIT;
                LINE.AMOUNTCURCREDIT = element.AMOUNTCURCREDIT;
                LINE.ACCOUNTNUM = element.ACCOUNTNUM;
                LIST.Add(LINE);
            });

            HEADER.LINES = LIST.ToArray();

            string BankStatementLines = SerializationService.Serialize(HEADER);







            return EntityResponse.CreateOk();
        }
    }
}
