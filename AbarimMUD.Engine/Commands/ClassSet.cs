using AbarimMUD.Commands.AreaBuilder;
using AbarimMUD.Data;

namespace AbarimMUD.Commands
{
	public class ClassSet : GenericSet<GameClass>
	{
		public ClassSet() : base((context, id) => CommandUtils.EnsureClassById(context, id))
		{
		}
	}
}
