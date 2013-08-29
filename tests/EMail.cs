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
    [Tag("email")]
    public class EMail : WorkItemTest
    {
        static KZApplication app;
        static KidoZen.MailSender mail;

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
            mail = app.MailSender;
            EnqueueTestComplete();
        }

        [TestMethod]
        public void CanGetAnInstance()
        {
            Assert.AreEqual(test.Constants.AppUrl + "/email", mail.Url.ToString());
        }

        [TestMethod]
        [Asynchronous]
        public async Task Send()
        {
            var result = await mail.Send(new Mail
            {
                from = test.Constants.Email,
                to = test.Constants.Email,
                subject = "test from windows SDK",
                textBody ="does it work?",
                htmlBody = "<html><body><a>does it work?</a></body></html>"
            });
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            EnqueueTestComplete();
        }
    }
}
