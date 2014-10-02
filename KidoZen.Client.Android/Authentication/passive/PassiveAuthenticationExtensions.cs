using System;
using KidoZen;
using Newtonsoft.Json.Linq;
using Android.Content;
using System.Collections;
using System.Collections.Generic;

namespace KidoZen.Client.Android
{
	public static class PassiveAuthenticationExtensions
	{
		private static JObject KidozenApplicationConfig;
		private static Dictionary<string,string> PassiveAuthSettings = new Dictionary<string, string>();

		static Context appContext;
		internal static string Validate(JObject config, string property)
		{
			//var authconfig = config.Value<JObject> ("authConfig");
			var data = config.Value<string>(property);
			if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(property, "Authentication's configuration");
			return data;
		}

		public static void Authenticate (this KZApplication application, Context context)
		{
			appContext = context;
			if (!application.Initialized) application.Initialize ().Wait ();

			KidozenApplicationConfig = application.ApplicationConfiguration;

			PassiveAuthSettings.Add ("grant_type", "refresh_token");
			PassiveAuthSettings.Add ("client_secret", application.applicationKey);
			PassiveAuthSettings.Add ("client_id", 
				Validate (KidozenApplicationConfig, "domain")
			);
			PassiveAuthSettings.Add ("scope", 
				Validate (KidozenApplicationConfig, "applicationScope")
			);
			PassiveAuthSettings.Add ("oauthTokenEndpoint", 
				Validate (KidozenApplicationConfig, "oauthTokenEndpoint")
			);

			var signInUrl = Validate (application.ApplicationConfiguration, "signInUrl");

			var startPassiveAuth = new Intent(appContext, typeof(PassiveAuthActivity));
			startPassiveAuth.AddFlags(ActivityFlags.NewTask);
			startPassiveAuth.PutExtra("signInUrl", signInUrl);
			appContext.StartActivity(startPassiveAuth);
			AuthenticationEventManager.AuthenticationResponseArrived+= (object sender, AuthenticationResponseEventArgs e) => {
				Console.WriteLine("*** Success : " + e.Success);
				if(e.Success) {
					application.PassiveAuthenticationInformation = e.TokenInfo;
				}
				else {
					//TODO: display alert
				}
			};
		}
	}
}

