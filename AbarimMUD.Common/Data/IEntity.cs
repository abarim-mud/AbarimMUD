namespace AbarimMUD.Data
{
	public interface IEntity
	{
		string Id { get; set; }

		void Save();
		void Create();
	}
}
