using AbarimMUD.Utils;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class ForgeCommand : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Info.ForgeShop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var grid = new AsciiGrid();
			grid.SetHeader(0, "Item");
			grid.SetHeader(1, "Components");

			var y = 0;
			foreach(var forge in shopKeeper.Info.ForgeShop.Forges)
			{
				grid.SetValue(0, y, forge.Result.ShortDescription);

				var sb = new StringBuilder();

				for(var i = 0; i < forge.Components.Count; i++)
				{
					var cp = forge.Components[i];
					sb.Append(cp.ShortDescription);

					if (cp.Quantity > 1)
					{
						sb.Append($" ({cp.Quantity})");
					}

					if (i < forge.Components.Count - 1)
					{
						sb.Append(", ");
					}
				}

				if (forge.Price > 0)
				{
					if (forge.Components.Count > 0)
					{
						sb.Append(", ");
					}

					sb.Append($"{forge.Price} gold coins");
				}

				grid.SetValue(1, y, sb.ToString());

				++y;
			}

			context.Send("You can forge following items:");
			context.Send(grid.ToString());

			return true;
		}
	}
}
