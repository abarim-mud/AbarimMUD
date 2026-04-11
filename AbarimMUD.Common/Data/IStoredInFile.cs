using AbarimMUD.Attributes;
using System.ComponentModel;

namespace AbarimMUD.Data
{
	public interface IHasId<IdType>
	{
		[Browsable(false)]
		[OLCIgnore]
		IdType Id { get; set; }
	}

	public interface IStoredInFile : IHasId<string>
	{
		void Save();
		void Create();
	}
}
