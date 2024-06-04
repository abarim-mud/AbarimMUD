using AbarimMUD.Data;
using AbarimMUD.Storage;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;

namespace AbarimMUD.Common.Tests
{
	public class DataContextTests
	{
		private static void ResetData()
		{
			if (Directory.Exists("Data"))
			{
				Directory.Delete("Data", true);
			}

			DataContext.Initialize("Data", s => Trace.WriteLine(s));
		}

		[Test]
		public void TestAccounts()
		{
			ResetData();

			DataContext.Register(Account.Storage);
			DataContext.Register(Character.Storage);
			DataContext.Load();

			var newAcc = new Account
			{
				Name = "test",
				PasswordHash = "1234"
			};
			newAcc.Create();

			var newChar = new Character
			{
				Account = newAcc,
				Name = "char1"
			};
			newChar.Create();

			var newChar2 = new Character
			{
				Account = newAcc,
				Name = "char2"
			};
			newChar2.Create();

			var accGet = Account.GetAccountByName(newAcc.Name);
			Assert.That(accGet, Is.Not.Null);
			Assert.That(accGet.Name, Is.EqualTo("test"));
			Assert.That(accGet.PasswordHash, Is.EqualTo("1234"));

			var charsGet = Character.GetCharactersByAccountName(newAcc.Name);
			Assert.That(charsGet, Is.Not.Null);
			Assert.That(charsGet.Length, Is.EqualTo(2));
			Assert.That(charsGet[0].Name, Is.EqualTo("char1"));
			Assert.That(charsGet[1].Name, Is.EqualTo("char2"));

			DataContext.Load();

			accGet = Account.GetAccountByName(newAcc.Name);
			Assert.That(accGet, Is.Not.Null);
			Assert.That(accGet.Name, Is.EqualTo("test"));
			Assert.That(accGet.PasswordHash, Is.EqualTo("1234"));

			charsGet = Character.GetCharactersByAccountName(newAcc.Name);
			Assert.That(charsGet, Is.Not.Null);
			Assert.That(charsGet.Length, Is.EqualTo(2));
			Assert.That(charsGet[0].Name, Is.EqualTo("char1"));
			Assert.That(charsGet[1].Name, Is.EqualTo("char2"));

			DataContext.Unregister(Account.Storage);
			DataContext.Unregister(Character.Storage);
		}

		[Test]
		public void TestAreas()
		{
			ResetData();

			DataContext.Register(Area.Storage);
			DataContext.Load();

			var area = new Area
			{
				Name = "test",
				Filename = "test.json"
			};

			var room = new Room
			{
				Id = Area.NextRoomId,
				Name = "Test Room"
			};
			area.Rooms.Add(room);

			var room2 = new Room
			{
				Id = Area.NextRoomId,
				Name = "Test Room2"
			};
			area.Rooms.Add(room2);

			room.Exits[Direction.Up] = new RoomExit
			{
				TargetRoom = room2
			};

			room2.Exits[Direction.Down] = new RoomExit
			{
				TargetRoom = room
			};

			var mobile = new Mobile
			{
				Id = Area.NextMobileId,
				Name = "Test Mobile"
			};
			area.Mobiles.Add(mobile);
			area.Create();

			// Reload
			DataContext.Load();

			area = Area.GetAreaByName("test");
			Assert.That(area, Is.Not.Null);
			Assert.That(area.Name, Is.EqualTo("test"));
			Assert.That(area.Rooms.Count, Is.EqualTo(2));
			Assert.That(area.Rooms[0].Name, Is.EqualTo("Test Room"));
			Assert.That(area.Rooms[1].Name, Is.EqualTo("Test Room2"));
			Assert.That(area.Rooms[0].Exits.Count, Is.EqualTo(1));
			Assert.That(area.Rooms[0].Exits.ContainsKey(Direction.Up), Is.True);
			Assert.That(area.Rooms[0].Exits[Direction.Up].TargetRoom, Is.EqualTo(area.Rooms[1]));
			Assert.That(area.Rooms[1].Exits.Count, Is.EqualTo(1));
			Assert.That(area.Rooms[1].Exits.ContainsKey(Direction.Down), Is.True);
			Assert.That(area.Rooms[1].Exits[Direction.Down].TargetRoom, Is.EqualTo(area.Rooms[0]));
			Assert.That(area.Mobiles.Count, Is.EqualTo(1));
			Assert.That(area.Mobiles[0].Name, Is.EqualTo("Test Mobile"));

			DataContext.Unregister(Area.Storage);
		}
	}
}