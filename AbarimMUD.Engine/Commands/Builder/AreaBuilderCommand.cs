using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public abstract class BuilderCommand : BaseCommand
	{
		public override Role RequiredRole => Role.Builder;
	}
}