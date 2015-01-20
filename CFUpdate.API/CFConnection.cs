// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from CFUpdate.API INC. team.
//  
// Copyrights (c) 2014 CFUpdate.API INC. All rights reserved.

using System;
using System.IO;
using System.Net;
using System.Text;

namespace CFUpdate.API
{
    /// <summary>
    ///   Provides methods to interact with the Cloudflare JSON API
    /// </summary>
    public class CFConnection
    {
        /// <summary>
        ///   Cloudflare API Url
        /// </summary>
        private const string APIUrl = "https://www.cloudflare.com/api_json.html";

        /// <summary>
        ///   Connection info necessary to access Cloudflare API
        /// </summary>
        private readonly CFConnectionInfo _connectionInfo;

        /// <summary>
        ///   Constructs an instance of the <see cref="CFConnection" /> class
        /// </summary>
        private CFConnection(CFConnectionInfo info)
        {
            _connectionInfo = info;
        }

        /// <summary>
        ///   Creates a new CF Connection
        /// </summary>
        public static CFConnection CreateConnection(CFConnectionInfo info)
        {
            return new CFConnection(info);
        }

        /// <summary>
        ///   Updates a subdomain's IP address
        /// </summary>
        public void UpdateDomainRecord(string domain, string subDomain, string ipAddress)
        {
            // Validate parameters
            if(string.IsNullOrEmpty(domain))
            {
                throw new Exception("Domain was null or empty");
            }
            if(string.IsNullOrEmpty(subDomain))
            {
                throw new Exception("Subdomain was null or empty");
            }
            if(string.IsNullOrEmpty(ipAddress))
            {
                throw new Exception("IP was null, empty, or invalid");
            }
            IPAddress tmpIp;
            if(!IPAddress.TryParse(ipAddress, out tmpIp))
            {
                throw new Exception("IP was not valid");
            }

            var request = WebRequest.Create(APIUrl);
            ServicePointManager.ServerCertificateValidationCallback += (snd, cert, chain, ssl) => true;
            request.Proxy = null;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "POST";

            var postData = "a=rec_edit&tkn=" + Uri.EscapeDataString(_connectionInfo.Token) + "&id=" + Uri.EscapeDataString(_connectionInfo.ID) + "&email="
                           + Uri.EscapeDataString(_connectionInfo.Email) + "&z=" + Uri.EscapeDataString(domain) + "&type=A&name="
                           + Uri.EscapeDataString(subDomain) + "&content=" + Uri.EscapeDataString(ipAddress) + "&service_mode=0&ttl=1";

            var byteArray = Encoding.UTF8.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            var response = request.GetResponse();
            dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            reader.Close();
            dataStream.Close();
            response.Close();
        }
    }
}