using AbarimMUD.Data;
using AbarimMUD.Storage;
using AssetManagementBase;
using System;

namespace AbarimMUD.MapViewer
{
	internal class Program
	{
		static void Main(string[] args)
		{
			try
			{
				AMBConfiguration.Logger = Console.WriteLine;
				StorageUtility.InitializeStorage(Console.WriteLine);

				using (var game = new EditorGame())
				{
					game.Run();
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
