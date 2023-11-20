using System.ComponentModel.DataAnnotations.Schema;

namespace AbarimMUD.Data
{
	public enum Role
	{
		Player,
		AreaBuilder,
		WorldBuilder,
		LowAdministrator,
		HighAdministrator,
		Owner
	}

	public sealed class Character : Entity
	{
		public int AccountId { get; set; }
		public Account Account { get; set; }

		public string Name { get; set; }

		public string GameClassName { get; set; }

		public Role Role { get; set; }

		public bool IsStaff
		{
			get { return Role >= Role.AreaBuilder; }
		}

		public bool IsMale { get; set; }

		public int CurrentRoomId { get; set; }

		public int CurrentHP { get; set; }
		public int CurrentIP { get; set; }
		public int CurrentMV { get; set; }

		[NotMapped]
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