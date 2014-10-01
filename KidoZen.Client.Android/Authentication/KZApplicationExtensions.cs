using System;
using KidoZen;
using Newtonsoft.Json.Linq;
using Android.Content;

namespace KidoZen.Client.Android
{
	public static class KZApplicationExtensions
	{
		static Context appContext;
		private static string Validate(JObject config, string property)
		{
			var authconfig = config.Value<JObject> ("authConfig");
			var data = authconfig.Value<string>(property);
			if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(property, "Authentication's configuration");
			return data;
		}

		public static void Authenticate (this KZApplication application, Context context)
		{
			appContext = context;
			if (!application.Initialized) application.Initialize ().Wait ();
			var signInUrl = Validate (application.ApplicationConfiguration, "signInUrl");
			startActivity (signInUrl);
		}

		private static void startActivity (String signInUrl) {
			var startPassiveAuth = new Intent(appContext, typeof(PassiveAuthActivity));
			startPassiveAuth.AddFlags(ActivityFlags.NewTask);
			startPassiveAuth.PutExtra("signInUrl", signInUrl);
			appContext.StartActivity(startPassiveAuth);
		}
	}

}

