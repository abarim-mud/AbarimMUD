using System.IO;
using System;

namespace AbarimMUD.Storage
{
	public class SingleFileStorage<KeyType, ItemType> : GenericBaseStorage<KeyType, ItemType> where ItemType : class
	{
		public string FileName { get; private set; }

		public override string Name => FileName;

		public SingleFileStorage(Func<ItemType, KeyType> keyGetter, string fileName, Func<KeyType, KeyType> keyConverter = null) : base(keyGetter, keyConverter)
		{
			FileName = fileName;
		}

		protected override void InternalLoad()
		{
			ClearCache();

			var path = Path.Combine(BaseFolder, FileName);
			var socials = JsonDeserializeFromFile<ItemType[]>(path);
			foreach (var social in socials)
			{
				AddToCache(social);
			}
		}

		public override void SaveAll()
		{
			EnsureFolder(BaseFolder);

			var path = Path.ChangeExtension(Path.Combine(BaseFolder, FileName), "json");

			JsonSerializeToFile(path, All);
		}

		public override void Remove(ItemType entity)
		{
			base.Remove(entity);

			// Save immediately after removal
			SaveAll();
		}
	}
}