using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace KidoZen
{
    public partial class Service
    {
        KZApplication app;
        Uri endpoint;

        public Uri Url { get; private set; }
        public String Name { get; private set; }

        internal Service(KZApplication app, Uri endpoint) :
            this(app, endpoint, null)
        {
        }

        internal Service(KZApplication app, Uri endpoint, string name)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            this.app = app;

            this.endpoint = endpoint;
            Name = name;
            if (name != null) Url = endpoint.Concat(name);
        }

        public Service this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
                return new Service(app, endpoint, name);
            }
        }

		public async Task<ServiceEvent<JObject>> Invoke(string method, int timeout = 60)
        {
			return await Invoke<JObject>(method, new JObject(), timeout);
        }
        
		public async Task<ServiceEvent<JObject>> Invoke<T>(string method, T args, int timeout = 60)
        {
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentNullException("method");

            Validate();
            var endpoint = Url.Concat("invoke/" + method);

			return await endpoint.ExecuteAsync<JObject>(app, args.ToJToken(), method:"POST", timeout:new TimeSpan(0,0,timeout));
        }

        private void Validate()
        {
            if (Name == null) throw new Exception(@"Service name is missing. Use Service[""name""]");
        }
		
		[Obsolete("InvokeArray is goint to be deprecated in next versions of KidoZen services/agents.")]
		public async Task<ServiceEvent<JArray>> InvokeArray<T>(string method, T args, int timeout = 60)
		{
			if (string.IsNullOrWhiteSpace(method)) throw new ArgumentNullException("method");

			Validate();
			var endpoint = Url.Concat("invoke/" + method);
			return await endpoint.ExecuteAsync<JArray>(app, args.ToJToken(), method:"POST", timeout:new TimeSpan(0,0,timeout));
		}

    }
}
