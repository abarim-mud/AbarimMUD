using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public abstract class PlayerCommand : BaseCommand
	{
		public override Role RequiredRole => Role.Player;
	}
}