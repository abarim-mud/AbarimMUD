using AbarimMUD.Storage;
using AbarimMUD.Utils;

namespace AbarimMUD.Data
{
	public enum RoomExitNotExistantBehavior
	{
		ThrowException,
		DeleteRoomExit,
		SetNull
	}

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

			public int PeaceRegenMultiplier { get; set; } = 2;

			public int PauseBetweenFightRoundsInMs { get; set; } = 3000;
			public int CastLagInMs = 2000;

			public int NegativeRegen { get; set; }
			public ValueRange BaseMartialArtsDamage { get; set; } = new ValueRange(1, 4);

			public int RecallManaCost { get; set; } = 35;
			public int BaseBuyPricePercentage { get; set; } = 150;
			public int BaseSellPricePercentage { get; set; } = 25;
			public int XpMultiply { get; set; } = 2;
			public int HuntPauseInMs { get; set; } = 6000;
			public int DefaultAbilityCheck { get; set; } = 95;

			public RoomExitNotExistantBehavior RoomExitNotExistantBehavior { get; set; } = RoomExitNotExistantBehavior.ThrowException;
			public bool SaveDataOnStart { get; set; } = false;
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
		public static int CastLagInMs => Instance.CastLagInMs;
		public static int HitpointsRegenPercentagePerMinute => Instance.HitpointsRegenPercentagePerMinute;
		public static int ManaRegenPercentagePerMinute => Instance.ManaRegenPercentagePerMinute;
		public static int MovesRegenPercentagePerMinute => Instance.MovesRegenPercentagePerMinute;

		public static int PeaceRegenMultiplier => Instance.PeaceRegenMultiplier;

		/// <summary>
		/// Negative regen rate(per minute), used when values(hp/mana/mv) are higher than their maxes
		/// </summary>
		public static int NegativeRegen => Instance.NegativeRegen;
		public static ValueRange CharacterBarehandedDamage => Instance.BaseMartialArtsDamage;

		public static int RecallManaCost => Instance.RecallManaCost;
		public static int BaseBuyPricePercentage => Instance.BaseBuyPricePercentage;
		public static int BaseSellPricePercentage => Instance.BaseSellPricePercentage;
		public static int XpMultiply => Instance.XpMultiply;
		public static int HuntPauseInMs => Instance.HuntPauseInMs;
		public static int DefaultAbilityCheck => Instance.DefaultAbilityCheck;
		public static RoomExitNotExistantBehavior RoomExitNotExistantBehavior => Instance.RoomExitNotExistantBehavior;
		public static bool SaveDataOnStart => Instance.SaveDataOnStart;

		public static void Save() => InternalStorage.Save();
	}
}
