
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

	[Activity (Label = "PassiveAuthActivity")]			
	public class PassiveAuthActivity : Activity
	{
		public class AuthenticationWebViewClient : WebViewClient
		{
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
			}
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.RequestWindowFeature (WindowFeatures.NoTitle);

			// Create your application here
			var endpoint = this.Intent.GetStringExtra ("signInUrl");

			var mainLayout = new LinearLayout (this);
			mainLayout.SetPadding (0, 0, 0, 0);

			var webClient = new AuthenticationWebViewClient ();
			
			var webView = new WebView(this);
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
}

