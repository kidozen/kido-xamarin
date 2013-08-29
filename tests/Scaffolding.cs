using System;
using System.IO;

#if WINDOWS_PHONE
using Microsoft.Phone.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NETFX_CORE
#endif

namespace test
{
    class Constants
    {
        public const string MarketplaceUrl = "Marketplace's URL";
        public const string AppName = "name of a Kidozen application";
        public const string User = "a valid kidozen user name";
        public const string Password = "user password";
        public const string Provider = "User source to be used";
        public const string Email = "user email address";

        static public string AppUrl {
            get {
                var marketplace = MarketplaceUrl.ToLower();
                var url = new Uri(marketplace);
                return marketplace.Replace(url.Host, AppName + "." + url.Host);
            }
        }
    }

#if NETFX_CORE
    // Fake classes for winrt
    public class WorkItemTest
    {
        static public void EnqueueTestComplete()
        {
        }
    }

    public class AsynchronousAttribute : Attribute
    {
    }

    public class TagAttribute : Attribute
    {
        public TagAttribute(string name)
        {
        }
    }

#elif WINDOWS_PHONE

#endif
}

