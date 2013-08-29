using System;
using System.Net;
using NUnit.Framework;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using KidoZen;

namespace KidoZen.Core.Tests
{
	[TestFixture()]
	public class StorageTests
	{
		static KZApplication app;
		static Storage objectSet;

		[SetUp]
		public void SetUp ()
		{
			Console.WriteLine ("Setting up");
			if (app==null) {
				app = new KZApplication(Constants.marketplace, Constants.application);
				app.Initialize().Wait();
				app.Authenticate(Constants.user, Constants.pass, Constants.provider).Wait();
			}
			if (objectSet == null) {
				objectSet = app.Storage["test-monodroid"];
			}

			//insert just in case the object set doesn't exist
			objectSet.Insert (new { Foo = "foo" }).Wait ();
			//drop the object set.
			var dropResult = objectSet.Drop().Result;
			Assert.AreEqual(HttpStatusCode.OK, dropResult.StatusCode);
		}

		[Test]
		public void Insert()
		{
			var result = objectSet.Insert(new { Foo = "foo" }).Result;
			Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
			Assert.IsNotNull(result.Data);
			Assert.IsFalse(string.IsNullOrWhiteSpace(result.Data._id));
			Assert.IsNotNull(result.Data._metadata);
		}


		[Test]
		public void GetById()
		{
			var insResult = objectSet.Insert(new { Foo = "foo" }).Result;
			var getResult = objectSet.Get(insResult.Data._id).Result;
			Assert.IsNotNull(getResult.Data);
			Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode);
			Assert.AreEqual("foo", getResult.Data.Value<string>("Foo"));
		}

		[Test]
		public void Update()
		{
			//arrange
			var insResult = objectSet.Insert(new { Foo = "foo" }).Result;
			objectSet.Update(insResult.Data._id, new { Foo = "bar", _metadata = insResult.Data._metadata }).Wait();
			//act
			var getResult = objectSet.Get(insResult.Data._id).Result;
			//assert
			Assert.IsNotNull(getResult.Data);
			Assert.AreEqual("bar", getResult.Data.Value<string>("Foo"));
		}

		[Test]
		public void Drop()
		{
			//arrange
			objectSet.Insert(new { Foo = "foo" }).Wait();
			//act
			var dropResult = objectSet.Drop().Result;
			//assert
			Assert.AreEqual(HttpStatusCode.OK, dropResult.StatusCode);
		}

		[Test]
		public void Delete()
		{
			var insResult = objectSet.Insert(new { Foo = "foo" }).Result;
			var delResult = objectSet.Delete(insResult.Data._id).Result;
			Assert.AreEqual(HttpStatusCode.OK, delResult.StatusCode);
		}

		[Test]
		public void QueryDoesFindObjects()
		{
			//arrange
			objectSet.Insert(new { x = 1 }).Wait();
			objectSet.Insert(new { x = 2 }).Wait();
			objectSet.Insert(new { x = 3 }).Wait();
			//act
			var result = objectSet.Query(@"{""x"":{""$gt"":1}}").Result;
			//assert
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual(2, result.Data.Count);
		}

		[Test]
		public void QueryDoesNotFindObjects()
		{
			//arrange
			objectSet.Insert(new { x = 1 }).Wait();
			objectSet.Insert(new { x = 2 }).Wait();
			objectSet.Insert(new { x = 3 }).Wait();
			//act
			var result = objectSet.Query(@"{""x"":{""$gt"":3}}").Result;
			//assert
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual(0, result.Data.Count);
		}

		[Test]
		public void QuerySkipAndLimit()
		{
			//arrange
			objectSet.Insert(new { x = 1 }).Wait();
			objectSet.Insert(new { x = 2 }).Wait();
			objectSet.Insert(new { x = 3 }).Wait();
			objectSet.Insert(new { x = 4 }).Wait();
			//act
			var result = objectSet.Query("{}", @"{""$skip"":1,""$limit"":2}").Result;
			//assert
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual(2, result.Data.Count);
			Assert.AreEqual(2, result.Data[0].Value<int>("x"));
			Assert.AreEqual(3, result.Data[1].Value<int>("x"));
		}

		[Test]
		public void QueryCount()
		{
			//arrange
			objectSet.Insert(new { x = 1 }).Wait();
			objectSet.Insert(new { x = 2 }).Wait();
			objectSet.Insert(new { x = 3 }).Wait();
			//act
			var result = objectSet.Query(@"{""x"":{""$gt"":1}}", @"{""$count"":true}").Result;
			//assert
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual(1, result.Data.Count);
			Assert.AreEqual(2, result.Data[0].Value<int>());
		}

		[Test]
		public void QuerySelect()
		{
			//arrange
			objectSet.Insert(new { x = 1, y = 2, z = 3 }).Wait();
			//act
			var result = objectSet.Query(fields: @"{""y"":1}").Result;
			//assert
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual(1, result.Data.Count);
			var obj = (JObject)result.Data[0];
			//TODO check the count property
			//Assert.AreEqual(2, obj.Properties().Count()); // _id and y
			Assert.AreEqual(2, obj.Value<int>("y"));
		}

		[Test]
		public void All()
		{
			//arrange
			objectSet.Insert(new { x = 1 }).Wait();
			objectSet.Insert(new { x = 2 }).Wait();
			objectSet.Insert(new { x = 3 }).Wait();
			//act
			var result = objectSet.All().Result;
			//assert
			Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
			Assert.AreEqual(3, result.Data.Count);
		}
	}
}

