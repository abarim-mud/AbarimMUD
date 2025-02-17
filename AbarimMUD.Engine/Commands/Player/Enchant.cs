using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Enchant : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Find shopkeeper
			var enchanter = (from cr in context.Room.Mobiles where cr.Info.Flags.Contains(MobileFlags.Enchanter) select cr).FirstOrDefault();
			if (enchanter == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			if (string.IsNullOrEmpty(data))
			{
				var grid = new AsciiGrid();
				grid.SetHeader(0, "Name");
				grid.SetHeader(1, "Stones");
				grid.SetHeader(2, "Price");
				grid.SetHeader(3, "Affects");

				var y = 0;
				foreach (var f in Enchantement.Storage)
				{
					grid.SetValue(0, y, f.Name);
					grid.SetValue(1, y, f.EnchantementStones.ToString());
					grid.SetValue(2, y, f.Price.ToString());

					var affects = string.Join(", ", (from pair in f.Affects select $"+{pair.Value} {pair.Key}"));
					grid.SetValue(3, y, affects);

					++y;
				}

				context.Send("You can do following enchantements:");
				context.Send(grid.ToString());

				return true;
			}

			// Find forge
/*			var creature = context.Creature;
			var inv = creature.Inventory;
			var forge = (from f in forges where f.Result.MatchesKeyword(data) select f).FirstOrDefault();
			if (forge == null)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} I don't know how to forge '{data}'.");
				return false;
			}

			// Check cps
			foreach (var cp in forge.Components)
			{
				var invItem = (from i in inv where ItemInstance.AreEqual(i.Item, cp.Item) select i).FirstOrDefault();

				if (invItem == null)
				{
					Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} You don't have any {cp.Item.ShortDescription}.");
					return false;
				}

				if (invItem.Quantity < cp.Quantity)
				{
					Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} You don't have enough amount of {cp.Item.ShortDescription}.");
					return false;
				}
			}

			// Check gold
			if (creature.Gold < forge.Price)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} You don't have enough gold.");
				return false;
			}

			// Create item
			var item = new ItemInstance(forge.Result);
			inv.AddItem(item, 1);

			foreach (var cp in forge.Components)
			{
				inv.AddItem(cp.Item, -cp.Quantity);
			}
			creature.Gold -= forge.Price;

			if (forge.Components.Count > 0)
			{
				context.Send($"You give {enchanter.ShortDescription} some items.");
			}

			if (forge.Price > 0)
			{
				context.Send($"You give {enchanter.ShortDescription} some coins.");
			}

			context.Send($"{enchanter} forges {forge.Result.ShortDescription} and gives it to you.");
			Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} There you go, {creature.ShortDescription}.");

			var character = creature as Character;
			character?.Save();*/

			return true;
		}
	}
}
