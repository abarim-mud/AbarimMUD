using System.Text.Json.Serialization;
using Ur;

namespace AbarimMUD.Data
{
	public class SkillCostInfo: IHasId<int>
	{
		public static readonly SingleFileStorage<int, SkillCostInfo> Storage = new SingleFileStorage<int, SkillCostInfo>("skillCosts.json");

		[JsonIgnore]
		public int Id
		{
			get => Order;
			set => Order = value;
		}

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
