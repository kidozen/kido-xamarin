using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidoZen.authentication
{
    public class Token
    {
        public string Value { get; private set; }
        public DateTime Expiration { get; private set; }

        private Token()
        {
        }
        
        static internal Token Create(RequestTokenResult request)
        {
            if (request == null) return null;

            return new Token {
                Value = request.Token,
                Expiration = request.ExpirationTime.HasValue ? request.ExpirationTime.Value : DateTime.MaxValue
            };
        }

        public Boolean Expired()
        {
            return Expiration < DateTime.Now;
        }
    }
}
