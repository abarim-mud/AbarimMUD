using AbarimMUD.Data;
using AbarimMUD.Storage;
using DikuLoad.Import.Ascii;
using System;

namespace AbarimMUD.Import.Diku
{
	internal class Program
	{
		private static void Log(string message) => Console.WriteLine(message);

		static void Process(SourceType sourceType, string[] areasNames, string inputFolder, string outputFolder)
		{
			StorageUtility.InitializeStorage(Log);
			DataContext.Load(outputFolder);

			var settings = new ImporterSettings(inputFolder, sourceType, SubSourceType.Default)
			{
				AreasNames = areasNames,
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

				var area = dikuArea.ToAmArea();

				// Set resets
				foreach(var reset in dikuArea.Resets)
				{
					if (reset.ResetType != DikuLoad.Data.AreaResetType.NPC)
					{
						continue;
					}

					area.MobileResets.Add(new AreaMobileReset(reset.MobileVNum, reset.Value4));
				}

				area.Save();

				++outputAreasCount;
			}

			// Update exits
			foreach (var area in Area.Storage)
			{
				foreach (var room in area.Rooms)
				{
					foreach (var pair in room.Exits)
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
					new[] { "Northern Midgaard", "Newbie Zone" },
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
