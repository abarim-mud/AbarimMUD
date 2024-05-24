using AbarimMUD.Data;
using System.IO;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Areas : CRUD<Area>
	{

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
				var area = JsonSerializer.Deserialize<Area>(data);
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
						exit.TargetRoom = area.Rooms[exit.JsonData.TargetRoomId];

						exit.JsonData = null;
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

			// Set json data
			foreach(var room in area.Rooms)
			{
				foreach(var pair in room.Exits)
				{
					var roomExit = pair.Value;

					roomExit.JsonData = new RoomReference
					{
						TargetRoomId = roomExit.TargetRoom.Id
					};
				}
			}

			var options = Utility.CreateDefaultOptions();
			var data = JsonSerializer.Serialize(area, options);

			var accountPath = Path.Combine(areasFolder, $"{area.Id}.json");
			File.WriteAllText(accountPath, data);

			// Clear json data
			foreach (var room in area.Rooms)
			{
				foreach (var pair in room.Exits)
				{
					var roomExit = pair.Value;
					roomExit.JsonData = null;
				}
			}
		}
	}
}
