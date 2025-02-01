namespace AbarimMUD.Utils
{
	public struct RandomRange
	{
		public int Minimum;
		public int Maximum;

		public RandomRange(int min, int max)
		{
			Minimum = min;
			Maximum = max;
		}

		public override string ToString() => $"{Minimum}-{Maximum}";

		public int Generate() => Utility.RandomRange(Minimum, Maximum);
	}
}
