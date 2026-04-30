using System.Linq;
using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.Player
{
	public class SocialsCommand : PlayerCommand
	{
		private const int SocialsPerRow = 6;

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var allSocials = from s in Social.Storage orderby s.Name select s.Name;

			var grid = new AsciiGrid();

			var x = 0;
			var y = 0;
			foreach(var social in allSocials)
			{
				grid.SetValue(x, y, social);

				++x;
				if (x >= SocialsPerRow)
				{
					x = 0;
					++y;
				}
			}

			context.Send(grid.ToString());

			return true;
		}
	}
}
