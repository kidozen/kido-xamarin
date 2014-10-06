using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Newtonsoft.Json.Linq;

using KidoZen;

#if __IOS__
using Kidozen.Client.iOS;
#else
using KidoZen.Client.Android;
using Android.Content;
#endif

namespace Todo
{
	public class TodoItemDatabase 
	{
		static object locker = new object ();
		KZApplication kidozenApplication;
		Storage database;
		DataSource queryDataSource, saveDataSource;

		public TodoItemDatabase()
		{
			this.kidozenApplication = new KZApplication (Settings.Marketplace, Settings.Application, Settings.Key);
		}

		public void Login(KZApplication.OnEventHandler onAuthFinish) {
			#if __ANDROID__
			this.kidozenApplication.Authenticate (App.AndroidContext, onAuthFinish);
			#else
			this.kidozenApplication.Authenticate (onAuthFinish);
			#endif
			database = kidozenApplication.Storage["todo"];
			queryDataSource = kidozenApplication.DataSource["QueryTodo"];
			saveDataSource = kidozenApplication.DataSource["AddTodo"];

		}

		public IEnumerable<TodoItem> GetItemsNotDone ()
		{
			lock (locker) {
				return  database.Query<TodoItem>(@"{""Done"":false}").Result.Data;
			}
		}

		public void DeleteItem(string id)
		{
			lock (locker) {
				var deleted = database.Delete (id).Result;
			}
		}

		public IEnumerable<TodoItem> GetItems ()
		{
			lock (locker) {
				return  database.All<TodoItem>().Result.Data;
			}
		}

		public void SaveItem (TodoItem item) 
		{
			lock (locker) {
				database.Save<TodoItem>(item); //upsert
			}
		}

		// ******************************
		// *** DataSource sample code ***
		// ******************************
		/*
		public IEnumerable<TodoItem> GetItems ()
		{
			lock (locker) {
				var results = queryDataSource.Query().Result.Data;
				return createTodoItemList (results);
			}
		}

		//Ensure that your DataSource can execute an UPSERT
		public void SaveItem (TodoItem item) 
		{
			lock (locker) {
				var result = saveDataSource.Invoke(item).Result;
			}
		}

		IEnumerable<TodoItem> createTodoItemList (JObject results)
		{
			var result = JArray.Parse (results.SelectToken("data").ToString());
			return result.Select ( todo => new TodoItem {
				Name = todo.Value<string>("Name"),
				Notes = todo.Value<string>("Notes") ,
				_id = todo.Value<string>("_id") ,
			}
			).ToList();
		}
		*/
	}
}

