using KidoZen.authentication;

public class IdentityProviderConfig
{
    public string key { get; set; }
    public string protocol { get; set; }
    public IIdentityProvider instance { get; set; }

    public string ipEndpoint { get; set; }
    public string authServiceScope { get; set; }

    public string authServiceEndpoint { get; set; }
    public string applicationScope { get; set; }
    public string marketplaceScope { get; set; }

    public string serviceBusIpScope { get; set; }
    public string serviceBusEndpoint { get; set; }
    public string serviceBusScope { get; set; }
}
