using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;


namespace KidoZen
{
	public partial class Notification
	{
		#region inner type

		public class SubscriptionBody
		{
			public SubscriptionBody(string deviceId, string registrationId)
			{
				platform = "gcm";
				subscriptionId = registrationId;
				this.deviceId = deviceId;
			}

			public string platform;
			public string subscriptionId;
			public string deviceId;
		}

		#endregion

		private const string CHANNEL_PREFIX = "kidozen.";
		private KZApplication app;

		internal string deviceId = KZApplication.GetDeviceUniqueID();

		public string ChannelName { get; private set; }
		public Uri Url { get; private set; }

		internal Notification(KZApplication app, Uri endpoint)
		{
			if (app == null) throw new ArgumentNullException("app");
			this.app = app;

			this.Url = endpoint;
			this.ChannelName = CHANNEL_PREFIX + app.Name;
		}

		#region private members


		private async Task<ServiceEvent<JToken>> doSubscribe(string channelName,string deviceId, string registrationId)
		{
			var resource = "/subscriptions/"
				+ WebUtility.UrlEncode(app.Name)
					+ (string.IsNullOrWhiteSpace(channelName) ? "" : "/" + WebUtility.UrlEncode(channelName));
			var body = new SubscriptionBody(deviceId, registrationId);
			return await Url.Concat(resource).ExecuteAsync<JToken>(app, body.ToJToken(), "POST");
		}

		private async Task<ServiceEvent<JToken>> doUnsubscribe(string channelName)
		{
			var resource = "/subscriptions/"
				+ WebUtility.UrlEncode (app.Name) + "/"
					+ WebUtility.UrlEncode (channelName);

			return await Url.Concat(resource).ExecuteAsync<JToken>(app, method:"DELETE")
				.ContinueWith<ServiceEvent<JToken>>(se =>
				                                    {
					var result = GetSubscriptionsCount().Result;
					return se.Result;
				});
		}

		#endregion


		public async Task<ServiceEvent<string[]>> GetSubscriptions()
		{
			var resource = "/devices/" + WebUtility.UrlEncode(deviceId) + "/" + WebUtility.UrlEncode(app.Name);

			var task = await Url.Concat(resource).ExecuteAsync<JArray>(app);

			string[] result = null;
			if ((int)(task.StatusCode) < 300)
			{
				result = task.Data
					.Select(subscription => subscription.Value<string>("channelName"))
						.ToArray();
			}

			return task.Clone<string[]>(result);
		}



		public async Task<ServiceEvent<int>> GetSubscriptionsCount()
		{
			var resource = "/devices/" + WebUtility.UrlEncode(deviceId) + "/" + WebUtility.UrlEncode(app.Name) + "?count=true";
			return await Url.Concat(resource).ExecuteAsync<int>(app);
		}

		public async Task<ServiceEvent<JToken>> Subscribe(string channelName, string deviceId, string registrationId)
		{
			if (string.IsNullOrWhiteSpace(channelName)) throw new ArgumentNullException("channelName");
			return await doSubscribe(channelName, deviceId, registrationId);
		}

		public async Task<ServiceEvent<JToken>> Unsubscribe(string channelName)
		{
			if (string.IsNullOrWhiteSpace(channelName)) throw new ArgumentNullException("channelName");
			return await doUnsubscribe(channelName);
		}

		public async Task<ServiceEvent<JToken>> Push(string channelName, NotificationData data)
		{
			var resource = "/push/"
				+ WebUtility.UrlEncode(app.Name) + "/"  
					+ WebUtility.UrlEncode(channelName);

			return await Url.Concat(resource).ExecuteAsync<JToken>(app, data.ToJToken(), "POST");
		}
	}
}