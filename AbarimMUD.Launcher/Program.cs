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
				if (args.Length == 0)
				{
					Console.WriteLine("Usage: AbarimMUD.Launcher <data_folder>");
					return;
				}

				Server.Instance.Start(args[0]);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}