using AbarimMUD.Storage;

namespace AbarimMUD.Data
{
	public class SkillCostInfo
	{
		public static readonly SingleFileStorage<int, SkillCostInfo> Storage = new SingleFileStorage<int, SkillCostInfo>(l => l.Order, "skillCosts.json");

		public int Order { get; set; }

		public long Gold { get; set; }

		public SkillCostInfo()
		{
		}

		public SkillCostInfo(int order, long gold)
		{
			Order = order;
			Gold = gold;
		}

		public static SkillCostInfo GetSkillCostInfo(int order) => Storage.EnsureByKey(order);
	}
}
