using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Diagnostics;

#if __IOS__
using Kidozen.Client.iOS;
#else
using KidoZen.Client.Android;
using Android.Content;
#endif

namespace Todo
{
	public class LoginPage : ContentPage
	{
		public LoginPage (KidoZen.KZApplication.OnEventHandler onAuthFinish)
		{
			Title = "Todo";

			NavigationPage.SetHasNavigationBar (this, true);

			var loginButton = new Button { Text = "LogIn" };
			loginButton.Clicked += (sender, e) => {
				App.Database.Login(onAuthFinish);
			};

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.StartAndExpand,
				Padding = new Thickness(20),
				Children = {
					loginButton
				}
			};
		}

		protected override void OnAppearing ()
		{
			Console.Write ("appearing");
		}

		protected override void OnDisappearing ()
		{
			Console.Write ("dis appearing");
		}

	}
}

