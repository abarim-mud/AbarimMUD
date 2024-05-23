using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public sealed class Account : Entity
	{
		[JsonIgnore]
		public string Name
		{
			get => Id;
			set => Id = value;
		}

		public string PasswordHash { get; set; }
	}
}