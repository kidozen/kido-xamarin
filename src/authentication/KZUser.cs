using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace KidoZen.authentication
{
    public class KZUser
    {
        public String[] Roles { get; internal set; }
        public Dictionary<String, String[]> Claims { get; internal set; }
        public Dictionary<String, String[]> ClaimsByName { get; internal set; }

        public Token TokenMarketplace { get; internal set; }
        public Token TokenApplication { get; internal set; }
        public Token TokenServiceBus { get; internal set; }

        public NetworkCredential Credential { get; internal set; }

        public String[] getClaimByName(String name)
        {
            string[] val = null;
            ClaimsByName.TryGetValue(name, out  val);
            return val;
        }

        public String[] getClaimByType(String typeName)
        {
            string[] val = null;
            Claims.TryGetValue(typeName, out  val);
            return val;
        }

        public Boolean IsInRole(String role)
        {
            return Roles.Any(r => string.Compare(role, r, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        public string Provider { get; internal set; }
    }
}
