using System;
using KidoZen;
using Newtonsoft.Json.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Kidozen.Client.iOS
{
	public static class KZApplicationExtensions
	{
		private static string Validate(JObject config, string property)
		{
			var authconfig = config.Value<JObject> ("authConfig");
			var data = authconfig.Value<string>(property);
			if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(property, "Authentication's configuration");
			return data;
		}

		public static void Authenticate (this KZApplication application)
		{
			application.Initialize ().Wait ();
			var signInUrl = Validate (application.ApplicationConfiguration, "signInUrl");
			var authController = new PassiveAuthViewController (signInUrl);
			var wv = new UINavigationController (authController);
			UIApplication.SharedApplication.Delegate.Window.RootViewController.PresentViewController (wv, true, 
				new NSAction(()=>{
					Console.WriteLine("cargando");
				}));

		}
	}
}

