using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;

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
				foreach (var f in Enchantment.Storage)
				{
					grid.SetValue(0, y, f.Name);
					grid.SetValue(1, y, f.EnchantmentStones.ToString());
					grid.SetValue(2, y, f.Price.ToString());

					var affects = string.Join(", ", (from pair in f.Affects select $"+{pair.Value} {pair.Key}"));
					grid.SetValue(3, y, affects);

					++y;
				}

				context.Send("You can do following enchantments:");
				context.Send(grid.ToString());

				return true;
			}

			var args = data.SplitByWhitespace();
			if (args.Length < 2)
			{
				context.Send("Usage: enchant <item_name> <enchantment_name>");
				return true;
			}

			// Find item
			var creature = context.Creature;
			var inv = creature.Inventory;
			var invItem = inv.FindItem(args[0]);
			if (invItem == null)
			{
				context.Send($"You don't seem to have '{args[0]}'");
				return false;
			}

			if (invItem.Item.Enchantment != null)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} {invItem.Name} holds an enchantment already.");
				return false;
			}

			if (invItem.Info.EnchantmentTier == null)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} {invItem.Name} can't be enchanted.");
				return false;
			}

			// Find enchantment
			var enchantment = (from f in Enchantment.Storage where f.MatchesKeyword(args[1]) select f).FirstOrDefault();
			if (enchantment == null)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} I don't know the enchantment '{args[1]}'.");
				return false;
			}

			var enchantmentType = invItem.Info.EnchantmentTier.Value.ToEnchantmentItemType();

			// Check stones
			var invStones = (from i in inv where i.Info.ItemType == enchantmentType select i).FirstOrDefault();
			var stoneInfo = (from i in Item.Storage where i.ItemType == enchantmentType select i).First();
			if (invStones == null || invStones.Quantity < enchantment.EnchantmentStones)
			{
				var q = invStones != null ? invStones.Quantity : 0;
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} You don't have enough enchantment stones. You have {q} of {enchantment.EnchantmentStones} '{stoneInfo.ShortDescription}'");
				return false;
			}

			// Check gold
			if (creature.Gold < enchantment.Price)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} You don't have enough gold.");
				return false;
			}

			// Do the enchantment
			invItem.Item.Enchantment = enchantment;

			// Deduct the stones
			inv.AddItem(new ItemInstance(stoneInfo), -enchantment.EnchantmentStones);

			// Deduct the coins
			creature.Gold -= enchantment.Price;

			context.Send($"You give {enchanter.ShortDescription} some enchantment stones.");

			if (enchantment.Price > 0)
			{
				context.Send($"You give {enchanter.ShortDescription} some coins.");
			}

			context.Send($"{enchanter} takes {invItem.JustName}, makes some gestures over it.\nThe item grows brightly.\nThe enchantment stones crumble to dust.\n{enchanter.ShortDescription} gives you {invItem.Name}.");
			Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} There you go, {creature.ShortDescription}.");

			var character = creature as Character;
			character?.Save();

			return true;
		}
	}
}
