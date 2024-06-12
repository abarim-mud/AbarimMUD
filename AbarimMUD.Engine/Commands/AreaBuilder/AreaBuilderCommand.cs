using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public abstract class AreaBuilderCommand : BaseCommand
	{
		public override Role RequiredType => Role.Builder;
	}
}