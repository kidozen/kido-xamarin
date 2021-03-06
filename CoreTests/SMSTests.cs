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
	public class SMSTests
	{
		static KZApplication app;
		static SmsSender sms;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application,Constants.applicationKey);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (sms == null) {
				sms = app.SmsSender["+13053038639"];
			}
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/sms", sms.Url.ToString());
		}

		[Test]
		public void Send()
		{
			try
			{
				var result = sms.Send("test").Result;
				Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
				Assert.IsNotNull(result.Data.status);
			}
			catch(Exception e) {
				Assert.Fail ();
			}
		}
	}
}

