using System;

namespace AbarimMUD.ImportAre
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var importer = new Importer();
				importer.Process();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}