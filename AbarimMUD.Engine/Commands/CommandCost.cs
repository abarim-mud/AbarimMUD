namespace AbarimMUD.Commands
{
	public struct CommandCost
	{
		public static readonly CommandCost Zero = new CommandCost();

		public int Hitpoints;
		public int Mana;
		public int Moves;

		public CommandCost(int hitpoints, int mana, int moves)
		{
			Hitpoints = hitpoints;
			Mana = mana;
			Moves = moves;
		}
	}
}
