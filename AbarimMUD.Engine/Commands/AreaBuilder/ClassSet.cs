using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ClassSet : GenericSet<GameClass>
	{
		public ClassSet() : base((context, id) => context.EnsureClassById(id))
		{
		}
	}
}
