using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace KidoZen
{
    public class Indexes
    {
        private KZApplication app;

        public Uri Url { get; private set; }

        internal Indexes(KZApplication app, Uri url)
        {
            if (app == null) throw new ArgumentNullException("app");
            this.app = app;

            if (url == null) throw new ArgumentNullException("url");
            Url = url.Concat("indexes");
        }

        public async Task<ServiceEvent<JToken>> All()
        {
            return await Url.ExecuteAsync<JToken>(app);
        }

        public async Task<ServiceEvent<JToken>> Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            return await new Uri(Url, "?name=" + name).ExecuteAsync<JToken>(app);
        }

        public async Task<ServiceEvent<JToken>> Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            return await new Uri(Url, "?name=" + name).ExecuteAsync<JToken>(app, method:"DELETE");
        }

        /// <summary>
        /// </summary>
        /// <param name="spec">JSON string with the index's specification. {lastName:1, firstName:1}</param>
        /// <param name="background">creates the index in the background, yielding whenever possible</param>
        /// <param name="dropDups">a unique index cannot be created on a key that has pre-existing duplicate: values. If you would like to create the index anyway, keeping the first document the database: indexes and deleting all subsequent documents that have duplicate value</param>
        /// <param name="min">for geospatial indexes set the lower bound for the co-ordinates</param>
        /// <param name="max">for geospatial indexes set the high bound for the co-ordinates.</param>
        public async Task<ServiceEvent<JToken>> Create(string spec, bool safe = false, bool unique = false, bool sparse = false, bool background = false, bool dropDups = false, double? min = null, double? max = null)
        {
            if (string.IsNullOrWhiteSpace(spec)) throw new ArgumentNullException("spec");

            var options = new JObject();
            options["safe"] = safe;
            options["unique"] = unique;
            options["sparse"] = sparse;
            options["background"] = background;
            options["dropDups"] = dropDups;
            if (min.HasValue) options["min"] = min.Value;
            if (max.HasValue) options["max"] = max.Value;

            var body = new JObject();
            body["spec"] = spec;
            body["options"] = options;

            return await Url.ExecuteAsync<JToken>(app, options);
        }
    }
}
