using System;
using System.Configuration;

namespace AbarimMUD
{
	class Program
	{
		static void Main(string[] args)
		{
			Configuration.DataFolder = @"D:\Projects\AbarimMUD\Data";

			Server.Instance.Start();
			Console.ReadKey();
		}
	}
}