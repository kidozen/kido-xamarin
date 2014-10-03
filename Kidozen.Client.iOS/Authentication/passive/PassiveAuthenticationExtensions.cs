using System;
using KidoZen;
using Newtonsoft.Json.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

namespace Kidozen.Client.iOS
{
	public static class PassiveAuthenticationExtensions
	{
		private static JObject KidozenApplicationConfig;
		private static Dictionary<string,string> PassiveAuthSettings = new Dictionary<string, string>();

		internal static string Validate(JObject config, string property)
		{
			var data = config.Value<string>(property);
			if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(property, "Authentication's configuration");
			return data;
		}

		public static void Authenticate (this KZApplication application, KZApplication.OnEventHandler onAuthFinish)
		{
			PassiveAuthSettings = new Dictionary<string, string>();
			if (!application.Initialized) application.Initialize ().Wait ();

			KidozenApplicationConfig = application.ApplicationConfiguration;

			PassiveAuthSettings.Add ("grant_type", "refresh_token");
			PassiveAuthSettings.Add ("client_secret", application.applicationKey);
			PassiveAuthSettings.Add ("client_id", Validate (KidozenApplicationConfig, "domain"));
			PassiveAuthSettings.Add ("scope", Validate (KidozenApplicationConfig.Value<JObject> ("authConfig"), "applicationScope"));
			PassiveAuthSettings.Add ("oauthTokenEndpoint", Validate (KidozenApplicationConfig.Value<JObject> ("authConfig"), "oauthTokenEndpoint"));

			var signInUrl = Validate (KidozenApplicationConfig.Value<JObject> ("authConfig"), "signInUrl");
			var authController = new PassiveAuthViewController (signInUrl);
			var wv = new UINavigationController (authController);
			authController.AuthenticationResponseArrived+= (object sender, AuthenticationResponseEventArgs e) => {
				Console.WriteLine("*** Success : " + e.Success);
				application.Authenticated = e.Success;
				if(e.Success) {
					application.PassiveAuthenticationInformation = new Dictionary<string,string>( e.TokenInfo);
				}
				else {
					//TODO: display alert
				}
				if(onAuthFinish!=null) {
					onAuthFinish.Invoke(application, e);
				}
			};
			UIApplication.SharedApplication.Delegate.Window.RootViewController.PresentViewController (wv, true, 
				new NSAction(()=>{
					Console.WriteLine("loading");
				}));

			//OnPassiveAuthentication.Invoke(application, new PassiveAuthenticationEventArgs {Success=e.Success});
		}
	}
}

