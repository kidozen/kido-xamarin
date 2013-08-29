using System;
using System.Net;

using NUnit.Framework;
using KidoZen;
using System.Threading.Tasks;

namespace KidoZen.Core.Tests
{
	[TestFixture()]
	public class KZApplicationTests
	{
		[Test()]
		public void ShouldInitialize ()
		{
			//arrange
			var app = new KZApplication(Constants.marketplace, Constants.application);
			//act
			app.Initialize().Wait();
			//assert
			Assert.AreEqual(true, app.Initialized);
		}

		[Test()]
		public void ShouldAuthenticate ()
		{
			//arrange
			var app = new KZApplication(Constants.marketplace, Constants.application);
			app.Initialize().Wait();
			//act
			var user = app.Authenticate(Constants.user, Constants.pass, Constants.provider).Result;
			//assert
			Assert.IsNotNull(user.TokenMarketplace);
			Assert.AreEqual(true, app.Authenticated);
		}
	}
}