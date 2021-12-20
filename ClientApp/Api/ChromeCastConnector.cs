using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ClientApp
{
    internal class ChromeCastConnector
    {
        private IPAddress? _localIpAddress;

        public void PlayVideo(string path)
        {
        }

        public void Initialize()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                throw new Exception("No Internet Connected!");
            }

            var host = Dns.GetHostEntry(Dns.GetHostName());

            _localIpAddress = host.AddressList.LastOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (_localIpAddress is null)
            {
                throw new Exception("No network adapters with an IPv4 address in the system!");
            }
        }

        public void Connect()
        {
        }
    }
}
