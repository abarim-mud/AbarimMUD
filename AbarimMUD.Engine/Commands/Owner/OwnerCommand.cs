using AbarimMUD.Data;

namespace AbarimMUD.Commands.Owner
{
	public abstract class OwnerCommand : BaseCommand
	{
		public override Role RequiredRole => Role.Owner;
	}
}