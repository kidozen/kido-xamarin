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
		static DataSource queryDataSrc, invokeDataSrc;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (queryDataSrc == null) {
				queryDataSrc = app.DataSource["test-query"];
			}
			if (invokeDataSrc == null) {
				invokeDataSrc = app.DataSource["test-operation"];
			}
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/api/v2/datasources/test-query", queryDataSrc.Url.ToString());
		}
			
		[Test]
		public void Get()
		{
			var getResult = queryDataSrc.Query().Result;
			Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode);
			Assert.IsNotNull(getResult.Data);
		}

		[Test]
		public void Invoke()
		{
			var invokeResult = invokeDataSrc.Invoke().Result;
			Assert.AreEqual(HttpStatusCode.OK, invokeResult.StatusCode);
			Assert.IsNotNull(invokeResult.Data);
		}

	}
}

