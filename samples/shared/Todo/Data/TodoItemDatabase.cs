using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using KidoZen;
#if __IOS__
using Kidozen.Client.iOS;
#else
using Kidozen.Client.Android;
#endif

namespace Todo
{
	public class TodoItemDatabase 
	{
		static object locker = new object ();
		KZApplication kidozenApplication;

		Storage database;

		string Marketplace = "https://loadtests.qa.kidozen.com";
		string Application = "tasks";
		string Key = "NuSSOjO4d/4Zmm+lbG3ntlGkmeHCPn8x20cj82O4bIo=";

		public TodoItemDatabase()
		{
			this.kidozenApplication = new KZApplication (Marketplace, Application, Key);
		}

		public void Login(KZApplication.OnEventHandler onAuthFinish) {
			this.kidozenApplication.Authenticate (onAuthFinish);
			database = kidozenApplication.Storage["todo"];
		}

		public IEnumerable<TodoItem> GetItems ()
		{
			lock (locker) {
				return  database.All<TodoItem>().Result.Data;
			}
		}

		public IEnumerable<TodoItem> GetItemsNotDone ()
		{
			lock (locker) {
				return  database.All<TodoItem>().Result.Data;
			}
		}

		public void SaveItem (TodoItem item) 
		{
			lock (locker) {
				database.Save (item); //upsert
			}
		}

		public void DeleteItem(string id)
		{
			lock (locker) {
				database.Delete(id);
			}
		}
	}
}

