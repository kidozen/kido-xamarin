using System;
using Xamarin.Forms;
#if __ANDROID__
using Android.Content;
#endif
namespace Todo
{
	public class App
	{		
		#if __ANDROID__
		public static Context AndroidContext { get; set;}
		#endif
		public static Page GetLoginPage (KidoZen.KZApplication.OnEventHandler onAuthFinish)
		{
			database = new TodoItemDatabase();
			return new LoginPage (onAuthFinish);
		}

		public static Page GetMainPage ()
		{
			var mainNav = new NavigationPage (new TodoListPage ());
			return mainNav;
		}

		static TodoItemDatabase database;
		public static TodoItemDatabase Database {
			get { return database; }
		}
	}
}

