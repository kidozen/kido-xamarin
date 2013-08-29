using System;
using System.Threading.Tasks;

namespace KidoZen.authentication
{
    public interface IIdentityProvider
    {
        Task<RequestTokenResult> RequestToken(Uri identityProviderUrl, string scope);
	    void Configure(IdentityProviderConfig configuration);
        void Initialize(String username, String password);
	    void BeforeRequestToken(Object[] args);
	    void AfterRequestToken(Object[] args);
    }
}
