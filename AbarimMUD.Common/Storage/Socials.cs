using AbarimMUD.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	public class Socials : BaseCRUD<Social>
	{
		private const string SocialsFileName = "social.json";

		internal string Folder => BaseFolder;


		internal Socials(DataContextSettings context) : base(context)
		{
			Load();
		}

		private void Load()
		{
			var path = Path.Combine(Folder, SocialsFileName);
			if (!File.Exists(path))
			{
				return;
			}
			Log($"Loading {path}");

			var data = File.ReadAllText(path);
			var helps = JsonSerializer.Deserialize<Dictionary<string, Social>>(data);
			foreach (var h in helps)
			{
				AddToCache(h.Value);
			}
		}

		public void Add(Social social) => AddToCache(social);

		public void Save()
		{
			var path = Path.Combine(Folder, SocialsFileName);

			var options = Utility.CreateDefaultOptions();
			var data = JsonSerializer.Serialize(_cache, options);
			File.WriteAllText(path, data);
		}
	}
}
