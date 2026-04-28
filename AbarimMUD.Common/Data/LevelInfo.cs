using System.Text.Json.Serialization;
using Ur;

namespace AbarimMUD.Data
{
	public class LevelInfo: IHasId<int>
	{
		public static readonly SingleFileStorage<int, LevelInfo> Storage = new SingleFileStorage<int, LevelInfo>("levels.json");

		[JsonIgnore]
		public int Id
		{
			get => Level;
			set => Level = value;
		}

		public int Level { get; set; }

		public long Experience { get; set; }

		public LevelInfo()
		{
		}

		public LevelInfo(int level, long experience)
		{
			Level = level;
			Experience = experience;
		}

		public static LevelInfo GetLevelInfo(int level) => Storage.EnsureByKey(level);
	}
}