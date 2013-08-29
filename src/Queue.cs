using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KidoZen
{
    public partial class Queue
    {
        KZApplication app;
        Uri endpoint;

        public Uri Url { get; private set; }
        public String Name { get; private   set; }
    
        internal Queue(KZApplication app, Uri endpoint) :
            this(app, endpoint, null)
        {
        }

        internal Queue(KZApplication app, Uri endpoint, string name)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            this.app = app;
            
            this.endpoint = endpoint;
            Name = name;
            if (name!=null) Url = endpoint.Concat(name);
        }

        public Queue this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
                return new Queue(app, endpoint, name);
            }
        }

        public async Task<ServiceEvent<JToken>> Enqueue<T>(T message)
        {
            Validate();
            return await Url.ExecuteAsync<JToken>(app, message.ToJToken(), "POST");
        }

        public async Task<ServiceEvent<JToken>> Dequeue()
        {
            return await Dequeue<JToken>();
        }

        public async Task<ServiceEvent<T>> Dequeue<T>()
        {
            Validate();
            return await Url.Concat("next").ExecuteAsync<T>(app, method:"DELETE");
        }

        public async Task<ServiceEvent<JObject>> GetInfo()
        {
            Validate();
            return await Url.ExecuteAsync<JObject>(app);
        }

        private void Validate()
        {
            if (Name==null) throw new Exception(@"Queue name is missing. Use Queue[""name""]");
        }
    }

}