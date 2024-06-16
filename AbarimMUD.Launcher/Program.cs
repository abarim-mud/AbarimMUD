using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AbarimMUD
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Server.Instance.Start(@"D:\Projects\AbarimMUD\Data");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}