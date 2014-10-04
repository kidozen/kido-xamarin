using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Xamarin.Forms.Platform.Android;
using KidoZen.Client.Android;

namespace Todo.Android
{
	[Activity (Label = "Todo.Android.Android", MainLauncher = true)]
	public class MainActivity : AndroidActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			Xamarin.Forms.Forms.Init (this, bundle);

			App.AndroidContext = this.ApplicationContext;
			SetPage (App.GetLoginPage (onKidoZenPassiveAuthenticationFinish));
		}

		internal void onKidoZenPassiveAuthenticationFinish(Object sender, EventArgs e ) {
			var isAuthenticated =(e as AuthenticationResponseEventArgs).Success;
			if(isAuthenticated) {
				Application.SynchronizationContext.Post(_=>
					SetPage (App.GetMainPage ())
					,null);
			}
		}
	}
}

