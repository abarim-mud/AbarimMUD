using AbarimMUD.Data;

namespace AbarimMUD.Storage
{
	public class LevelInfos : SingleFileStorage<int, LevelInfo>
	{
		public LevelInfos(): base(l => l.Level, "levels.json")
		{
		}
	}
}
