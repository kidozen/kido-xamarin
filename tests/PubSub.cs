using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using KidoZen;

#if WINDOWS_PHONE
using Microsoft.Phone.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

namespace test
{
    [TestClass]
    [Tag("pubsub")]
    public class PubSub : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.PubSubChannel pubsub;

        [ClassInitialize]
        [Asynchronous]
#if WINDOWS_PHONE
        public async Task ClassInit()
#elif NETFX_CORE
        static public async Task ClassInit(TestContext context)
#endif
        {
            app = new KZApplication(test.Constants.MarketplaceUrl, test.Constants.AppName);
            await app.Initialize();
            var user = await app.Authenticate(test.Constants.User, test.Constants.Password, test.Constants.Provider);
            pubsub = app.PubSubChannel["test"];
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.AppUrl + "/pubsub/local/test", pubsub.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task PublishSubcribeUnsubscribe()
        {
            var allDone = new ManualResetEvent(false);
            // send a first publish before is subscribed
            var first = await pubsub.Publish(new { x = 1 } );
            Assert.AreEqual(HttpStatusCode.Created, first.StatusCode);

            // subscribe to the channel
            pubsub.Susbscribe<JObject>((message) =>
                {
                    // validate that we received second publish
                    Assert.AreEqual(2, message.Value<int>("x"));
                    allDone.Set();
                    EnqueueTestComplete();
                }, (exception) =>
                {
                    throw exception;
                });

            Sleep(5000);

            // do another publish
            var second = await pubsub.Publish(new { x = 2 });
            Assert.AreEqual(HttpStatusCode.Created, first.StatusCode);
            if (!allDone.WaitOne(5000)) Assert.Fail("Timeout");
        }

        static private void Sleep(int ms)
        {
            new ManualResetEvent(false).WaitOne(ms);
        }

    }
}
