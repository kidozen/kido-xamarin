using System;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

			this.endpoint = endpoint;
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
			return await Invoke<JObject>(string.Empty, new JObject());
		}

		private async Task<ServiceEvent<JObject>> Invoke<T>(string method, T args)
		{
			Validate();
			return await Url.ExecuteAsync<JObject>(app, args.ToJToken(), method="POST");
		}

		public async Task<ServiceEvent<JObject>> Query()
		{
			Validate();
			return await Url.ExecuteAsync<JObject>(app);
		}
	}
}

