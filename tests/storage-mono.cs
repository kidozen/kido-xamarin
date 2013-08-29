using System;
using System.Net;
using NUnit.Framework;
using KidoZen;
using TestConstants;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace MonoTouchTests
{
	[TestFixture]
	public class StorageTest
    {
        static KZApplication app;
        static Storage objectSet;

		private async Task InitApplication(){
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				await app.Initialize();
				await app.Authenticate (Constants.user, Constants.pass, Constants.provider);
			}
			await InitStorage ();
		}

		private async Task InitStorage()
		{
			if (objectSet != null) {
				return;
			} 

			objectSet = app.Storage ["test-monodroid"];

			try {
				await objectSet.Drop();
			} catch (Exception) {
			}

			var insResult = await objectSet.Insert(new { x = 1 });
			insResult = await objectSet.Insert(new { x = 2 });
			insResult = await objectSet.Insert(new { x = 3 });
			insResult = await objectSet.Insert(new { x = 4 });
			Assert.AreEqual(HttpStatusCode.Created, insResult.StatusCode);
		}

		/*
		[TestFixtureSetUp] 
		public  void TextfixtureInit()
        {
			app = new KZApplication(Constants.marketplace, Constants.application);
			app.Initialize();
			var user =  app.Authenticate (Constants.user, Constants.pass, Constants.provider).Result;
			objectSet = app.Storage["test-monodroid"];
        }
		*/
		[Test]
		public void SyncInsert()
		{
			InitApplication ().RunSynchronously();
			var rnd = Guid.NewGuid ().ToString ();
			var result = objectSet.Insert(new { Foo = rnd }).Result;

			Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
			Assert.IsNotNull(result.Data);
			Assert.IsFalse(string.IsNullOrWhiteSpace(result.Data._id));
			Assert.IsNotNull(result.Data._metadata);
		}

		[Test]
		public async void Insert()
		{
			await InitApplication ();
			var rnd = Guid.NewGuid ().ToString ();
			var result = await objectSet.Insert(new { Foo = rnd });

			Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
			Assert.IsNotNull(result.Data);
			Assert.IsFalse(string.IsNullOrWhiteSpace(result.Data._id));
			Assert.IsNotNull(result.Data._metadata);
		}


		[Test]
        public async void GetById()
        {
			await InitApplication ();
			var rnd = Guid.NewGuid ().ToString ();
			var insResult = await objectSet.Insert(new { Foo = rnd });
			Assert.AreEqual(HttpStatusCode.Created, insResult.StatusCode);

            var getResult = await objectSet.Get(insResult.Data._id);

            Assert.IsNotNull(getResult.Data);
            Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode);
            Assert.AreEqual(rnd, getResult.Data.Value<string>("Foo"));
        }

		[Test]
        public async void Update()
        {
			await InitApplication ();
			var rnd = Guid.NewGuid ().ToString ();
			var newrnd = Guid.NewGuid ().ToString ();
            var insResult = await objectSet.Insert(new { Foo = rnd });
			var updResult = await objectSet.Update(insResult.Data._id, new { Foo = newrnd, _metadata = insResult.Data._metadata });
            var getResult = await objectSet.Get(insResult.Data._id);
			Assert.AreEqual(HttpStatusCode.OK, updResult.StatusCode);
			Assert.IsNotNull(getResult.Data);
			Assert.AreEqual(newrnd, getResult.Data.Value<string>("Foo"));
        }

        public async void Drop()
        {
			await InitApplication ();
			var rnd = Guid.NewGuid ().ToString ();
            var insResult = await objectSet.Insert(new { Foo = rnd });
			Assert.AreEqual(HttpStatusCode.Created, insResult.StatusCode);

            var dropResult = await objectSet.Drop();
            Assert.AreEqual(HttpStatusCode.OK, dropResult.StatusCode);
        }

		[Test]
        public async void Delete()
        {
			await InitApplication ();
			var rnd = Guid.NewGuid ().ToString ();
            var insResult = await objectSet.Insert(new { Foo = rnd });
			Assert.AreEqual(HttpStatusCode.Created, insResult.StatusCode);

            var delResult = await objectSet.Delete(insResult.Data._id);
            Assert.AreEqual(HttpStatusCode.OK, delResult.StatusCode);
        }

		[Test]
        public async void QueryDoesFindObjects()
        {
			await InitApplication ();

            var result = await objectSet.Query(@"{""x"":{""$gt"":1}}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(result.Data.Count>0);
            
        }

		[Test]
        public async void QueryDoesNotFindObjects()
        {
			await InitApplication ();

            var result = await objectSet.Query(@"{""x"":{""$gt"":4}}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(0, result.Data.Count);
            
        }

		[Test]
        public async void QuerySkipAndLimit()
        {
			await InitApplication ();

            var result = await objectSet.Query("{}", @"{""$skip"":1,""$limit"":2}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(2, result.Data.Count);
            Assert.AreEqual(2, result.Data[0].Value<int>("x"));
            Assert.AreEqual(3, result.Data[1].Value<int>("x"));
            
        }

		[Test]
        public async void QueryCount()
        {
			await InitApplication ();

            var result = await objectSet.Query(@"{""x"":{""$gt"":1}}", @"{""$count"":true}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, result.Data.Count);         
        }

		[Test]
        public async void QuerySelect()
        {
			await InitApplication ();
			try {
				var dropResult = await objectSet.Drop();
			} catch (Exception ex) {
			}

			var insResult = await objectSet.Insert(new { x = 7, y = 8, z = 9 });
			Assert.AreEqual(HttpStatusCode.Created, insResult.StatusCode);

			var result = await objectSet.Query(fields: @"{""y"": true, ""_id"": false}");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var obj = (JObject)result.Data[0];
			//TODO check the count property
            //Assert.AreEqual(2, obj.Properties().Count()); // _id and y
            Assert.AreEqual(8, obj.Value<int>("y"));
            
        }

		[Test]
        public async void All()
        {
			await InitApplication ();

            var result = await objectSet.All();
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

        }


    }
}
