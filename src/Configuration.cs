using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KidoZen
{
    public partial class Configuration
    {
        KZApplication app;
        Uri endpoint;

        public Uri Url { get; private set; }
        public string Name { get; private set; }

        internal Configuration(KZApplication app, Uri endpoint)
            : this(app, endpoint, null)
        {
        }

        internal Configuration(KZApplication app, Uri endpoint, string name)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            this.app = app;

            if (name != null)
            {
                name = name.ToLower();
                Url = endpoint.Concat(name);
            }
            
            Name = name;
            this.endpoint = endpoint;
        }

        public Configuration this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
                return new Configuration(app, this.endpoint, name);
            }
        }

        public async Task<ServiceEvent<JToken>> Delete()
        {
            Validate();
            return await Url.ExecuteAsync<JToken>(app, method: "DELETE");
        }

        public async Task<ServiceEvent<IEnumerable<Config>>> All()
        {
            Validate();
            return await Url.ExecuteAsync<IEnumerable<Config>>(app);
        }

        public async Task<ServiceEvent<JToken>> Save<T>(T value)
        {
            Validate();
            return await Url.ExecuteAsync<JToken>(app, value.ToJToken(), "POST");
        }

        public async Task<ServiceEvent<JToken>> Get()
        {
            return await Get<JToken>();
        }

        public async Task<ServiceEvent<T>> Get<T>()
        {
            Validate();
            return await Url.ExecuteAsync<T>(app);
        }

        private void Validate()
        {
            if (Name == null) throw new Exception(@"Configuration name is missing. Use Configuration[""name""]");
        }
    }
}