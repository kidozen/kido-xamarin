using System;
using System.Net;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;

namespace KidoZen.authentication
{
    public class WRAPv09IdentityProvider : IIdentityProvider
    {
        private string wrapName;
        private string wrapPassword;

        public WRAPv09IdentityProvider()
        {
        }

        public void Configure(IdentityProviderConfig configuration)
        {
        }

        public void Initialize(String username, String password)
        {
            this.wrapName = username;
            this.wrapPassword = password;
        }

        public void BeforeRequestToken(Object[] args)
        {
        }

        public void AfterRequestToken(Object[] args)
        {
        }

        public async Task<RequestTokenResult> RequestToken(Uri identityProviderUrl, String scope)
        {
            try
            {
                var sb = new StringBuilder(512);
                sb.Append("wrap_name=");
                sb.Append(WebUtility.UrlEncode(wrapName));
                sb.Append("&wrap_password=");
                sb.Append(WebUtility.UrlEncode(wrapPassword));
                sb.Append("&wrap_scope=");
                sb.Append(WebUtility.UrlEncode(scope));

                var headers = new Dictionary<string, string>();
                headers["Accept"] = "text/xml";

                var res = await identityProviderUrl.ExecuteAsync<object>(null, sb.ToString(), "application/x-www-form-urlencoded", "POST", headers: headers, cache:true);
#if MONOTOUCH
				var content = res.DataAsString;
#else

                var content = res.Data.ToString();
#endif
                var contentType = default(string);
                res.Headers.TryGetValue("Content-Type", out contentType);

                if (contentType==null && !contentType.ToLower().Contains("xml"))
                    throw new Exception(content);

                var doc = XDocument.Parse(content);
                var fault = doc
                    .Descendants(XName.Get("Fault", "http://www.w3.org/2003/05/soap-envelope"))
                    .FirstOrDefault();

                if (fault != null)
                {
                    var text = fault
                    .Descendants(XName.Get("Text", "http://www.w3.org/2003/05/soap-envelope"))
                    .FirstOrDefault();

                    throw new Exception( text != null ? text.Value : fault.ToString() );
                }

                var indexBegin = content.IndexOf("<Assertion ");
                var indexEnd = content.IndexOf("</Assertion>") + 12;
                var token = content.Substring(indexBegin, indexEnd - indexBegin);
                return new RequestTokenResult { Token = token };
            }
            catch (Exception ex)
            {
				Console.WriteLine (ex.ToString ());
                throw;
            }
        }
    }
}
