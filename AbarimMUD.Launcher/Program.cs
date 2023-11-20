using System;

namespace AbarimMUD
{
	class Program
	{
		static void Main(string[] args)
		{
			Server.Instance.Start();
			Console.ReadKey();
		}
	}
}