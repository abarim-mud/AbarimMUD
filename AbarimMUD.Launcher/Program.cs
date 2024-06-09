using System;

namespace AbarimMUD
{
	class Program
	{
		static void Main(string[] args)
		{
			Configuration.DataFolder = @"D:\Projects\AbarimMUD\Data";

			try
			{
				Server.Instance.Start();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}