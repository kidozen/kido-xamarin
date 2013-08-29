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
				service = app.Service["echo"];
			}
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/api/services/echo", service.Url.ToString());
		}

		[Test]
		public void Invoke()
		{
			var invokeResult = service.Invoke("send", new { foo = "bar" }).Result;

			Assert.AreEqual(HttpStatusCode.OK, invokeResult.StatusCode);
			Assert.IsNotNull(invokeResult.Data);
			Assert.AreEqual("bar", invokeResult.Data.Value<string>("foo"));
		}
	}
}

