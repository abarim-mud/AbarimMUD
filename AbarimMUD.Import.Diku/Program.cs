using AbarimMUD.Data;
using AbarimMUD.Storage;
using DikuLoad.Import.Ascii;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AbarimMUD.Import.Diku
{
	internal class Program
	{
		private static void Log(string message) => Console.WriteLine(message);

		static void Process(SourceType sourceType, string nameFilter, string inputFolder, string outputFolder)
		{
			DataContext.Initialize(outputFolder, Log);

			DataContext.Register(Race.Storage);
			DataContext.Register(GameClass.Storage);
			DataContext.Register(Mobile.Storage);
			DataContext.Register(Item.Storage);
			DataContext.Register(Skill.Storage);
			DataContext.Register(Area.Storage);
			DataContext.Register(Account.Storage);
			DataContext.Register(Character.Storage);
			DataContext.Register(Social.Storage);

			DataContext.Load();

			var settings = new ImporterSettings(inputFolder, sourceType)
			{
				AreaNameFilter = nameFilter
			};

			var importer = new Importer(settings);

			importer.Process();

			// Convert DikuLoad areas to AM Areas
			var outputAreasCount = 0;
			foreach (var dikuArea in importer.Areas)
			{
				if (dikuArea.Rooms == null || dikuArea.Rooms.Count == 0)
				{
					Console.WriteLine($"Warning: Area '{dikuArea.Name} has no rooms. Skipping.");
					continue;
				}

				var area = dikuArea.ToMMBArea();
				area.Save();

				++outputAreasCount;
			}

			// Update exits
			foreach(var area in Area.Storage)
			{
				foreach(var room in area.Rooms)
				{
					foreach(var pair in room.Exits)
					{
						var exit = pair.Value;
						var roomId = (int)exit.Tag;

						exit.TargetRoom = Room.EnsureRoomById(roomId);
						exit.Tag = null;
					}
				}

				area.Save();
			}

			Console.WriteLine($"Wrote {outputAreasCount} areas.");
		}

		static void Main(string[] args)
		{
			try
			{
				Process(SourceType.Circle,
					"Northern Midgaard",
					@"D:\Projects\chaos\tbamud\lib\world",
					@"D:\Projects\AbarimMUD\Data");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
