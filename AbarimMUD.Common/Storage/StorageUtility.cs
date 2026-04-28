using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using Ur;

namespace AbarimMUD.Storage
{
	public static class StorageUtility
	{
		public static void InitializeStorage(Action<string> logger)
		{
			UrContext.Logger = logger;
			UrContext.BaseOptionsCreator = JsonUtils.CreateOptions;

			UrContext.Register(Configuration.Storage);
			UrContext.Register(LevelInfo.Storage);
			UrContext.Register(SkillCostInfo.Storage);
			UrContext.Register(Item.Storage);
			UrContext.Register(Ability.Storage);
			UrContext.Register(Skill.Storage);
			UrContext.Register(PlayerClass.Storage);
			UrContext.Register(Area.Storage);
			UrContext.Register(Account.Storage);
			UrContext.Register(Character.Storage);
			UrContext.Register(Social.Storage);
			UrContext.Register(GenericLoot.Storage);
			UrContext.Register(Shop.Storage);
			UrContext.Register(ForgeShop.Storage);
			UrContext.Register(ExchangeShop.Storage);
			UrContext.Register(Enchantment.Storage);
		}
	}
}
