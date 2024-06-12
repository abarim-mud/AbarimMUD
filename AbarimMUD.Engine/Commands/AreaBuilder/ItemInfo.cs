using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemInfo : GenericInfo<Item>
	{
		protected override Item GetById(ExecutionContext context, string id) => context.EnsureItemById(id);
	}
}
