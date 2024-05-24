using AbarimMUD.Data;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal class Areas : CRUD<Area>
	{
		private class RoomExitConverter : JsonConverter<RoomExit>
		{
			public override RoomExit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var id = reader.GetInt32();

				return new RoomExit
				{
					Tag = id
				};
			}

			public override void Write(Utf8JsonWriter writer, RoomExit value, JsonSerializerOptions options)
			{
				writer.WriteNumberValue(value.TargetRoom.Id);
			}
		}

		internal const string SubfolderName = "areas";

		private string Folder => Path.Combine(BaseFolder, SubfolderName);

		public Areas(DataContextSettings context) : base(context)
		{
			Load();
		}

			
		private void Load()
		{
			var areasFolder = Folder;
			if (!Directory.Exists(areasFolder))
			{
				return;
			}

			var files = Directory.GetFiles(areasFolder, "*.json");
			foreach (var path in files)
			{
				Log($"Loading {path}");

				var data = File.ReadAllText(path);
				var options = Utility.CreateDefaultOptions();
				options.Converters.Add(new RoomExitConverter());
				var area = JsonSerializer.Deserialize<Area>(data, options);
				AddToCache(area);
			}
		}

		internal override void SetReferences(DataContext db)
		{
			base.SetReferences(db);

			foreach(var pair in _cache)
			{
				var area  = pair.Value;
				foreach(var room in area.Rooms)
				{
					foreach(var pair2 in room.Exits)
					{
						var exit = pair2.Value;

						exit.Direction = pair2.Key;

						var targetRoomId = (int)exit.Tag;
						exit.TargetRoom = area.Rooms[targetRoomId];

						exit.Tag = null;
					}
				}
			}
		}

		internal override void Save(Area area)
		{
			var areasFolder = Folder;
			if (!Directory.Exists(areasFolder))
			{
				Directory.CreateDirectory(areasFolder);
			}

			var options = Utility.CreateDefaultOptions();
			options.Converters.Add(new RoomExitConverter());
			var data = JsonSerializer.Serialize(area, options);

			var accountPath = Path.Combine(areasFolder, $"{area.Id}.json");
			File.WriteAllText(accountPath, data);
		}
	}
}
