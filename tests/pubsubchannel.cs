using System;
using System.Net;
using NUnit.Framework;
using KidoZen;
using TestConstants;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace MonoTouchTests
{
	public class ChannelMessage {
		public string value { get; set; }
	}

	[TestFixture]
	public class PubSubChannelTest
    {
		AutoResetEvent autoEvent = new AutoResetEvent(false);

		[Test]
		public async void Subscribe()
		{
			var app = new KZApplication(Constants.marketplace, Constants.application);
			await app.Initialize();
			var user = await app.Authenticate (Constants.user, Constants.pass, Constants.provider);




			var channel = app.PubSubChannel ["pubsubchanneltest"];


			var rnd = Guid.NewGuid ().ToString ();


			channel.Susbscribe<ChannelMessage> (msg=> {
				Assert.Equals(rnd, msg.value);
				autoEvent.Set();
			}, err=>{
				if(err!=null) Assert.Fail();
			}) ;
		
			var message = new ChannelMessage { value = rnd};

			var response = channel.Publish (message);
			//Assert.AreEqual(HttpStatusCode.Created, response.Result.StatusCode);

			if (autoEvent.WaitOne (60000)) {
				Assert.True (1 == 1);
			} else {
				Assert.Fail();
			}
		}

		void onMessage (ChannelMessage obj)
		{
			throw new NotImplementedException ();
		}

		void onError (Exception obj)
		{
			Assert.Fail ();
		}


    }
}
