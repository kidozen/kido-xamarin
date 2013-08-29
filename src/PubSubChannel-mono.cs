using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;
using WebSocketSharp;
using System.IO;

namespace KidoZen
{
    public partial class PubSubChannel
    {
        public class PubSubException : Exception
        {
            public ushort Code { get; private set;}
            public PubSubException(ushort code, string message): base(message)
            {
                Code = code;
            }
        }

        Uri endpoint;
        Uri wsEndpoint;
        KZApplication app;
		WebSocket webSocket;
        
        public Uri Url { get; private set; }
        public string Name { get; private set; }

        internal PubSubChannel(KZApplication app, Uri endpoint, Uri wsEndpoint):
            this(app, endpoint, wsEndpoint, null)
        {
        }

        internal PubSubChannel(KZApplication app, Uri endpoint, Uri wsEndpoint, string name)
        {
            if (app == null) throw new ArgumentNullException("app");
            this.app = app;

            this.Url = endpoint.Concat(name);
            this.Name = name;
            this.endpoint = endpoint;
            this.wsEndpoint = wsEndpoint;
        }

        public PubSubChannel this[string name]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
                return new PubSubChannel(app, endpoint, wsEndpoint, name);
            }
        }

        public async Task<ServiceEvent<JToken>> Publish<T>(T message)
        {
            return await Url.ExecuteAsync<JToken>(app, message.ToJToken(), "POST");
        }

        public void Unsubscribe()
        {
            if (webSocket != null)
            {
                webSocket.Dispose();
                webSocket = null;
            }
        }

        public async void Susbscribe<T>(Action<T> onMessage, Action<Exception> onError)
        {
            if (onMessage == null) throw new ArgumentNullException("onMessage");
			var newWebSocket = new WebSocket(wsEndpoint.ToString());

            newWebSocket.OnMessage += (sender, args) =>
            {
				var colons = args.Data.IndexOf("::") + 2;
				var lengh = args.Data.Length - colons;
				var message = args.Data.Substring(colons, lengh);
				onMessage(JsonConvert.DeserializeObject<T>(message));
            };

            newWebSocket.OnClose += (sender, args) =>
            {
                var webSocketClosed = Interlocked.Exchange(ref webSocket, null);
                if (webSocketClosed != null)
                {
                    webSocketClosed.Dispose();
                    webSocket = null;
                }

                if (onError != null && args.Code != 1000) // 1000 = OK
                {
                    onError(new PubSubException(args.Code, args.Reason)); 
                }
            };

			newWebSocket.OnOpen += (object sender, EventArgs e) => {
				var connect = ("bindToChannel::{\"application\":\"local\", \"channel\":\"" + Name + "\"}");
				newWebSocket.Send(connect);
			};
			newWebSocket.Connect();
        }
    }
}
