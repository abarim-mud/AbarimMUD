using System;
using System.Linq;
using AbarimMUD.Data;

namespace AbarimMUD.Commands
{
	public static class ExecutionContextUtils
	{
		public static bool ContainsPattern(this string[] keywords, string[] pattern)
		{
			foreach (var s in pattern)
			{
				var found = (from k in keywords where k.StartsWith(s, StringComparison.OrdinalIgnoreCase) select k).Any();

				if (!found)
				{
					return false;
				}
			}

			return true;
		}

		public static ExecutionContext Find(this Room room, string search)
		{
			// Split into parts
			var pattern = (from s in search.Split(' ') select s.Trim()).ToArray();

			// Search among characters
			foreach (var c in room.Characters)
			{
				var context = (PlayerExecutionContext) c.Tag;
				if (context.Keywords.ContainsPattern(pattern))
				{
					return context;
				}
			}

			// Search among mobiles
			foreach (var m in room.Mobiles)
			{
				var context = new MobileExecutionContext(m);
				if (context.Keywords.ContainsPattern(pattern))
				{
					return context;
				}
			}

			return null;
		}
	}
}