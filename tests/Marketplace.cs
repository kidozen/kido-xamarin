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
    [Tag("marketplace")]
    public class Marketplace : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.Marketplace marketplace;

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
            marketplace = app.Marketplace;
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.MarketplaceUrl + "/", marketplace.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task GetApplication()
        {
            var result = await marketplace.GetApplication(test.Constants.AppName);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(test.Constants.AppName, result.Data.Name);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task GetApplications()
        {
            var result = await marketplace.GetApplications();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Data.Where(a => a.Name == test.Constants.AppName).SingleOrDefault()); ;
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task GetApplicationNames()
        {
            var result = await marketplace.GetApplicationNames();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Data.Where(n => n == test.Constants.AppName).SingleOrDefault()); ;
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task GetUserApplications()
        {
            var result = await marketplace.GetUserApplications();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task GetUserApplicationNames()
        {
            var result = await marketplace.GetUserApplicationNames();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            EnqueueTestComplete();
        }
    }
}
