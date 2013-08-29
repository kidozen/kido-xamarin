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
    [Tag("service")]
    public class Service : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.Service service;

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
            service = app.Service["echo"];
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.AppUrl + "/api/services/echo", service.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task Invoke()
        {
            var invokeResult = await service.Invoke("send", new { foo = "bar" });

            Assert.AreEqual(HttpStatusCode.OK, invokeResult.StatusCode);
            Assert.IsNotNull(invokeResult.Data);
            Assert.AreEqual("bar", invokeResult.Data.Value<string>("foo"));
            EnqueueTestComplete();
        }
    }
}
