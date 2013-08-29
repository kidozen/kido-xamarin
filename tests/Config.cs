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
    [Tag("config")]
    public class Config : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.Configuration config;

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
            config = app.Configuration["foo"];
            EnqueueTestComplete();
        }

        [TestInitialize]
        [Asynchronous]
        public async Task TestInit()
        {
            await config.Save("bar");
            await config.Delete();
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.AppUrl + "/config/foo", config.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task Save()
        {
            var result = await config.Save("bar");
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task Get()
        {
            var saveResult = await config.Save("bar");
            var result = await config.Get<string>();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual("bar", result.Data);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task Delete()
        {
            var saveResult = await config.Save("bar");
            var result = await config.Delete();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            EnqueueTestComplete();
        }
    }
}
