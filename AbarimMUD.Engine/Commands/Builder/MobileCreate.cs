using AbarimMUD.Data;
using System.Collections.Generic;

namespace AbarimMUD.Commands.Builder
{
/*	public class MobileCreate : GenericCreate<Mobile>
	{
		public MobileCreate() : base(id => Mobile.GetMobileById(id))
		{
		}

		protected override void PreCreate(ExecutionContext context, Mobile newMobile)
		{
			newMobile.Keywords = new HashSet<string> { newMobile.Id };
			newMobile.ShortDescription = newMobile.Id;
			newMobile.Description = newMobile.Id;
			newMobile.Race = Race.EnsureRaceById(Configuration.DefaultRace);
			newMobile.Class = GameClass.EnsureClassById(Configuration.DefaultClass);
			newMobile.Level = 1;
		}

		protected override void PostCreate(ExecutionContext context, Mobile newMobile)
		{
			MobileSpawn.Execute(context, newMobile.Id);
		}
	}*/
}
