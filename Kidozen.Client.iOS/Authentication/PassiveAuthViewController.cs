using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace Kidozen.Client.iOS
{
	public class PassiveAuthViewController: UIViewController 
	{
		UIWebView webview;
		NSUrl signInEndpoint;

		public PassiveAuthViewController (String endpoint)
		{
			signInEndpoint = new NSUrl(endpoint);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (x,y) => this.DismissViewController(true,null));
			this.webview = new UIWebView (this.View.Frame);
			webview.ShouldStartLoad = (webView, request, navType) => {return true;};
			webview.LoadFinished+= (object sender, EventArgs e) => {
				var payload = webview.EvaluateJavascript("document.title");
				Console.WriteLine(payload);
			};
			this.View.AddSubview (webview);
			webview.LoadRequest (new NSUrlRequest (this.signInEndpoint));
		}
	}
}

