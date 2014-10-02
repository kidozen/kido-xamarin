
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;

namespace KidoZen.Client.Android
{
	public class AuthenticationResponseEventArgs : EventArgs {
		public Boolean Success { get; set;}
		public String Content { get; set;}
	}
	public delegate void AuthenticationResponse(object sender, AuthenticationResponseEventArgs e);

	[Activity (Label = "PassiveAuthActivity")]			
	public class PassiveAuthActivity : Activity
	{
		//public event AuthenticationResponse AuthenticationResponseArrived;
		public WebView webView;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.RequestWindowFeature (WindowFeatures.NoTitle);

			// Create your application here
			var endpoint = this.Intent.GetStringExtra ("signInUrl");

			var mainLayout = new LinearLayout (this);
			mainLayout.SetPadding (0, 0, 0, 0);

			var webClient = new AuthenticationWebViewClient ();
			
			webView = new WebView(this);
			webView.VerticalScrollBarEnabled=false;
			webView.HorizontalScrollBarEnabled=false;
			webView.SetWebViewClient (webClient);
			webView.Settings.JavaScriptEnabled=true;
			webView.LayoutParameters= new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,ViewGroup.LayoutParams.MatchParent);
			webView.Settings.SavePassword=false;
			webView.LoadUrl (endpoint);

			mainLayout.AddView(webView);
			this.SetContentView(mainLayout,new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,ViewGroup.LayoutParams.MatchParent));

		}
	}

	public class AuthenticationWebViewClient : WebViewClient
	{
		public event AuthenticationResponse AuthenticationResponseArrived;

		protected virtual void OnAuthenticationResponseArrived(AuthenticationResponseEventArgs e) 
		{
			if (AuthenticationResponseArrived != null)
				AuthenticationResponseArrived(this, e);
		}

		public AuthenticationWebViewClient ()
		{
		}
		public override void OnReceivedError (WebView view, ClientError errorCode, string description, string failingUrl)
		{
			base.OnReceivedError (view, errorCode, description, failingUrl);
		}
		public override void OnPageFinished (WebView view, string url)
		{
			base.OnPageFinished (view, url);
			Console.WriteLine (view.Title);
			if (view.Title.Contains ("Success payload="))
				OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs { Success = true, Content = view.Title.Replace("Success payload=", String.Empty) });
			else if (view.Title.Contains ("Error message="))
				OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs { Success = true, Content = view.Title.Replace("Error message=", String.Empty) });
		}
	}
}

