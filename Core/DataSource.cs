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

		public async Task<ServiceEvent<JObject>> Invoke(string method)
		{
			return await Invoke<JObject>(method, new JObject());
		}

		public async Task<ServiceEvent<JObject>> Invoke<T>(string method, T args)
		{
			if (string.IsNullOrWhiteSpace(method)) throw new ArgumentNullException("method");

			Validate();
			var endpoint = Url.Concat(method);
			return await endpoint.ExecuteAsync<JObject>(app, args.ToJToken(), method="POST");
		}

		[Obsolete("InvokeArray is goint to be deprecated in next versions of KidoZen datasources.")]
		public async Task<ServiceEvent<JArray>> InvokeArray<T>(string method, T args)
		{
			if (string.IsNullOrWhiteSpace(method)) throw new ArgumentNullException("method");

			Validate();
			var endpoint = Url.Concat(method);
			return await endpoint.ExecuteAsync<JArray>(app, args.ToJToken(), method="POST");
		}

		public async Task<ServiceEvent<JObject>> Query<T>()
		{
			Validate();
			return await Url.ExecuteAsync<JObject>(app);
		}

		[Obsolete("QueryArray is goint to be deprecated in next versions of KidoZen datasources.")]
		public async Task<ServiceEvent<JArray>> QueryArray<T>()
		{
			Validate();
			return await Url.ExecuteAsync<JArray>(app);
		}
	}
}

