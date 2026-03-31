using System.ServiceModel.Channels;
using System.ServiceModel;
using System;
using CRM.Infrastructure.Core;
using Microsoft.Extensions.Options;
using ServiceReference1;

namespace CRM.Infrastructure.Endpoint
{
    public class AXEndpoint
    {
        private readonly AXConnectionSettings _axConnectionSettings;

        public AXEndpoint(IOptions<AXConnectionSettings> axConnectionSettings)
        {
            _axConnectionSettings = axConnectionSettings.Value;
        }
        public EndpointAddress GetEndpointAddr(string serviceGroup)
        {
            string connectionString = _axConnectionSettings.DefaultConnection;

            //string url = "net.tcp://gim-pro3-AOS:8201/DynamicsAx/Services/IM_CDJournalSG";
            string url = $"{connectionString}{serviceGroup}";
            string user = "sqladmin@intermoda.com.hn";

            var uri = new Uri(url);
            //var epid = new UpnEndpointIdentity(user);
            var addrHdrs = new AddressHeader[0];
            var endpointAddr = new EndpointAddress(uri, addrHdrs); //, epid, addrHdrs);
            return endpointAddr;
        }
        public NetTcpBinding GetBinding()
        {
            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.Name = "NetTcpBinding_IM_WMSCreateJournalServices";
            netTcpBinding.MaxBufferSize = int.MaxValue;
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;
            return netTcpBinding;
        }

        public dynamic Service(dynamic serviceClient)
        {
            serviceClient.ClientCredentials.Windows.ClientCredential.UserName = _axConnectionSettings.UserName;
            serviceClient.ClientCredentials.Windows.ClientCredential.Password = _axConnectionSettings.Password;

            return serviceClient;
        }
    }
}
