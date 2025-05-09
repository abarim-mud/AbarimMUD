﻿using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class ForgeCommand : PlayerCommand
	{
		public override string HelpText => "Usage:\n'forge list' shows list of available forge.\n'forge _forgeName_' does the actual forge.";


		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				ShowHelp(context);
				return true;
			}

			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Info.ForgeShop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var forges = shopKeeper.Info.ForgeShop.Forges;
			if (data.EqualsToIgnoreCase("list"))
			{
				var grid = new AsciiGrid();
				grid.SetHeader(0, "Item");
				grid.SetHeader(1, "Components");
				grid.SetHeader(2, "Price");

				var y = 0;
				foreach (var f in forges)
				{
					grid.SetValue(0, y, f.Result.ShortDescription);

					var sb = new StringBuilder();

					for (var i = 0; i < f.Components.Count; i++)
					{
						var cp = f.Components[i];
						sb.Append(cp.Name);

						if (cp.Quantity > 1)
						{
							sb.Append($" ({cp.Quantity})");
						}

						if (i < f.Components.Count - 1)
						{
							sb.Append(", ");
						}
					}

					grid.SetValue(1, y, sb.ToString());
					grid.SetValue(2, y, f.Price.ToString());

					++y;
				}

				context.Send("You can forge following items:");
				context.Send(grid.ToString());

				return true;
			}

			// Find forge
			var creature = context.Creature;
			var inv = creature.Inventory;
			var forge = (from f in forges where f.Result.MatchesKeyword(data) select f).FirstOrDefault();
			if (forge == null)
			{
				Tell.Execute(shopKeeper.GetContext(), $"{creature.ShortDescription} I don't know how to forge '{data}'.");
				return false;
			}

			// Check cps
			foreach (var cp in forge.Components)
			{
				var invItem = (from i in inv where ItemInstance.AreEqual(i.Item, cp.Item) select i).FirstOrDefault();

				if (invItem == null)
				{
					Tell.Execute(shopKeeper.GetContext(), $"{creature.ShortDescription} You don't have any {cp.Item.Name}.");
					return false;
				}

				if (invItem.Quantity < cp.Quantity)
				{
					Tell.Execute(shopKeeper.GetContext(), $"{creature.ShortDescription} You don't have enough amount of {cp.Item.Name}.");
					return false;
				}
			}

			// Check gold
			if (creature.Gold < forge.Price)
			{
				Tell.Execute(shopKeeper.GetContext(), $"{creature.ShortDescription} You don't have enough gold.");
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
				context.Send($"You give {shopKeeper.ShortDescription} some items.");
			}

			if (forge.Price > 0)
			{
				context.Send($"You give {shopKeeper.ShortDescription} some coins.");
			}

			context.Send($"{shopKeeper} forges {item.Name} and gives it to you.");
			Tell.Execute(shopKeeper.GetContext(), $"{creature.ShortDescription} There you go, {creature.ShortDescription}.");

			var character = creature as Character;
			character?.Save();

			return true;
		}
	}
}
