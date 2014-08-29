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
	public class ServiceTests
	{
		static KZApplication app;
		static Service service;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (service == null) {
				service = app.Service["weather"];
			}
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/api/services/weather", service.Url.ToString());
		}

		[Test]
		public void Invoke()
		{
			var invokeResult = service.Invoke("get", new { qs = new { q = "Buenos Aires"} }).Result;

			Assert.AreEqual(HttpStatusCode.OK, invokeResult.StatusCode);
			Assert.IsNotNull(invokeResult.Data);
			Assert.IsTrue( invokeResult.Data.ToString().Contains("Buenos Aires") );
		}
	}
}

