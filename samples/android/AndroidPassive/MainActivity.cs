using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using System.Net;

using KidoZen;
using KidoZen.Client.Android;


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
				var app = new KZApplication("https://loadtests.qa.kidozen.com", "tasks", false);
				app.Authenticate(this);
				//System.Diagnostics.Debug.WriteLine (app.DoPassiveAuth(this).Result);

			};
		}
	}
}


