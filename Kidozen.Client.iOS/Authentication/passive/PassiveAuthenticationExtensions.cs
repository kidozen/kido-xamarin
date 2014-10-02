using System;
using KidoZen;
using Newtonsoft.Json.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections;
using System.Collections.Generic;

namespace Kidozen.Client.iOS
{
	public static class PassiveAuthenticationExtensions
	{
		private static JObject KidozenApplicationConfig;
		private static Dictionary<string,string> PassiveAuthSettings = new Dictionary<string, string>();

		internal static string Validate(JObject config, string property)
		{
			//var authconfig = config.Value<JObject> ("authConfig");
			var data = config.Value<string>(property);
			if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(property, "Authentication's configuration");
			return data;
		}

		public static void Authenticate (this KZApplication application)
		{
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

			var signInUrl = Validate (KidozenApplicationConfig.Value<JObject> ("authConfig"), "signInUrl");
			var authController = new PassiveAuthViewController (signInUrl);
			var wv = new UINavigationController (authController);
			UIApplication.SharedApplication.Delegate.Window.RootViewController.PresentViewController (wv, true, 
				new NSAction(()=>{
					Console.WriteLine("loading");
				}));
			authController.AuthenticationResponseArrived+= (object sender, AuthenticationResponseEventArgs e) => {
				Console.WriteLine("*** Success : " + e.Success);
				if(e.Success) {
					application.PassiveAuthenticationInformation = new Dictionary<string,string>( e.TokenInfo);
				}
				else {
					//TODO: display alert
				}
			};
		}
	}
}

