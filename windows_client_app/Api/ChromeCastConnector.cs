using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;

namespace ClientApp
{
    internal class ChromeCastConnector
    {
        private IPAddress _localIpAddress;

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

        public IEnumerable<ChromeCastClient> GetChromeCastDevices()
        {
            const string port = "8080";

            string baseAddress = $"http://{_localIpAddress}:{port}/ping";

            var request = WebRequest.Create(baseAddress);

            request.Method = "GET";

            using var responseStream = request.GetResponse();

            var reader = new StreamReader(responseStream.GetResponseStream());

            var result = reader.ReadToEnd();

            return JsonSerializer.Deserialize<IEnumerable<ChromeCastClient>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true } );
        }
    }
}
