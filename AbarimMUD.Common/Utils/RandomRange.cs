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

		public static RandomRange operator +(RandomRange r1, RandomRange r2)
		{
			return new RandomRange(r1.Minimum + r2.Minimum, r1.Maximum + r2.Maximum);
		}
	}
}
