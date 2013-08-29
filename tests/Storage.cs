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
    [Tag("storage")]
    public class Storage : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.Storage objectSet;

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
            objectSet = app.Storage["test"];
            EnqueueTestComplete();
        }

        [TestInitialize]
        [Asynchronous]
        public async Task TestInit()
        {
            var insResult = await objectSet.Insert(new { Foo = "foo" });
            var dropResult = await objectSet.Drop();
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.AppUrl + "/storage/local/test", objectSet.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task GetById()
        {
            var insResult = await objectSet.Insert(new { Foo = "foo" });
            var getResult = await objectSet.Get(insResult.Data._id);
            Assert.IsNotNull(getResult.Data);
            Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode);
            Assert.AreEqual("foo", getResult.Data.Value<string>("Foo"));
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task Update()
        {
            var insResult = await objectSet.Insert(new { Foo = "foo" });
            var updResult = await objectSet.Update(insResult.Data._id, new { Foo = "bar", _metadata = insResult.Data._metadata });
            var getResult = await objectSet.Get(insResult.Data._id);
            Assert.IsNotNull(getResult.Data);
            Assert.AreEqual("bar", getResult.Data.Value<string>("Foo"));
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task Drop()
        {
            var insResult = await objectSet.Insert(new { Foo = "foo" });
            var dropResult = await objectSet.Drop();
            Assert.AreEqual(HttpStatusCode.OK, dropResult.StatusCode);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task Delete()
        {
            var insResult = await objectSet.Insert(new { Foo = "foo" });
            var delResult = await objectSet.Delete(insResult.Data._id);
            Assert.AreEqual(HttpStatusCode.OK, delResult.StatusCode);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QueryDoesFindObjects()
        {
            var insResult = await objectSet.Insert(new { x = 1 });
            insResult = await objectSet.Insert(new { x = 2 });
            insResult = await objectSet.Insert(new { x = 3 });

            var result = await objectSet.Query(@"{""x"":{""$gt"":1}}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(2, result.Data.Count);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QueryDoesNotFindObjects()
        {
            var insResult = await objectSet.Insert(new { x = 1 });
            insResult = await objectSet.Insert(new { x = 2 });
            insResult = await objectSet.Insert(new { x = 3 });

            var result = await objectSet.Query(@"{""x"":{""$gt"":3}}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(0, result.Data.Count);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QuerySkipAndLimit()
        {
            var insResult = await objectSet.Insert(new { x = 1 });
            insResult = await objectSet.Insert(new { x = 2 });
            insResult = await objectSet.Insert(new { x = 3 });
            insResult = await objectSet.Insert(new { x = 4 });

            var result = await objectSet.Query("{}", @"{""$skip"":1,""$limit"":2}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(2, result.Data.Count);
            Assert.AreEqual(2, result.Data[0].Value<int>("x"));
            Assert.AreEqual(3, result.Data[1].Value<int>("x"));
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QueryCount()
        {
            var insResult = await objectSet.Insert(new { x = 1 });
            insResult = await objectSet.Insert(new { x = 2 });
            insResult = await objectSet.Insert(new { x = 3 });

            var result = await objectSet.Query(@"{""x"":{""$gt"":1}}", @"{""$count"":true}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual(2, result.Data[0]);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task QuerySelect()
        {
            var insResult = await objectSet.Insert(new { x = 1, y = 2, z = 3 });
            var result = await objectSet.Query(fields: @"{""y"":1}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, result.Data.Count);
            var obj = (JObject)result.Data[0];
            Assert.AreEqual(2, obj.Properties().Count()); // _id and y
            Assert.AreEqual(2, obj.Value<int>("y"));
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task All()
        {
            var insResult = await objectSet.Insert(new { x = 1 });
            insResult = await objectSet.Insert(new { x = 2 });
            insResult = await objectSet.Insert(new { x = 3 });

            var result = await objectSet.All();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(3, result.Data.Count);
            EnqueueTestComplete();
        }


        [TestMethod]
        [Asynchronous]
        public async Task Insert()
        {
            var result = await objectSet.Insert(new { Foo = "foo" });
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(result.Data);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Data._id));
            Assert.IsNotNull(result.Data._metadata);
            EnqueueTestComplete();
        }
    }
}
