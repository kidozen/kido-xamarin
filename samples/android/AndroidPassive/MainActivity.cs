using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using System.Net;

using Kidozen;
using Kidozen.Android;


namespace AndroidPassive
{
	[Activity (Label = "AndroidPassive", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
				ServicePointManager.ServerCertificateValidationCallback = (x,w,y,z)=> true;

				// Perform any additional setup after loading the view, typically from a nib.
				var app = new KidoApplication("loadtests.qa.kidozen.com", "tasks", "NuSSOjO4d/4Zmm+lbG3ntlGkmeHCPn8x20cj82O4bIo=");
				app.Authenticate(this).ContinueWith(r=> {
					System.Diagnostics.Debug.WriteLine ("doPassive: " + r);

					var ds = app.DataSource("GetCityWeather");
					var result = ds.Query().Result;
					System.Diagnostics.Debug.WriteLine ("ViewResult:" + result);
				});
				//System.Diagnostics.Debug.WriteLine (app.DoPassiveAuth(this).Result);

			};
		}
	}
}


