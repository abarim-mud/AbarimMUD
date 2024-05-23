using AbarimMUD.Data;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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
						exit.TargetRoom = area.Rooms[exit.TargetRoomId];
					}
				}
			}
		}

		internal override void Save(Area entity)
		{
			var areasFolder = Folder;
			if (!Directory.Exists(areasFolder))
			{
				Directory.CreateDirectory(areasFolder);
			}

			var options = Utility.CreateDefaultOptions();
			var data = JsonSerializer.Serialize(entity, options);

			var accountPath = Path.Combine(areasFolder, $"{entity.Id}.json");
			File.WriteAllText(accountPath, data);
		}
	}
}
