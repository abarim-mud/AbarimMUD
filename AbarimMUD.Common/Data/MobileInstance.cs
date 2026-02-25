using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class MobileInstance : Creature
	{
		private Room _room;

		public Mobile Info { get; }

		public override int Level => Info.Level;

		public override Sex Sex
		{
			get
			{
				if (Spawn != null && Spawn.CustomSex != null)
				{
					return Spawn.CustomSex.Value;
				}

				return Info.Sex;
			}
		}

		public override string ShortDescription
		{
			get
			{
				if (Spawn != null && !string.IsNullOrEmpty(Spawn.CustomShortDescription))
				{
					return Spawn.CustomShortDescription;
				}

				return Info.ShortDescription;
			}
		}

		public string LongDescription
		{
			get
			{
				if (Spawn != null && !string.IsNullOrEmpty(Spawn.CustomLongDescription))
				{
					return Spawn.CustomLongDescription;
				}

				return Info.LongDescription;
			}
		}

		public override string Description
		{
			get
			{
				if (Spawn != null && !string.IsNullOrEmpty(Spawn.CustomDescription))
				{
					return Spawn.CustomDescription;
				}

				return Info.Description;
			}
		}

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

		public override HashSet<string> Keywords => Info.Keywords;

		private MobileSpawn Spawn { get; set; }

		public override event EventHandler RoomChanged;

		public MobileInstance(Mobile mobile, Room room)
		{
			Info = mobile ?? throw new ArgumentNullException(nameof(mobile));
			Gold = Info.Gold;
			Room = room ?? throw new ArgumentNullException(nameof(room));

			Restore();
			RebuildInventory();

			ActiveCreatures.Add(this);
		}

		public MobileInstance(MobileSpawn mobileSpawn) : this(mobileSpawn.Mobile, mobileSpawn.Room)
		{
			Spawn = mobileSpawn;
			Spawn.Instance = this;
		}

		public override bool MatchesKeyword(string keyword)
		{
			if (Spawn != null && Spawn.CustomKeywords != null && Spawn.CustomKeywords.Count > 0)
			{
				return Spawn.CustomKeywords.StartsWithPattern(keyword);
			}

			return Info.MatchesKeyword(keyword);
		}

		protected override void Slain()
		{
			base.Slain();
			ActiveCreatures.Remove(this);
			Room = null;

			if (Spawn != null)
			{
				Spawn.Instance = null;
				Spawn = null;
			}
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
