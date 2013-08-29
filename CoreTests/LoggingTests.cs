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
	public class LoggingTests
	{
		static KZApplication app;
		static Logging logging;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (logging == null) {
				logging = app.Logger;
			}

			logging.Write("foo", LogLevel.LogLevelVerbose).Wait();
			logging.Clear().Wait();
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/logging", logging.Url.ToString());
		}

		[Test]
		public void Write()
		{
			var result = logging.Write("foo", LogLevel.LogLevelError).Result;
			Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
		}


		[Test]
		public void QueryDoesFindObjects()
		{
			logging.Write(new { x = 1 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 2 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 3 }, LogLevel.LogLevelInfo).Wait();

			var result = logging.Query(@"{""data.x"":{""$gt"":1}}").Result;
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual(2, result.Data.Count());
		}

		[Test]
		public void QueryDoesNotFindObjects()
		{
			logging.Write(new { x = 1 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 2 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 3 }, LogLevel.LogLevelInfo).Wait();

			var result = logging.Query(@"{""data.x"":{""$gt"":3}}").Result;
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual(0, result.Data.Count());			
		}

		[Test]
		public void QuerySkipAndLimit()
		{
			logging.Write(new { x = 1 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 2 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 3 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 4 }, LogLevel.LogLevelInfo).Wait();

			var result = logging.Query("{}", @"{""$skip"":2,""$limit"":2}").Result;
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			var items = result.Data.ToArray();
			Assert.AreEqual(2, items.Length);
			var item = items[0].Value<JObject>("data");
			Assert.AreEqual(2, item.Value<int>("x"));
			item = items[1].Value<JObject>("data");
			Assert.AreEqual(3, item.Value<int>("x"));
		}

		[Test]		
		public void QueryCount()
		{
			logging.Write(new { x = 1 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 2 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 3 }, LogLevel.LogLevelInfo).Wait();

			var result = logging.Query(@"{""data.x"":{""$gt"":1}}", @"{""$count"":true}").Result;
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			var items = result.Data.ToArray();
			Assert.AreEqual(1, items.Length);
			Assert.AreEqual(2, items[0].Value<int>());
		}
	}
}

