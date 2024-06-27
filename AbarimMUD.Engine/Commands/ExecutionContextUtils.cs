using AbarimMUD.Data;

namespace AbarimMUD.Commands
{
	public static class ExecutionContextUtils
	{
		public static ExecutionContext Find(this Room room, string keyword)
		{
			// Search among characters
			foreach (var c in room.Characters)
			{
				if (c.MatchesKeyword(keyword))
				{
					return (ExecutionContext)c.Tag;
				}
			}

			// Search among mobiles
			foreach (var m in room.Mobiles)
			{
				if (m.MatchesKeyword(keyword))
				{
					return (ExecutionContext)m.Tag;
				}
			}

			return null;
		}
	}
}