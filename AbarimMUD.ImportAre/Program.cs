using AbarimMUD.Import;
using System;
using System.IO;

namespace AbarimMUD.ImportAre
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var outputDir = Path.Combine(ImportUtility.ExecutingAssemblyDirectory, "../../../../Data");
				var settings = new ImporterSettings(@"D:\Projects\chaos\envy22\area", outputDir, SourceType.Envy);
				var importer = new Importer(settings);
				importer.Process();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}