using System;
using System.Linq;
using System.Net;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#if NETFX_CORE
    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;
#endif
#if NET45
	using System.Security.Cryptography;    
#endif
#if WINDOWS_PHONE
    using System.Security.Cryptography;
#endif

namespace KidoZen.authentication
{
    public class Authentication
    {
        private ConcurrentDictionary<string, IdentityProviderConfig> identityProviders = new ConcurrentDictionary<string, IdentityProviderConfig>(StringComparer.CurrentCultureIgnoreCase);
        private string authServiceScope;
        private string applicationScope;
        private string marketplaceScope;
        private string authServiceEndpoint;
        private KZUser kidoZenUser;

        private ConcurrentDictionary<string, KZUser> securityTokens = new ConcurrentDictionary<string, KZUser>();
        private IIdentityProvider currentIP;

        internal Authentication(string domain, JObject config, string application)
        {
            authServiceEndpoint = Validate(config, "authServiceEndpoint");
            applicationScope = Validate(config, "applicationScope");
            marketplaceScope = "http://" + domain + "/";
            authServiceScope = Validate(config, "authServiceScope");

            var providers = Validate<JObject>(config, "identityProviders");

            foreach (var prop in providers.Properties())
            {
                IIdentityProvider instance;
                var ip = prop.Value as JObject;
                var protocol = Validate(ip, "protocol");
                switch (protocol.ToLower())
                {
                    case "wrapv0.9":
                        instance = new WRAPv09IdentityProvider();
                        break;
                    case "adfs":
                        instance = new ADFSWSTrustIdentityProvider();
                        break;
                    case "ws-trus":
                        instance = new ADFSWSTrustIdentityProvider();
                        break;
                    default:
                        throw new Exception("Invalid Identity Provider protocol: " + protocol);
                }

                var ipConfig = new IdentityProviderConfig
                {
                    protocol = protocol,
                    authServiceScope = authServiceScope,
                    ipEndpoint = Validate(ip, "endpoint"),
                    applicationScope = applicationScope,
                    marketplaceScope = marketplaceScope,
                    authServiceEndpoint = authServiceEndpoint,
                    key = prop.Name,
                    instance = instance
                };

                var serviceBus = ip.Value<JObject>("serviceBus");
                if (serviceBus != null)
                {
                    ipConfig.serviceBusIpScope = Validate(serviceBus, "ipScope");
                    ipConfig.serviceBusEndpoint = Validate(serviceBus, "endpoint");
                    ipConfig.serviceBusScope = Validate(serviceBus, "scope");
                }
                
                identityProviders.AddOrUpdate(prop.Name, ipConfig, (key, current) => ipConfig);
            }
        }
        
	    public void RegisterIdentityProvider(string key, string protocol, string endpoint, string scope, IIdentityProvider provider, string serviceBusIpScope = null, string serviceBusEndpoint = null, string serviceBusScope = null)
	    {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException("key");
            if (string.IsNullOrWhiteSpace(protocol)) throw new ArgumentNullException("protocol");
            if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentNullException("endpoint");
            if (provider==null) throw new ArgumentNullException("provider");

            var ipConfig = new IdentityProviderConfig
            {
                authServiceScope = scope,
                ipEndpoint = endpoint,
                applicationScope = applicationScope,
                authServiceEndpoint = authServiceEndpoint,
                marketplaceScope = marketplaceScope,
                serviceBusIpScope = serviceBusIpScope,
                serviceBusEndpoint = serviceBusEndpoint,
                serviceBusScope = serviceBusScope,
                key = key,
                protocol = protocol,
                instance = provider
            };

            identityProviders.AddOrUpdate(key, ipConfig, (k, current) => ipConfig);
        }

        public async Task<KZUser> Authenticate(string userName, string password, string providerKey = "default")
        {
            if (string.IsNullOrWhiteSpace(providerKey)) throw new ArgumentNullException("providerKey");
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentNullException("userName");

            var securityTokenKey = GetHash(string.Format("{0}|{1}|{2}", providerKey, userName, password).ToLower());
            if (securityTokens.TryGetValue(securityTokenKey, out kidoZenUser))
            {
                if (!kidoZenUser.TokenApplication.Expired() &&
                    (kidoZenUser.TokenMarketplace==null || !kidoZenUser.TokenMarketplace.Expired()) &&
                    (kidoZenUser.TokenServiceBus==null || !kidoZenUser.TokenServiceBus.Expired()))
                {
                    return kidoZenUser;
                }
            }

            IdentityProviderConfig provider;
            if (!identityProviders.TryGetValue(providerKey, out provider))
            {
                throw new Exception("The specified provider does not exist");
            }
            currentIP = provider.instance;
            currentIP.Configure(provider);
            currentIP.Initialize(userName, password);
            kidoZenUser = await KZUserFactory.Authenticate(provider);
            if (kidoZenUser != null)
            {
                kidoZenUser.Provider = providerKey;
                kidoZenUser.Credential = new NetworkCredential(userName, password);
                securityTokens.AddOrUpdate(securityTokenKey, kidoZenUser, (k, current) => kidoZenUser);
            }
            return kidoZenUser;
        }

        internal void RemoveFromCache(string userName, string password, string providerKey)
        {
            var securityTokenKey = GetHash(string.Format("{0}|{1}|{2}", providerKey, userName, password).ToLower());
            KZUser user = null;
            securityTokens.TryRemove(securityTokenKey, out user);
        }

        public void SignOut()
        {
            kidoZenUser = null;           
        }

        public KZUser User
        {
            get { return kidoZenUser; }
        }

        public IEnumerable<string> ProviderNames
        {
            get { return identityProviders.Keys; }
        }

        private string Validate(JObject config, string property)
        {
            var data = config.Value<string>(property);
            if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(property, "Authentication's configuration");
            return data;
        }

        private T Validate<T>(JObject config, string property)
        {
            var data = config.Value<T>(property);
            if (data==null) throw new ArgumentNullException(property, "Authentication's configuration");
            return data;
        }

        private String GetHash(String str)
        {
            #if NETFX_CORE
            
            var buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var objAlgProv = HashAlgorithmProvider.OpenAlgorithm("SHA1");
            var buffHash = objAlgProv.HashData(buffUtf8Msg);

            if (buffHash.Length != objAlgProv.HashLength)
            {
                throw new Exception("There was an error creating the hash");
            }

            return CryptographicBuffer.EncodeToBase64String(buffHash);
            
            #elif WINDOWS_PHONE

            var unicodeText = new byte[str.Length * 2];
            var enc = Encoding.Unicode.GetEncoder();
            enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);

            var sha = new SHA1Managed();
            var result = sha.ComputeHash(unicodeText);

            return Convert.ToBase64String(result);

			#elif NET45
			using (var sha1 = new SHA1CryptoServiceProvider())
			{
				var hash = sha1.ComputeHash(UTF8Encoding.UTF8.GetBytes(str));
				var delimitedHexHash = BitConverter.ToString(hash);
				return delimitedHexHash.Replace("-", "");
			}
            
			#elif MONODROID
                return "";    
           
            #elif MONOTOUCH
                return "";    
            #endif


        }
    }
}
