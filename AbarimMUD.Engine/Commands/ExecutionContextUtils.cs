using System;
using System.Linq;
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
				var context = (PlayerExecutionContext) c.Tag;
				if (context.MatchesKeyword(keyword))
				{
					return context;
				}
			}

			// Search among mobiles
			foreach (var m in room.Mobiles)
			{
				var context = new MobileExecutionContext(m);
				if (context.MatchesKeyword(keyword))
				{
					return context;
				}
			}

			return null;
		}
	}
}