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
    [Tag("initialization")]
    public class Initialization : WorkItemTest
    {
        static KZApplication app;

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
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.IsTrue(app.Initialized);
        }

        [TestMethod]
        [Asynchronous]
        public async Task CanAuthenticateanUserUsing()
        {
            var user = await app.Authenticate(test.Constants.User, test.Constants.Password, test.Constants.Provider);
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.TokenApplication);
            Assert.IsNotNull(user.TokenMarketplace);
            EnqueueTestComplete();
        }
    }
}
