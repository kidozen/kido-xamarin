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


		partial void   UIButton5_TouchUpInside (UIButton sender)
		{
			var nn = UIApplication.SharedApplication.Delegate.Window.RootViewController;
			ServicePointManager.ServerCertificateValidationCallback = (x,w,y,z)=> true;

			// Perform any additional setup after loading the view, typically from a nib.
			var app = new KZApplication("https://loadtests.qa.kidozen.com", "tasks",false);
			app.Authenticate();
			/*
			app.Authenticate().ContinueWith(r=> {
				System.Diagnostics.Debug.WriteLine ("result:" + r.Result);
				var ds = app.DataSource("GetCityWeather");
				var result = ds.Query().Result;
				System.Diagnostics.Debug.WriteLine ("ViewResult:" + result);

			});
			*/
			//System.Diagnostics.Debug.WriteLine (app.Login().Result);
		}
        #endregion
    }
}