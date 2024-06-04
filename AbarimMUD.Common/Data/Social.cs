using AbarimMUD.Storage;

namespace AbarimMUD.Data
{
	/// <summary>
	/// Action type tells us what type of social it is.  Used so we can have mobs react to
	/// player actions if we want to. 
	/// </summary>
	public enum SocialActionType
	{
		/// <summary>
		/// Not set or not one of the other types.
		/// </summary>
		None = 0,

		/// <summary>
		/// A friendly social action.
		/// </summary>
		Friendly = 1,

		/// <summary>
		/// A neutral action.
		/// </summary>
		Neutral = 2,

		/// <summary>
		/// An aggressive action.
		/// </summary>
		Aggressive = 3,

		/// <summary>
		/// A greeting.
		/// </summary>
		Greeting = 4,

		/// <summary>
		/// A goodbye.
		/// </summary>
		Goodbye = 5,

		/// <summary>
		/// An insulting or offensive action.
		/// </summary>
		Insulting = 6
	}

	public class Social
	{
		public static readonly SingleFileStorageString<Social> Storage = new SingleFileStorageString<Social>(s => s.Name, "socials.json");

		public string Name { get; set; }
		public string CharNoArgument { get; set; }
		public string OthersNoArgument { get; set; }
		public string CharFound { get; set; }
		public string OthersFound { get; set; }
		public string VictimFound { get; set; }
		public string CharSelf { get; set; }
		public string CharAuto { get; set; }
		public string OthersSelf { get; set; }

		/// <summary>
		/// Gets the type of social action.
		/// </summary>
		public SocialActionType Type { get; set; }

		public static Social GetSocialByName(string name) => Storage.GetByKey(name);
		public static Social EnsureSocialByName(string name) => Storage.EnsureByKey(name);
		public static Social LookupSocial(string name) => Storage.Lookup(name);
		public static void SaveSocials() => Storage.SaveAll();
	}
}
