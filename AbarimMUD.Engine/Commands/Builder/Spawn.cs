﻿using AbarimMUD.Commands.Builder.OLCUtils;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public class Spawn : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 2)
			{
				context.Send($"Usage: spawn {OLCManager.SpawnString} _id_");
				return false;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return false;
			}

			if (!storage.CanSpawn)
			{
				context.Send($"Object of type {objectType} can't be spawned.");
				return false;
			}

			var objectId = parts[1].ToLower();
			var obj = context.EnsureItemById(storage, objectId);
			if (obj == null)
			{
				return false;
			}

			switch (objectType)
			{
				case "item":
					{
						var item = new ItemInstance((Item)obj);
						context.Creature.Inventory.AddItem(item, 1);
						context.Send($"{item} appeared in your inventory");
					}

					break;

				case "mobile":
					{
						// Create new mobile
						var mobile = (Mobile)obj;
						var newMobile = new MobileInstance(mobile)
						{
							Room = context.Room
						};

						new ExecutionContext(newMobile);

						foreach (var ctx in context.AllInRoom())
						{
							if (ctx.IsStaff)
							{
								ctx.Send(string.Format("{0} (#{1}) appears in a puff of smoke.",
									mobile.ShortDescription,
									mobile.Id));
							}
							else
							{
								ctx.Send(string.Format("{0} appears in a puff of smoke.",
									mobile.ShortDescription));
							}
						}
					}
					break;

				default:
					context.Send($"Spawn of {objectType} isn't implemented.");
					return false;
			}

			var character = context.Creature as Character;
			if (character != null)
			{
				character.Save();
			}

			return true;
		}
	}
}
