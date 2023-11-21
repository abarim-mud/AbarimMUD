namespace AbarimMUD.Data
{
	public sealed class Account : Entity
	{
		public string Name { get; set; }
		public string PasswordHash { get; set; }
	}
}