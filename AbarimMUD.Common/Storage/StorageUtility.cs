using AbarimMUD.Data;
using System;

namespace AbarimMUD.Storage
{
	public static class StorageUtility
	{
		public static void InitializeStorage(Action<string> logger)
		{
			DataContext.Logger = logger;

			DataContext.Register(Configuration.Storage);
			DataContext.Register(LevelInfo.Storage);
			DataContext.Register(SkillCostInfo.Storage);
			DataContext.Register(Item.Storage);
			DataContext.Register(Ability.Storage);
			DataContext.Register(Skill.Storage);
			DataContext.Register(PlayerClass.Storage);
			DataContext.Register(Area.Storage);
			DataContext.Register(Account.Storage);
			DataContext.Register(Character.Storage);
			DataContext.Register(Social.Storage);
			DataContext.Register(GenericLoot.Storage);
			DataContext.Register(Shop.Storage);
			DataContext.Register(Forge.Storage);
			DataContext.Register(ForgeShop.Storage);
			DataContext.Register(ExchangeShop.Storage);
			DataContext.Register(Enchantment.Storage);
		}
	}
}
