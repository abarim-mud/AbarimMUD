using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class MobileInstance : Creature
	{
		private Room _room;

		public Mobile Info { get; }

		public HashSet<string> Keywords => Info.Keywords;
		public override string ShortDescription => Info.ShortDescription;
		public override string Description => Info.Description;

		public override int Level => Info.Level;

		public override Sex Sex => Info.Sex;

		public override Room Room
		{
			get { return _room; }

			set
			{
				if (_room != null)
				{
					_room.Mobiles.Remove(this);
				}

				_room = value;

				if (_room != null)
				{
					_room.Mobiles.Add(this);
				}

				RoomChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public override event EventHandler RoomChanged;

		public MobileInstance(Mobile mobile)
		{
			Info = mobile ?? throw new ArgumentNullException(nameof(mobile));
			Gold = Info.Gold;

			Restore();
			RebuildInventory();

			ActiveCreatures.Add(this);
		}

		public override bool MatchesKeyword(string keyword) => Info.MatchesKeyword(keyword);

		protected override void Slain()
		{
			base.Slain();
			ActiveCreatures.Remove(this);
			Room = null;
		}

		// Mobiles ignore attacksCount, since their attacks are set explicitly
		protected override CreatureStats CreateBaseStats(int attacksCount) => Info.CreateStats();

		internal void RebuildInventory()
		{
			Inventory.Items.Clear();

			// Generic loot
			if (Info.Shop == null && Info.Guildmaster == null)
			{
				foreach (var pair in GenericLoot.Items)
				{
					if (Level <= pair.Key)
					{
						// Level match
						foreach (var genericLootRecord in pair.Value)
						{
							// Roll loot prob
							if (!Utility.RollPercentage(genericLootRecord.ProbabilityPercentage))
							{
								continue;
							}

							// Another roll to determine which item to loot
							var prob = Utility.Random1to100();
							var total = 0;
							foreach (var rec in genericLootRecord.Choice)
							{
								total += rec.ProbabilityPercentage;

								if (prob <= total)
								{
									foreach (var invRec in rec.Items.Items)
									{
										Inventory.AddItem(invRec);
									}

									break;
								}
							}
						}
					}
				}
			}

			if (Info.Shop != null)
			{
				// Shop
				Inventory.AddInventory(Info.Shop.Inventory);
			}
		}

		public override string ToString() => Info.ToString();
	}
}
