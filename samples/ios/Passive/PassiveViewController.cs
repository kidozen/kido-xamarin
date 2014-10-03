using System;
using System.Drawing;
using System.Net;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using KidoZen;
using Kidozen.Client.iOS;

namespace Passive
{
    public partial class PassiveViewController : UIViewController
    {
        public PassiveViewController(IntPtr handle)
            : base(handle)
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        #region View lifecycle

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
        }

		KZApplication kidozenApp;
		partial void   UIButton5_TouchUpInside (UIButton sender)
		{
			ServicePointManager.ServerCertificateValidationCallback = (x,w,y,z)=> true;
			kidozenApp = new KZApplication("https://loadtests.qa.kidozen.com", "tasks","NuSSOjO4d/4Zmm+lbG3ntlGkmeHCPn8x20cj82O4bIo=");
			//kidozenApp = new KZApplication("https://loadtests.qa.kidozen.com", "testexpiration","zbOIwN3KhH184K3C12hJle7rMKEmNR1jaheAZKAAhNM=");
			kidozenApp.Authenticate();
		}

		partial void UIButton10_TouchUpInside (UIButton sender)
		{
			DataSource ds = kidozenApp.DataSource["GetCityWeather"];
			var result = ds.Query().Result;
			System.Diagnostics.Debug.WriteLine ("ViewResult:" + result.Data);
		}
        #endregion
    }
}