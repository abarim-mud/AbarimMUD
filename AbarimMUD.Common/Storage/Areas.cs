using AbarimMUD.Data;
using System.IO;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Areas : CRUD<Area>
	{
		internal const string SubfolderName = "areas";

		private string Folder => Path.Combine(BaseFolder, SubfolderName);

		public Areas(DataContext context) : base(context)
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

		internal override void Save(Area entity)
		{
			var areasFolder = Folder;
			if (!Directory.Exists(areasFolder))
			{
				Directory.CreateDirectory(areasFolder);
			}

			var options = new JsonSerializerOptions { WriteIndented = true };
			var data = JsonSerializer.Serialize(entity, options);

			var accountPath = Path.Combine(areasFolder, $"{entity.Id}.json");
			File.WriteAllText(accountPath, data);
		}
	}
}
