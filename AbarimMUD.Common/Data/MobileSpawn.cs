using AbarimMUD.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AbarimMUD.Data
{
	public class MobileSpawn
	{
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

		public bool HasCustomParams
		{
			get
			{
				return CustomSex != null ||
					(CustomKeywords != null && CustomKeywords.Count > 0) ||
					!string.IsNullOrEmpty(CustomShortDescription) ||
					!string.IsNullOrEmpty(CustomLongDescription) ||
					!string.IsNullOrEmpty(CustomDescription);
			}
		}
	}
}