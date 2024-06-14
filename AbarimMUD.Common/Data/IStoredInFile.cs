namespace AbarimMUD.Data
{
	public interface IHasId<IdType>
	{
		IdType Id { get; set; }
	}

	public interface IStoredInFile: IHasId<string>
	{
		void Save();
		void Create();
	}
}
