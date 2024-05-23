using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum Role
	{
		Player,
		Builder,
		Administrator,
		Owner
	}

	public sealed class Character: Entity
	{
		[JsonIgnore]
		public Account Account { get; set; }

		[JsonIgnore]
		public string Name
		{
			get => Id;
			set => Id = value;
		}

		public string GameClassName { get; set; }

		public Role Role { get; set; }

		public bool IsStaff
		{
			get { return Role >= Role.Builder; }
		}

		public bool IsMale { get; set; }

		public int CurrentRoomId { get; set; }

		public int CurrentHP { get; set; }
		public int CurrentIP { get; set; }
		public int CurrentMV { get; set; }

		[JsonIgnore]
		public object Tag { get; set; }

		public Character()
		{
			Role = Role.Player;
			IsMale = true;

			CurrentHP = 200;
			CurrentIP = 100;
			CurrentMV = 250;
		}
	}
}