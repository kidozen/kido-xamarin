using System;
using System.Net;
using NUnit.Framework;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using KidoZen;

namespace KidoZen.Core.Tests
{
	[TestFixture]
	public class ConfigTests
	{
		static KZApplication app;
		static KidoZen.Configuration config;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (config == null) {
				config = app.Configuration["foo"];
			}
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/config/foo", config.Url.ToString());
		}

		[Test]
		public void Save()
		{
			var result = config.Save("bar").Result;
			Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
		}

		[Test]
		public void Get()
		{
			var saveResult = config.Save("bar").Result;
			var result = config.Get<string>().Result;
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("bar", result.Data);
			
		}

		[Test]
		public void Delete()
		{
			var saveResult = config.Save("bar").Result;
			var result = config.Delete().Result;
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		}
	}
}

