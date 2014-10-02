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
		static DataSource queryDataSrc, invokeDataSrc,queryWithData, invokeWithData;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application,Constants.applicationKey);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (queryDataSrc == null) {
				queryDataSrc = app.DataSource["GetCityWeather"];
			}
			if (invokeDataSrc == null) {
				invokeDataSrc = app.DataSource["InvokeCityWeather"];
			}
			if (queryWithData == null) {
				queryWithData = app.DataSource["GetCityWeather"];
			}
			if (invokeWithData == null) {
				invokeWithData = app.DataSource["InvokeCityWeather"];
			}
		}

		//[Test]
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

		[Test]
		public void GetWithData()
		{
			var data = new {city="London"};
			var getResult = queryDataSrc.Query(data).Result;
			Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode);
			Assert.IsNotNull(getResult.Data);
		}

		[Test]
		public void InvokeWithData()
		{
			var invokeResult = invokeWithData.Invoke(new { city = "London" }).Result;
			Assert.AreEqual(HttpStatusCode.OK, invokeResult.StatusCode);
			Assert.IsNotNull(invokeResult.Data);
		}


	}
}

