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
    [Tag("file")]
    public class File : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.Files files;
        static MemoryStream file;

        [ClassInitialize]
        [Asynchronous]
#if WINDOWS_PHONE
        public async Task ClassInit()
#elif NETFX_CORE
        static public async Task ClassInit(TestContext context)
#endif
        {
            var bytes = UTF8Encoding.UTF8.GetBytes("sample file");
            file = new MemoryStream();
            file.Write(bytes, 0, bytes.Length);
            file.Flush();

            app = new KZApplication(test.Constants.MarketplaceUrl, test.Constants.AppName);
            await app.Initialize();
            var user = await app.Authenticate(test.Constants.User, test.Constants.Password, test.Constants.Provider);
            files = app.Files;
            EnqueueTestComplete();
        }

        [TestInitialize]
        [Asynchronous]
        public async Task TestInit()
        {
            file.Seek(0, SeekOrigin.Begin);
            await files.Delete("/");
           EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.AppUrl + "/uploads", files.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task Upload()
        {
            var result = await files.Upload(file, "/foo.txt");
            Assert.IsTrue(result.Data);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task Download()
        {
            var uploadResult = await files.Upload(file, "/foo.txt");
            var result = await files.Download("/foo.txt");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsInstanceOfType(result.Data, typeof(MemoryStream));
            var stream = (MemoryStream)result.Data;
            Assert.AreEqual(file.Length, stream.Length);
            Assert.AreEqual(
                UTF8Encoding.UTF8.GetString(file.ToArray(), 0, (int)file.Length),
                UTF8Encoding.UTF8.GetString(stream.ToArray(), 0, (int)file.Length)
            );
            EnqueueTestComplete();
        }


        [TestMethod]
        [Asynchronous]
        public async Task Browse()
        {
            var uploadResult = await files.Upload(file, "/foo.txt");
            var result = await files.Browse("/");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, result.Data.files
                .Where(f => f.name == "foo.txt")
                .Count());
            EnqueueTestComplete();
        }

        [TestMethod]
        [Asynchronous]
        public async Task Delete()
        {
            var app = new KZApplication(test.Constants.MarketplaceUrl, test.Constants.AppName);
            await app.Initialize();
            var user = await app.Authenticate(test.Constants.User, test.Constants.Password, test.Constants.Provider);
            var files = app.Files;
            var uploadResult = await files.Upload(file, "/foo.txt");
            var result = await files.Browse("/");
            Assert.AreEqual(1, result.Data.files
                .Where(f => f.name == "foo.txt")
                .Count());

            var delResult = await files.Delete("/foo.txt");
            Assert.AreEqual(HttpStatusCode.NoContent, delResult.StatusCode);
            Assert.IsTrue(delResult.Data);

            result = await files.Browse("/");
            Assert.AreEqual(0, result.Data.files
                .Where(f => f.name == "foo.txt")
                .Count());
            EnqueueTestComplete();
        }

    }
}
