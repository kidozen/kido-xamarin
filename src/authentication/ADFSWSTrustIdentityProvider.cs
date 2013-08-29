using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using System.Xml;

namespace KidoZen.authentication
{
    public class ADFSWSTrustIdentityProvider : IIdentityProvider
    {
        private string username;
        private string password;
        private string template;

        private const string TEMPLATE = "KidoZen.Resources.SOAPRSTTemplate.xml";
        private const string ADFSTOKEN = "KidoZen.Resources.ADFSToken.xml";

        public ADFSWSTrustIdentityProvider()
        {
        }

        public void Initialize(string username, string password)
        {
            this.username = username;
            this.password = password;

            template = GetResource(TEMPLATE)
                .Replace("[Username]", username)
                .Replace("[Password]", password);
        }

        public void Configure(IdentityProviderConfig configuration)
        {
        }

        public void BeforeRequestToken(Object[] args)
        {
        }

        public void AfterRequestToken(Object[] args)
        {
        }

        public async Task<RequestTokenResult> RequestToken(Uri identityProviderUrl, string scope)
        {
            var message = template
                .Replace("[To]", identityProviderUrl.ToString())
                .Replace("[applyTo]", scope);

            try
            {
                var res = await identityProviderUrl.ExecuteAsync<object>(null, message, "application/soap+xml", "POST", cache:true);
                var content = res.Data.ToString();
                var doc = XDocument.Parse(content);
                var fault = doc
                    .Descendants(XName.Get("Fault", "http://www.w3.org/2003/05/soap-envelope"))
                    .FirstOrDefault();

                if (fault!=null)
                {
                    var text = fault
                    .Descendants(XName.Get("Text", "http://www.w3.org/2003/05/soap-envelope"))
                    .FirstOrDefault();

                    throw new Exception(text!=null ? text.Value : fault.ToString());
                }

                var indexBegin = content.IndexOf("<Assertion ");
                var indexEnd = content.IndexOf("</Assertion>") + 12;
                var token = content.Substring(indexBegin, indexEnd - indexBegin);
                return new RequestTokenResult { Token = token };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetResource(string name)
        {
            using (var s = typeof(KZApplication).GetTypeInfo().Assembly.GetManifestResourceStream(name))
                using (var r = new StreamReader(s))
                    return r.ReadToEnd();
        }
    }
}
