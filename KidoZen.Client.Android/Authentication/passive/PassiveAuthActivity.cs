
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

using Java.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace KidoZen.Client.Android
{
	public class AuthenticationResponseEventArgs : EventArgs {
		public Boolean Success { get; set;}
		public String ErrorMessage { get; set;}
		public Dictionary<string,string> TokenInfo { get; set;}
	}
	public delegate void AuthenticationResponse(object sender, AuthenticationResponseEventArgs e);


	public class AuthenticationEventManager {
		public static event AuthenticationResponse AuthenticationResponseArrived;

		public static void OnAuthenticationResponseArrived(AuthenticationResponseEventArgs e) 
		{
			if (AuthenticationResponseArrived != null)
				AuthenticationResponseArrived(null, e);
		}
	}

	public class AuthenticationJavaScriptInterface: Java.Lang.Object {
		Context context;

		public AuthenticationJavaScriptInterface (Context context) {
			this.context = context;
		}

		public AuthenticationJavaScriptInterface (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer) {
		}

		[Export ("getTitleCallback")]
		[JavascriptInterface]
		public void getTitleCallback(String jsResult) {
			Console.WriteLine(jsResult);
			var payload = Encoding.UTF8.GetString (Convert.FromBase64String (jsResult.Replace ("Success payload=", String.Empty)));
			var rawToken = JsonConvert.DeserializeObject<Dictionary<string,string>> (payload);
			AuthenticationEventManager.OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs {
				Success = true,
				TokenInfo = rawToken 
			});
			PassiveAuthActivity.getInstance ().Finish ();
		}
	}


	public class AuthenticationWebViewClient : WebViewClient
	{	
		private string GET_TITLE_FN = "javascript:( function () { window.HTMLOUT.getTitleCallback(document.title); } ) ()";

		public AuthenticationWebViewClient ()
		{
		}
		public override void OnReceivedError (WebView view, ClientError errorCode, string description, string failingUrl)
		{
			base.OnReceivedError (view, errorCode, description, failingUrl);
			var error = String.Format ("Code:{0}, Description: {1}", errorCode, description);
			AuthenticationEventManager.OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs { Success = false, ErrorMessage = error });
		}

		public override void OnPageFinished (WebView view, string url)
		{
			base.OnPageFinished (view, url);
			var payload = view.Title;
			if (payload.Contains ("Success payload=")) {
				view.LoadUrl(GET_TITLE_FN);
			} else if (payload.Contains ("Error message=")) {
				AuthenticationEventManager.OnAuthenticationResponseArrived (new AuthenticationResponseEventArgs {
					Success = false,
					ErrorMessage = payload.Replace ("Error message=", String.Empty)
				});
				//TODO: display alert with error
			}
		}
	}
	[Activity (Label = "PassiveAuthActivity")]			
	public class PassiveAuthActivity : Activity
	{
		private static PassiveAuthActivity thisActivity;

		public static PassiveAuthActivity getInstance() {
			return thisActivity;
		}

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
			webView.AddJavascriptInterface(new AuthenticationJavaScriptInterface(this), "HTMLOUT");
			mainLayout.AddView(webView);
			this.SetContentView(mainLayout,new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,ViewGroup.LayoutParams.MatchParent));

			if (thisActivity == null) {
				thisActivity = this;
			} 

		}
	}

}

