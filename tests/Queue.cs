using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [Tag("queue")]
    public class Queue : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.Queue queue;

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
            queue = app.Queue["foo"];
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.AppUrl + "/queue/local/foo", queue.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task GetInfo()
        {
            var result = await queue.GetInfo();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Data);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QueueAndDequeue()
        {
            var enqueueResult = await queue.Enqueue("foo");
            Assert.AreEqual(HttpStatusCode.Created, enqueueResult.StatusCode);
            var result = await queue.Dequeue<string>();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual("foo", result.Data);
            EnqueueTestComplete();
        }
    }
}
