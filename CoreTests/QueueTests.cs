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
	public class QueueTests
	{
		static KZApplication app;
		static Queue queue;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (queue == null) {
				queue = app.Queue["foo"];
			}
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/queue/local/foo", queue.Url.ToString());
		}

		[Test]
		public void GetInfo()
		{
			var result = queue.GetInfo().Result;
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.IsNotNull(result.Data);
		}

		[Test]
		public void QueueAndDequeue()
		{
			var enqueueResult = queue.Enqueue("foo").Result;
			Assert.AreEqual(HttpStatusCode.Created, enqueueResult.StatusCode);
			var result = queue.Dequeue<string>().Result;
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual("foo", result.Data);
		}

	}
}

