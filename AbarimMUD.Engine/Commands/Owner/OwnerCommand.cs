using AbarimMUD.Data;

namespace AbarimMUD.Commands.Owner
{
	public abstract class OwnerCommand : BaseCommand
	{
		public override Role RequiredType
		{
			get { return Role.Owner; }
		}
	}
}