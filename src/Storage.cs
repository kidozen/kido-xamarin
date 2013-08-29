using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KidoZen
{
    public partial class Storage
    {
        KZApplication app;
        Uri endpoint;

        public Uri Url { get; private set; }
        public String Name { get; private set; }
        public Indexes Indexes { get; private set; }

        internal Storage(KZApplication app, Uri endpoint)
            : this(app, endpoint, null)
        {
        }

        internal Storage(KZApplication app, Uri endpoint, string name)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (endpoint == null) throw new ArgumentNullException("endpoint");

            this.app = app;
            this.endpoint = endpoint;
            this.Url = endpoint.Concat(name);
            this.Name = name;
            this.Indexes = new Indexes(app, Url);
        }

        public Storage this[string name]
        {
            get 
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

                return new Storage(app, endpoint, name);
            }
        }

        public async Task<ServiceEvent<JToken>> Drop()
        {
            Validate();
            return await Url.ExecuteAsync<JToken>(app, method:"DELETE");
        }

        public async Task<ServiceEvent<JToken>> Delete(string id)
        {
            Validate();
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException("id");

            return await Url.Concat(id).ExecuteAsync<JToken>(app, method: "DELETE");
        }

        public async Task<ServiceEvent<JArray>> Query(string query = "{}", string options = "{}", string fields = "{}")
        {
            return await DoQuery<JArray>(query, options, fields);
        }

        public async Task<ServiceEvent<IEnumerable<T>>> Query<T>(string query = "{}", string options = "{}", string fields = "{}")
        {
            return await DoQuery<IEnumerable<T>>(query, options, fields);
        }

        private async Task<ServiceEvent<T>> DoQuery<T>(string query = "{}", string options = "{}", string fields = "{}")
        {
            Validate();
            var queryString = string.Format("?query={0}&options={1}&fields={2}",
                WebUtility.UrlEncode(query),
                WebUtility.UrlEncode(options),
                WebUtility.UrlEncode(fields));

            return await new Uri(Url, queryString).ExecuteAsync<T>(app);
        }

        public async Task<ServiceEvent<JToken>> Get(string id)
        {
            Validate();
            return await Url.Concat(id).ExecuteAsync<JToken>(app);
        }

        public async Task<ServiceEvent<T>> Get<T>(string id)
        {
            Validate();
            return await Url.Concat(id).ExecuteAsync<T>(app);
        }

        public async Task<ServiceEvent<StorageObject>> Save(StorageObject value, bool isPrivate = false)
        {
            return await Save<StorageObject>(value, isPrivate);
        }

        public async Task<ServiceEvent<StorageObject>> Save<T>(T value, bool isPrivate = false)
        {
            Validate();
            var obj = value.ToJToken() as JObject;
            if (obj == null) throw new ArgumentException("Value must be an object, it could not be a value type.", "value");

            var id = obj.Value<string>("_id");
            if (string.IsNullOrWhiteSpace(id))
                return await Insert(obj, isPrivate);
            else
                return await Update(id, obj);
        }

        public async Task<ServiceEvent<StorageObject>> Insert(StorageObject value, bool isPrivate = false)
        {
            return await Insert<StorageObject>(value, isPrivate);
        }

        public async Task<ServiceEvent<StorageObject>> Insert<T>(T value, bool isPrivate = false)
        {
            Validate();
            return await new Uri(Url, "?isPrivate=" + isPrivate.ToString()).ExecuteAsync<StorageObject>(app, value.ToJToken(), "POST");
        }

        public async Task<ServiceEvent<StorageObject>> Update(StorageObject value)
        {
            return await Update(value._id, value);
        }

        public async Task<ServiceEvent<StorageObject>> Update<T>(string id, T value)
        {
            Validate();
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException("id");
        
            return await Url.Concat(id).ExecuteAsync<StorageObject>(app, value.ToJToken(), "PUT");
        }

        public async Task<ServiceEvent<JArray>> All()
        {
            Validate();
            return await Url.ExecuteAsync<JArray>(app);
        }

        public async Task<ServiceEvent<IEnumerable<T>>> All<T>()
        {
            Validate();
            return await Url.ExecuteAsync<IEnumerable<T>>(app);
        }

        private void Validate()
        {
            if (Name == null) throw new Exception(@"ObjectSet name is missing. Use Storage[""name""]");
        }
    }
}