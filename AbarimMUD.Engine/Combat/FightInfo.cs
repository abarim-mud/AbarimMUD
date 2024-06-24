using AbarimMUD.Commands;

namespace AbarimMUD.Combat
{
	public class FightInfo
	{
		public Fight Fight { get; set; }
		public FightSide Side { get; set; }
		public ExecutionContext Target { get; set; }
	}
}
