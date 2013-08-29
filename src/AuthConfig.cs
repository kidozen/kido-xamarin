using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidoZen
{
    public class AuthConfig
    {
        public string applicationScope { get; internal set; }
        public string authServiceScope { get; internal set; }
        public string authServiceEndpoint { get; internal set; }

        public IReadOnlyDictionary<string, JObject> IdentityProviders { get; internal set; }

        internal AuthConfig(JObject info)
        {
            applicationScope = info.Value<string>("applicationScope");
            authServiceScope = info.Value<string>("authServiceScope");
            authServiceEndpoint = info.Value<string>("authServiceEndpoint");

            IdentityProviders = info.Value<JObject>("identityProviders")
                .Properties()
                .ToDictionary((p) => p.Name, (p) => (JObject)p.Value);
        }
    }
}
