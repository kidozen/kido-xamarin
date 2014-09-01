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

		/*
		This version of the SDK uses the old endpoint of logging, the service ignores the query and returns all log entries
		*/
		[Test]		
		public void QueryCount()
		{
			logging.Clear ().Wait ();

			logging.Write(new { x = 1 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 2 }, LogLevel.LogLevelInfo).Wait();
			logging.Write(new { x = 3 }, LogLevel.LogLevelInfo).Wait();
			var q = "{\"query\":{\"filtered\":{\"filter\":{\"range\":{\"data.x\":{\"gte\":2}}}}}}";

			var result = logging.Query(q).Result;
			Assert.AreEqual (3, result.Data.ToList().Count());
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		}
	}
}

