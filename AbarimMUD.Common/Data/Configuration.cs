using AbarimMUD.Storage;
using AbarimMUD.Utils;

namespace AbarimMUD.Data
{
	public static class Configuration
	{
		private static readonly CustomStorage<ConfigurationInstance> InternalStorage = new CustomStorage<ConfigurationInstance>("settings.json");

		private class ConfigurationInstance
		{
			public int MaximumLevel { get; set; }
			public int ServerPort { get; set; }
			public string WebServiceUrl { get; set; }
			public string SplashFile { get; set; }
			public int StartRoomId { get; set; }
			public string DefaultCharacter { get; set; }

			public int HitpointsRegenPercentagePerMinute { get; set; } = 5;
			public int ManaRegenPercentagePerMinute { get; set; } = 5;
			public int MovesRegenPercentagePerMinute { get; set; } = 5;

			public int HitpointsRegenMinimumPerMinute { get; set; } = 20;
			public int ManaRegenMinimumPerMinute { get; set; } = 20;
			public int MovesRegenMinimumPerMinute { get; set; } = 20;

			public int PeaceRegenMultiplier { get; set; } = 2;

			public int PauseBetweenFightRoundsInMs { get; set; }
			public int NegativeRegen { get; set; }
			public ValueRange BaseMartialArtsDamage { get; set; } = new ValueRange(1, 4);

			public int[] PrimarySkillsLevelsConstraints { get; set; } = new[] { 1, 5, 10, 15, 20 };


			public int[] NonPrimarySkillsLevelsConstraints { get; set; } = new[] { 1, 10, 20, 30, 40 };
			public int RecallManaCost { get; set; } = 35;
			public int BaseBuyPricePercentage { get; set; } = 150;
			public int BaseSellPricePercentage { get; set; } = 25;
			public int XpMultiply { get; set; } = 2;
			public int HuntPauseInMs { get; set; } = 6000;
		}

		public static BaseStorage Storage => InternalStorage;

		private static ConfigurationInstance Instance => InternalStorage.Item;

		public static int MaximumLevel => Instance.MaximumLevel;
		public static int ServerPort => Instance.ServerPort;
		public static string WebServiceUrl => Instance.WebServiceUrl;
		public static string SplashFile => Instance.SplashFile;
		public static int StartRoomId => Instance.StartRoomId;
		public static string DefaultCharacter => Instance.DefaultCharacter;
		public static int PauseBetweenFightRoundsInMs => Instance.PauseBetweenFightRoundsInMs;
		public static int HitpointsRegenPercentagePerMinute => Instance.HitpointsRegenPercentagePerMinute;
		public static int ManaRegenPercentagePerMinute => Instance.ManaRegenPercentagePerMinute;
		public static int MovesRegenPercentagePerMinute => Instance.MovesRegenPercentagePerMinute;
		public static int HitpointsRegenMinimumPerMinute => Instance.HitpointsRegenMinimumPerMinute;
		public static int ManaRegenMinimumPerMinute => Instance.ManaRegenMinimumPerMinute;
		public static int MovesRegenMinimumPerMinute => Instance.MovesRegenMinimumPerMinute;

		public static int PeaceRegenMultiplier => Instance.PeaceRegenMultiplier;

		/// <summary>
		/// Negative regen rate(per minute), used when values(hp/mana/mv) are higher than their maxes
		/// </summary>
		public static int NegativeRegen => Instance.NegativeRegen;
		public static ValueRange CharacterBarehandedDamage => Instance.BaseMartialArtsDamage;

		public static int[] PrimarySkillsLevelsConstraints => Instance.PrimarySkillsLevelsConstraints;

		public static int[] NonPrimarySkillsLevelsConstraints => Instance.NonPrimarySkillsLevelsConstraints;

		public static int RecallManaCost => Instance.RecallManaCost;
		public static int BaseBuyPricePercentage => Instance.BaseBuyPricePercentage;
		public static int BaseSellPricePercentage => Instance.BaseSellPricePercentage;
		public static int XpMultiply => Instance.XpMultiply;
		public static int HuntPauseInMs => Instance.HuntPauseInMs;

		public static void Save() => InternalStorage.Save();
	}
}
