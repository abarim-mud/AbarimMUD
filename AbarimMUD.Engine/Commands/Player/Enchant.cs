﻿using AbarimMUD.Data;
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

			var args = data.SplitByWhitespace();
			if (args.Length < 2)
			{
				context.Send("Usage: enchant <item_name> <enchantement_name>");
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

			if (invItem.Item.Enchantement != null)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} {invItem.Name} holds an enchantement already.");
				return false;
			}

			if (invItem.Info.EnchantementTier == null)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} {invItem.Name} can't be enchanted.");
				return false;
			}

			// Find enchantement
			var enchantement = (from f in Enchantement.Storage where f.MatchesKeyword(args[1]) select f).FirstOrDefault();
			if (enchantement == null)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} I don't know enchantement '{args[1]}'.");
				return false;
			}

			var enchantementType = invItem.Info.EnchantementTier.Value.ToEnchantementItemType();

			// Check stones
			var invStones = (from i in inv where i.Info.ItemType == enchantementType select i).FirstOrDefault();
			var stoneInfo = (from i in Item.Storage where i.ItemType == enchantementType select i).First();
			if (invStones == null || invStones.Quantity < enchantement.EnchantementStones)
			{
				var q = invStones != null ? invStones.Quantity : 0;
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} You don't have enough enchantement stones.\nYou need {enchantement.EnchantementStones} of {stoneInfo.ShortDescription}.\nAnd you have only {q}.");
				return false;
			}

			// Check gold
			if (creature.Gold < enchantement.Price)
			{
				Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} You don't have enough gold.");
				return false;
			}

			// Do the enchantement
			invItem.Item.Enchantement = enchantement;

			// Deduct the stones
			inv.AddItem(new ItemInstance(stoneInfo), -enchantement.EnchantementStones);

			// Deduct the coins
			creature.Gold -= enchantement.Price;

			context.Send($"You give {enchanter.ShortDescription} some enchantement stones.");

			if (enchantement.Price > 0)
			{
				context.Send($"You give {enchanter.ShortDescription} some coins.");
			}

			context.Send($"{enchanter} takes {invItem.Info.ShortDescription}, makes some gestures over it.\nThe item grows brightly.\nThe enchantement stones crumble to dust.\n{enchanter.ShortDescription} gives you {invItem.Item.Name}.");
			Tell.Execute(enchanter.GetContext(), $"{creature.ShortDescription} There you go, {creature.ShortDescription}.");

			var character = creature as Character;
			character?.Save();

			return true;
		}
	}
}
