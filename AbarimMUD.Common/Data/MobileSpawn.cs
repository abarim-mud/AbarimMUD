using AbarimMUD.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AbarimMUD.Data
{
	public class MobileSpawn
	{
		private Shop _shop;
		private Mobile _mobile;

		[Browsable(false)]
		public Mobile Mobile
		{
			get => _mobile;

			set
			{
				_mobile = value ?? throw new ArgumentNullException(nameof(value));
				Instance = null;
			}
		}

		[Browsable(false)]
		public Room Room { get; internal set; }

		[Browsable(false)]
		public MobileInstance Instance { get; internal set; }

		[OLCAlias("sex")]
		public Sex? CustomSex { get; set; }

		[OLCAlias("keywords")]
		public HashSet<string> CustomKeywords { get; set; }

		[OLCAlias("short")]
		public string CustomShortDescription { get; set; }

		[OLCAlias("long")]
		public string CustomLongDescription { get; set; }

		[OLCAlias("description")]
		public string CustomDescription { get; set; }

		public PlayerClass Guildmaster { get; set; }

		public Shop Shop
		{
			get => _shop;

			set
			{
				if (value == _shop)
				{
					return;
				}

				_shop = value;

				// Rebuild inventories
				foreach (var creature in Creature.ActiveCreatures)
				{
					var asMobile = creature as MobileInstance;
					if (asMobile == null)
					{
						continue;
					}

					if (asMobile.Info.Id == Mobile.Id)
					{
						asMobile.RebuildInventory();
					}
				}
			}
		}

		public ForgeShop ForgeShop { get; set; }

		public ExchangeShop ExchangeShop { get; set; }

		public bool HasCustomParams
		{
			get
			{
				return (CustomKeywords != null && CustomKeywords.Count > 0) ||
					!string.IsNullOrEmpty(CustomShortDescription) ||
					!string.IsNullOrEmpty(CustomLongDescription) ||
					!string.IsNullOrEmpty(CustomDescription) ||
					Guildmaster != null ||
					Shop != null ||
					ForgeShop != null ||
					ExchangeShop != null;
			}
		}
	}
}