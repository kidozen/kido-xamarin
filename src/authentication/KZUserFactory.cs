using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace KidoZen.authentication
{
    internal static class KZUserFactory
    {
        static internal async Task<KZUser> Authenticate(IdentityProviderConfig config)
        {
            try
            {
                var ipToken = await config.instance.RequestToken(new Uri(config.ipEndpoint), config.authServiceScope);
                var kzTokenUser = await RequestKidoTokenAsync(config.authServiceEndpoint, config.applicationScope, ipToken.Token);
                var kzMarketplaceTokenUser = await RequestKidoTokenAsync(config.authServiceEndpoint, config.marketplaceScope, ipToken.Token);

                RequestTokenResult kzTokenSB = null;
                //if (!string.IsNullOrWhiteSpace(config.serviceBusIpScope))
                //{
                //    ipToken = await config.instance.RequestToken(new Uri(config.ipEndpoint), config.serviceBusIpScope);
                //    kzTokenSB = await RequestKidoTokenAsync(config.serviceBusEndpoint, config.serviceBusScope, ipToken.Token);
                //}

                return createUser(kzTokenUser, kzTokenSB, kzMarketplaceTokenUser);
            }
            catch (Exception e)
            {
                throw new Exception("User could not be authenticated.", e);
            }
        }

        static private async Task<RequestTokenResult> RequestKidoTokenAsync(string kidoEndpoint, string scope, string ipToken)
        {
            var sb = new StringBuilder(512);
            sb.Append("wrap_scope=");
            sb.Append(WebUtility.UrlEncode(scope));
            sb.Append("&wrap_assertion_format=SAML");
            sb.Append("&wrap_assertion=");
            sb.Append(WebUtility.UrlEncode(ipToken));

            try
            {
                var res = await new Uri(kidoEndpoint).ExecuteAsync<object>(null, sb.ToString(), "application/x-www-form-urlencoded", "POST", cache:true);
                var contentType = default(string);
                contentType = (res.Headers.TryGetValue("Content-Type", out contentType)) ? contentType.ToLower() : "";
                
                if (contentType.Contains("application/json"))
                {
#if MONOTOUCH
					var content = JObject.Parse(res.DataAsString);
#else
					var content = JObject.Parse(res.Data.ToString());
#endif
                    return new RequestTokenResult
                    {
                        Token = content.Value<string>("rawToken"),
                        ExpirationTime = DateTime.ParseExact(content.Value<string>("expirationTime"), "G", DateTimeFormatInfo.InvariantInfo)
                    };
                }
                else if (contentType.Contains("application/x-www-form-urlencoded"))
                {
                    var dictionary = res.Data.ToString()
                        .Split('&')
                        .Select((item) =>
                        {
                            var pair = item.Split('=');
                            return new { key = pair[0], value = pair[1] };
                        })
                        .ToDictionary((pair) => pair.key, (pair) => pair.value);
                    
                    return new RequestTokenResult
                    {
                        Token = WebUtility.UrlDecode(dictionary["wrap_access_token"]),
                        ExpirationTime = DateTime.Now.AddSeconds(int.Parse(dictionary["wrap_access_token_expires_in"]))
                    };
                }
                throw new Exception (res.Data.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Unable to retrieve authentication token.", e);
            }
        }

        static private KZUser createUser(RequestTokenResult userToken, RequestTokenResult serviceBusToken, RequestTokenResult marketplaceToken)
        {
            var tokenClaims = WebUtility.UrlDecode(userToken.Token)
                .Split('&')
                .Select(c =>
                {
                    var pair = c.Split('=');
                    return new { Key = pair[0], Value = pair[1] };
                })
                .GroupBy((p) => p.Key, (k, e) => new { Key = k, Value = e.Select((p) => p.Value).ToArray() })
                .ToDictionary(p => p.Key, p => p.Value);

            var tokenClaimsByName = tokenClaims.Select(p =>
                {
                    var key = p.Key;
                    var indexOfClaimKeyword = p.Key.IndexOf("/claims/");
                    if (indexOfClaimKeyword > -1)
                    {
                        key = p.Key.Substring(indexOfClaimKeyword + "/claims/".Length);
                    }
                    return new { Key = key, Value = p.Value };
                })
                .GroupBy((p) => p.Key, (k, e) => new { Key = k, Value = e.Select((p) => p.Value).Aggregate((x,y)=>x.Concat(y).ToArray()) })
                .ToDictionary(p => p.Key, p => p.Value);

            return new KZUser
            {
                TokenApplication = Token.Create(userToken),
                TokenMarketplace = Token.Create(marketplaceToken),
                TokenServiceBus = Token.Create(serviceBusToken),
                Claims = tokenClaims,
                ClaimsByName = tokenClaimsByName,
                Roles = tokenClaims.ContainsKey("role") ? tokenClaims["role"] : new string[0]
            };
        }    
    }
}
