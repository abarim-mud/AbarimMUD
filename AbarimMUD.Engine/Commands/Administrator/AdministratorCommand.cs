using AbarimMUD.Data;

namespace AbarimMUD.Commands.Administrator
{
	public abstract class AdministratorCommand : BaseCommand
	{
		public override Role RequiredRole => Role.Administrator;
	}
}
