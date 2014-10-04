using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Xamarin.Forms;
using Todo;
using Kidozen.Client.iOS;

namespace Passive
{
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			Forms.Init ();		
			Window.RootViewController = App.GetLoginPage (onKidoZenPassiveAuthenticationFinish).CreateViewController ();
			return true;
		}

		internal void onKidoZenPassiveAuthenticationFinish(Object sender, EventArgs e ) {
			var isAuthenticated =(e as AuthenticationResponseEventArgs).Success;
			if(isAuthenticated) {
				Window.RootViewController = App.GetMainPage ().CreateViewController ();
			}
		}
	}
}