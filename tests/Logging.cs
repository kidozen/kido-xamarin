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
    [Tag("logging")]
    public class Logging : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.Logging logging;

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
            logging = app.Logger;
            EnqueueTestComplete();
        }

        [TestInitialize]
        [Asynchronous]
        public async Task TestInit()
        {
            await logging.Write("foo", LogLevel.LogLevelVerbose);
            await logging.Clear();
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.AppUrl + "/logging", logging.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task Write()
        {
            var result = await logging.Write("foo", LogLevel.LogLevelError);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            EnqueueTestComplete();
        }


        [TestMethod]
        [Asynchronous]
        public async Task QueryDoesFindObjects()
        {
            await logging.Write(new { x = 1 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 2 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 3 }, LogLevel.LogLevelInfo);

            var result = await logging.Query(@"{""data.x"":{""$gt"":1}}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(2, result.Data.Count());
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QueryDoesNotFindObjects()
        {
            await logging.Write(new { x = 1 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 2 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 3 }, LogLevel.LogLevelInfo);

            var result = await logging.Query(@"{""data.x"":{""$gt"":3}}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(0, result.Data.Count());
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QuerySkipAndLimit()
        {
            await logging.Write(new { x = 1 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 2 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 3 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 4 }, LogLevel.LogLevelInfo);

            var result = await logging.Query("{}", @"{""$skip"":2,""$limit"":2}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var items = result.Data.ToArray();
            Assert.AreEqual(2, items.Length);
            var item = items[0].Value<JObject>("data");
            Assert.AreEqual(2, item.Value<int>("x"));
            item = items[1].Value<JObject>("data");
            Assert.AreEqual(3, item.Value<int>("x"));
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QueryCount()
        {
            await logging.Write(new { x = 1 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 2 }, LogLevel.LogLevelInfo);
            await logging.Write(new { x = 3 }, LogLevel.LogLevelInfo);

            var result = await logging.Query(@"{""data.x"":{""$gt"":1}}", @"{""$count"":true}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var items = result.Data.ToArray();
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual(2, items[0]);
            EnqueueTestComplete();
        }
    }
}
