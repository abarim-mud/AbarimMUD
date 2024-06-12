using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class MobileInfo : GenericInfo<Mobile>
	{
		protected override Mobile GetById(ExecutionContext context, string id) => context.EnsureMobileById(id);
	}
}
