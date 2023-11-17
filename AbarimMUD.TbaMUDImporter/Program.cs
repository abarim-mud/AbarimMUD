using System;

namespace AbarimMUD.TbaMUDImporter
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var importer = new Importer();
				importer.Process();
				Console.ReadKey();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}