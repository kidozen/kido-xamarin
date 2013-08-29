using System;
using NUnit.Framework;
using KidoZen;
using TestConstants;
using System.Threading.Tasks;

namespace MonoTouchTests
{
	[TestFixture]
	public class KidoApplication
	{
		[Test]
		public async void ShouldAuthenticate ()
		{
			var app = new KZApplication(Constants.marketplace, Constants.application);
			await app.Initialize ();
			var t = app.Authenticate(Constants.user, Constants.pass, Constants.provider);
			await t.ContinueWith ((r) => {
				Assert.IsNotNull(r.Result.TokenMarketplace);
			});
		}

		[Test]
		public async void ShouldReturnIsInitialized ()
		{
			var app = new KZApplication(Constants.marketplace, Constants.application);
			await app.Initialize ();

			Assert.That (app.Initialized, Is.True);
		}
		/*
		public async void AuthenticationShouldThrowException ()
		{
			var app = new KZApplication(Constants.marketplace, Constants.application);
			await app.Initialize ();
			Assert.That(async () => await app.Authenticate(Constants.user, "", Constants.provider), Throws.InstanceOf<Exception>());
		}
		*/
	}
}
