using AbarimMUD.Data;
using AbarimMUD.Storage;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace AbarimMUD.Common.Tests
{
	public class DataContextTests
	{
		[SetUp]
		public void Setup()
		{
		}

		private static DataContext CreateDatabase(bool delete)
		{
			if (delete)
			{
				Directory.Delete("Data", true);
			}

			return new DataContext("Data", s => Trace.WriteLine(s));
		}

		[Test]
		public void TestAccounts()
		{
			var db = CreateDatabase(true);
			var newAcc = new Account
			{
				Name = "test",
				PasswordHash = "1234"
			};
			db.Accounts.Update(newAcc);

			var newChar = new Character
			{
				AccountName = newAcc.Id,
				Name = "char1"
			};
			db.Characters.Update(newChar);

			var newChar2 = new Character
			{
				AccountName = newAcc.Id,
				Name = "char2"
			};
			db.Characters.Update(newChar2);

			var accGet = db.Accounts.GetById(newAcc.Name);
			Assert.That(accGet, Is.Not.Null);
			Assert.That(accGet.Name, Is.EqualTo("test"));
			Assert.That(accGet.PasswordHash, Is.EqualTo("1234"));

			var charsGet = db.Characters.GetByAccountName("test");
			Assert.That(charsGet, Is.Not.Null);
			Assert.That(charsGet.Length, Is.EqualTo(2));
			Assert.That(charsGet[0].Name, Is.EqualTo("char1"));
			Assert.That(charsGet[1].Name, Is.EqualTo("char2"));

			var db2 = CreateDatabase(false);
			accGet = db2.Accounts.GetById(newAcc.Name);
			Assert.That(accGet, Is.Not.Null);
			Assert.That(accGet.Name, Is.EqualTo("test"));
			Assert.That(accGet.PasswordHash, Is.EqualTo("1234"));

			charsGet = db2.Characters.GetByAccountName("test");
			Assert.That(charsGet, Is.Not.Null);
			Assert.That(charsGet.Length, Is.EqualTo(2));
			Assert.That(charsGet[0].Name, Is.EqualTo("char1"));
			Assert.That(charsGet[1].Name, Is.EqualTo("char2"));
		}

		[Test]
		public void TestAreas()
		{
			var db = CreateDatabase(true);
			var area = new Area
			{
				Name = "test",
			};

			var room = new Room
			{
				Name = "Test Room"
			};

			area.Rooms.Add(room);
			db.Areas.Update(area);

			var db2 = CreateDatabase(false);
			area = db2.Areas.GetById("test");
			Assert.That(area, Is.Not.Null);
			Assert.That(area.Name, Is.EqualTo("test"));
			Assert.That(area.Rooms.Count, Is.EqualTo(1));
			Assert.That(area.Rooms[0].Name, Is.EqualTo("Test Room"));
		}
	}
}