using AbarimMUD.Storage;

namespace AbarimMUD.Data
{
	public class LevelInfo
	{
		public static readonly SingleFileStorage<int, LevelInfo> Storage = new SingleFileStorage<int, LevelInfo>(l => l.Level, "levels.json");

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