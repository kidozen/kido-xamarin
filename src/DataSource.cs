using System;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace KidoZen
{
	public class DataSource
	{
		KZApplication app;
		Uri endpoint;

		public Uri Url { get; private set; }
		public String Name { get; private set; }

		internal DataSource(KZApplication app, Uri endpoint) :
		this(app, endpoint, null)
		{
		}

		internal DataSource(KZApplication app, Uri endpoint, string name)
		{
			if (app == null) throw new ArgumentNullException("app");
			if (endpoint == null) throw new ArgumentNullException("endpoint");
			this.app = app;

			this.endpoint = endpoint ;
			Name = name;
			if (name != null) Url = endpoint.Concat(name);
		}

		public DataSource this[string name]
		{
			get
			{
				if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
				return new DataSource(app, endpoint, name);
			}
		}

		private void Validate()
		{
			if (Name == null) throw new Exception(@"Datasource name is missing. Use Datasource[""name""]");
		}

		public async Task<ServiceEvent<JObject>> Invoke()
		{
			return await Invoke<JObject>( new JObject());
		}

		public async Task<ServiceEvent<JObject>> Invoke<T>( T args)
		{
			Validate();
			var obj = args.ToJToken() as JObject;
			if (obj == null) throw new ArgumentException("Value must be an object, it could not be a value type.", "value");

			//return await Url.ExecuteAsync<JObject>(app, obj, "POST");

			return await new Uri(Url,"").ExecuteAsync<JObject>(app, obj, "POST");

		}

		public async Task<ServiceEvent<JObject>> Query()
		{
			Validate();
			return await Url.ExecuteAsync<JObject>(app);
		}

		public async Task<ServiceEvent<JObject>> Query<T>( T args)
		{
			var token = args.ToJToken ().ToString ().Replace (System.Environment.NewLine, string.Empty);
			var source = JsonConvert.DeserializeObject<Dictionary<string, object>>(token);
			var pairs = source.Select(x => string.Format("{0}={1}", x.Key, WebUtility.UrlEncode(x.Value.ToString())));		
			var qs = string.Join("&", pairs).Replace (System.Environment.NewLine, string.Empty).Replace(" ",string.Empty);

			var url = new Uri(Url, string.Format("?{0}",qs));

			Validate();
			return await url.ExecuteAsync<JObject>(app);
		}
	}
}

