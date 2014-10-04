using Newtonsoft;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidoZen
{
    public class AppInfo
    {
        public string _id { get; internal set; }
        public string Name { get; internal set; }
        public string Domain { get; internal set; }
        public int Port { get; internal set; }
        public bool Published { get; internal set; }
        public int Rating { get; internal set; }
        public int RatingCount { get; internal set; }
        public int RatingSum { get; internal set; }
        public string Version { get; internal set; }
        public string Description { get; internal set; }
        public string ShortDescription { get; internal set; }
        public string[] Categories { get; internal set; }
        public string[] Voters { get; internal set; }
        public string Path { get; internal set; }
        public string Uploads { get; internal set; }
        public string GitPath { get; internal set; }

        public IReadOnlyDictionary<string, string> URLs{ get; internal set; }
        public AuthConfig AuthConfig { get; internal set; }
        public JObject JSON { get; internal set; }
 
        internal AppInfo(JObject info)
        {
            JSON = info;

            _id = info.Value<string>("_id");
            Name = info.Value<string>("name");
            Domain = info.Value<string>("domain");
            Port = info.Value<int>("port");
            Published = info.Value<bool>("published");
            Rating = info.Value<int>("rating");
            RatingCount = info.Value<int>("ratingCount");
            RatingSum = info.Value<int>("ratingSum");
            Version = info.Value<string>("version");
            Description = info.Value<string>("description");
            ShortDescription = info.Value<string>("shortDescription");
            Path = info.Value<string>("path");
            Uploads = info.Value<string>("uploads");
            GitPath = info.Value<string>("gitPath");
            Categories = GetArray<string>(info, "categories");
            Voters = GetArray<string>(info, "voters");
            AuthConfig = new AuthConfig(info.Value<JObject>("authConfig"));

            var uris = new Dictionary<string, string>();
            URLs = uris;

            // add uris and removes empty properties
            var props = info.Properties().ToArray();
            foreach (var prop in props)
            {
                var name = prop.Name;
                var value = prop.Value;

                switch (value.Type)
                {
                    case JTokenType.Uri:
                        uris.Add(name, prop.Value<Uri>().ToString());
                        break;
                    case JTokenType.String:
                        var valueStr = value.Value<string>();
                        if (Uri.IsWellFormedUriString(valueStr, UriKind.Absolute) || 
                            name.EndsWith("URL", StringComparison.CurrentCultureIgnoreCase))
                        {
                            uris.Add(name, valueStr);
                        }
                        break;
                }
            }
        }

        T[] GetArray<T>(JObject obj, string name)
        {
            var a = obj.Value<JArray>(name);
            return a == null ? null : a.Select(t => t.Value<T>()).ToArray();
        }
    }
}
