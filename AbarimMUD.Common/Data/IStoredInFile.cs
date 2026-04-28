using Ur;

namespace AbarimMUD.Data
{
	public interface IStoredInFile : IHasId<string>
	{
		void Save();
		void Create();
	}
}
