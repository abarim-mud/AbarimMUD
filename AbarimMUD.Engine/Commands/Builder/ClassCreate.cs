using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public class ClassCreate : GenericCreate<GameClass>
	{
		public ClassCreate() : base(id => GameClass.GetClassById(id))
		{
		}

		protected override void PreCreate(ExecutionContext context, GameClass newClass)
		{
			newClass.Name = newClass.Id;
			newClass.Description = newClass.Id;
		}

		protected override void PostCreate(ExecutionContext context, GameClass newClass)
		{
		}
	}
}