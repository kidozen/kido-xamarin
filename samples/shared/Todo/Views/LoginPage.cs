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
#endif

namespace Todo
{
	public class LoginPage : ContentPage
	{
		public LoginPage ()
		{
			Title = "Todo";

			NavigationPage.SetHasNavigationBar (this, true);

			var loginButton = new Button { Text = "LogIn" };
			loginButton.Clicked += (sender, e) => {
				App.Database.Login(onKidoZenPassiveAuthenticationFinish);
			};

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.StartAndExpand,
				Padding = new Thickness(20),
				Children = {
					loginButton
				}
			};
		}

		internal void onKidoZenPassiveAuthenticationFinish(Object sender, EventArgs e ) {
			var isAuthenticated =(e as AuthenticationResponseEventArgs).Success;
			if(isAuthenticated) {
				var todoListPage = new TodoListPage();
				this.Navigation.PushAsync(todoListPage);
			}
		}
	}
}

