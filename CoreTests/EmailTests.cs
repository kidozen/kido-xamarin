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
	public class EmailTests
	{
		static KZApplication app;
		static MailSender mail;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (mail == null) {
				mail = app.MailSender;
			}
		}

		[Test]
		public void CanGetAnInstance()
		{
			Assert.AreEqual(Constants.appUrl + "/email", mail.Url.ToString());
		}

		[Test]
		public void Send()
		{
			var result = mail.Send(new Mail {
				from = Constants.email,
				to = Constants.email,
				subject = "test from windows SDK",
				textBody ="does it work?",
				htmlBody = "<html><body><a>does it work?</a></body></html>"
			}).Result;
			Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
		}
	}
}