using System;
using System.Configuration;

namespace AbarimMUD
{
	class Program
	{
		static void Main(string[] args)
		{
			Configuration.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

			Server.Instance.Start();
			Console.ReadKey();
		}
	}
}