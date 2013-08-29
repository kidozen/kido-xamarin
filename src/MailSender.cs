using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KidoZen
{
    public class MailSender
    {
        KZApplication app;
        Uri endpoint;

        public Uri Url { get; private set; }

        internal MailSender(KZApplication app, Uri endpoint)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            this.app = app;
            this.endpoint = endpoint;
            Url = endpoint;
        }

        public async Task<ServiceEvent<JToken>> Send(Mail mail)
        {
            if (mail == null) throw new ArgumentNullException("mail");
            if (string.IsNullOrWhiteSpace(mail.to)) throw new ArgumentNullException("mail.to");
            if (string.IsNullOrWhiteSpace(mail.from)) throw new ArgumentNullException("mail.from");

            return await Url.ExecuteAsync<JToken>(app, mail.ToJToken(), "POST");
        }
    }
}