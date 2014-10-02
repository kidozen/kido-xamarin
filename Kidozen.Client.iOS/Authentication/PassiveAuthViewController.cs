using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace Kidozen.Client.iOS
{
	public class AuthenticationResponseEventArgs : EventArgs {
		public Boolean Success { get; set;}
		public String Content { get; set;}
	}
	public delegate void AuthenticationResponse(object sender, AuthenticationResponseEventArgs e);

	public class PassiveAuthViewController: UIViewController 
	{
		UIWebView webview;
		NSUrl signInEndpoint;

		public event AuthenticationResponse AuthenticationResponseArrived;

		protected virtual void OnAuthenticationResponseArrived(AuthenticationResponseEventArgs e) 
		{
			if (AuthenticationResponseArrived != null)
				AuthenticationResponseArrived(this, e);
		}
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
				if (payload.Contains ("Success payload="))
					OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs { Success = true, Content = payload.Replace("Success payload=", String.Empty) });
				else if (payload.Contains ("Error message="))
					OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs { Success = true, Content = payload.Replace("Error message=", String.Empty) });

			};
			this.View.AddSubview (webview);
			webview.LoadRequest (new NSUrlRequest (this.signInEndpoint));
		}
	}
}

