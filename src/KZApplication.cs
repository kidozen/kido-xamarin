using System;
using System.Linq;
using System.Net;
using System.Threading;
using KidoZen.authentication;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace KidoZen
{
    public class KZApplication 
    {
		public Dictionary<string,string> PassiveAuthenticationInformation;

        readonly Uri marketPlaceUri;
        readonly object sync = new object();
#if !NET45
        public Notification Notification { get; private set; }
        public PubSubChannel PubSubChannel { get; private set; }
#endif
		public string Name{ get; private set; }
		public Queue Queue { get; private set; }
		public Storage Storage { get; private set; }
		public Configuration Configuration { get; private set; }
        public MailSender MailSender { get; private set; }
        public SmsSender SmsSender { get; private set; }
        public Logging Logger { get; private set; }
        public Authentication Authentication { get; private set; }
        public Files Files { get; private set; }
        public Marketplace Marketplace { get; private set; }
        public Service Service { get; private set; }
		public DataSource DataSource { get; private set; }

		public bool IsInitializing { get; internal set; }
		public bool Initialized { get; internal set; }
		public bool Authenticated { get; internal set; }
		public bool IsAuthenticating { get; internal set; }
        public KZUser User { get { return Authentication.User; } }

        public delegate void OnEventHandler (object sender, EventArgs e);

		public event OnEventHandler OnInitialization;
		public event OnEventHandler OnAuthentication;
		internal String applicationKey;

		internal JObject ApplicationConfiguration;

		public KZApplication(string marketPlaceUri, string name, String appicationKey)
        {
            Name = name;
            this.marketPlaceUri = new Uri(marketPlaceUri);
            this.Initialized= false;
            this.Authenticated = false;
			applicationKey = appicationKey;
        }

        public async Task Initialize()
        {
            try
            {
                lock (sync)
                {
                    if (IsInitializing)
                    {
                        throw new Exception("The application is already initializing");
                    }
                    IsInitializing = true;
                    Initialized = false;
                }

                var configurations = await new Uri(string.Format("{0}publicapi/apps?name={1}", marketPlaceUri, Name)).ExecuteAsync<JArray>(this);

                lock (sync)
                {
                    if ( configurations.Data.Count > 0)
                    {
						this.ApplicationConfiguration = configurations.Data[0] as JObject;
                        AllocServices(configurations.Data[0] as JObject);
                        Initialized = true;
                    }
                    else
                    {
						IsInitializing = false;
                        throw new Exception("Can not get application's configuration");
                    }
                    
					if (OnInitialization != null)
					{
						Task.Run(() => 
							{
								OnInitialization.Invoke(this, new EventArgs());
							}
						);
					}

                }
            }
            catch (Exception e)
            {
                IsInitializing = false;
                throw;
            }

            return;
        }

        public Task<ServiceEvent<Stream>> SendByServiceBus(Uri url, string method = "GET", Stream content = null, Dictionary<string, string> headers =null, bool secured = true, TimeSpan? timeout = null, bool cache = false)
        {
            // Validations
            if (secured && this.User == null) throw new Exception("User is not authenticated.");
            if (this.User.TokenServiceBus ==null) throw new Exception("The IP provider does not support Service Bus");

            // default values
            if (string.IsNullOrWhiteSpace(method)) method = "GET";

            // Send request
            return url.ExecuteAsync<Stream>(this, content, method, cache, timeout, headers, (secured ? UseToken.ServiceBus : UseToken.None));
        }
        
        public void SignOut()
        {
            lock (sync)
            {
                if (Authenticated)
                {
                    Authenticated = false;
                    Authentication.SignOut();
                }
            };
        }

        /// <summary>
        /// Returns the unique ID for the device
        /// </summary>
        /// <returns>an hexadecimal string</returns>
        public static string GetDeviceUniqueID()
        {
            byte[] myDeviceID = null;
            
            #if WINDOWS_PHONE
                myDeviceID = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");
            #elif NETFX_CORE
                var token = Windows.System.Profile.HardwareIdentification.GetPackageSpecificToken(null);
                var stream = token.Id.AsStream();
                using (var reader = new BinaryReader(stream))
                {
                    myDeviceID = reader.ReadBytes((int)stream.Length);
                }
            #endif

            return myDeviceID == null ? null : ByteArrayToHex(myDeviceID);
        }

        /// <summary>
        /// Authenticates an user using an identity procider
        /// </summary>
        /// <param name="user">User's name</param>
        /// <param name="password">User's password</param>
        /// <param name="provider">Provider name</param>
        /// <returns>The result could contains an exception or an user instance</returns>
        public async Task<KZUser> Authenticate(string user, string password, string provider = "default")
        {
            try
            {
                lock (sync)
                {
                    if (!Initialized) throw new Exception("The application was not initialized.");
                    if (IsAuthenticating) throw new Exception("The application is already authenticating an user.");
                    IsAuthenticating = true;
                    Authenticated = false;
                }

                var userAuthenticated = await Authentication.Authenticate(user, password, provider);
				
                lock (sync)
                {
                    this.Authenticated = true;
                    IsAuthenticating = false;
                    Task.Run(() =>
                    {
                        OnAuthentication.Invoke(this, new EventArgs());
                    });
                    return userAuthenticated;
                }
            }
            catch(Exception)
            {
                lock (sync)
                {
                    this.Authenticated = false;
                    this.IsAuthenticating = false;
                    throw;
                }
            }
        }

        #region private members

        private void AllocServices(JObject config)
        {
#if !NET45
			Notification = new Notification(this, config.ValueUri("notification"));
			PubSubChannel = new PubSubChannel(this, config.ValueUri("pubsub"), config.ValueUri("ws"));
#endif
			Marketplace = new Marketplace(this, marketPlaceUri);
            Queue = new Queue(this, config.ValueUri("queue"));
            Configuration = new Configuration(this, config.ValueUri("config"));
            SmsSender = new SmsSender(this, config.ValueUri("sms"));
            MailSender = new MailSender(this, config.ValueUri("email"));
            Logger = new Logging(this, config.ValueUri("logging"));
            Storage = new Storage(this, config.ValueUri("storage"));
            Files = new Files(this, config.ValueUri("files"));
            Authentication = new Authentication(marketPlaceUri.Host, config.Value<JObject>("authConfig"), this.Name);
            Service = new Service(this, config.ValueUri("service"));
			DataSource = new DataSource(this, config.ValueUri("datasource"));
        }


        private static string ByteArrayToHex(byte[] barray)
        {
            char[] c = new char[barray.Length * 2];
            byte b;
            for (int i = 0; i < barray.Length; ++i)
            {
                b = ((byte)(barray[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(barray[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }

            return new string(c);
        }

        #endregion
    }
}
