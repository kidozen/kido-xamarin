using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using KidoZen;


namespace KidoZen.Core.Tests
{
	[TestFixture]
	public class DataSourceTests
	{
		static KZApplication app;
		static DataSource datasource;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (datasource == null) {
				datasource = app.DataSource["test-query"];
			}
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/api/v2/datasources/test-query", datasource.Url.ToString());
		}
			
		[Test]
		public void Get()
		{
			var getResult = datasource.Query<JObject>().Result;
			Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode);
			Assert.IsNotNull(getResult.Data);
		}

		[Test]
		public void GetArray()
		{
			var getResult = datasource.Query<JArray>().Result;
			Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode);
			Assert.IsNotNull(getResult.Data);
		}

	}
}

