using AbarimMUD.Data;
using AbarimMUD.Storage;
using DikuLoad.Import;
using DikuLoad.Import.Ascii;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Import.Diku
{
	internal class Program
	{
		private static void Log(string message) => Console.WriteLine(message);

		static void MergeAreas(List<DikuLoad.Data.Area> areas, DikuLoad.Data.Area main, DikuLoad.Data.Area merged)
		{
			int minLevel, maxLevel;
			if (int.TryParse(main.MinimumLevel, out minLevel) && int.TryParse(merged.MinimumLevel, out maxLevel))
			{
				main.MinimumLevel = Math.Min(minLevel, maxLevel).ToString();
			}

			if (int.TryParse(main.MaximumLevel, out minLevel) && int.TryParse(merged.MaximumLevel, out maxLevel))
			{
				main.MaximumLevel = Math.Max(minLevel, maxLevel).ToString();
			}

			var builders = new List<string>();
			if (!string.IsNullOrEmpty(main.Builders))
			{
				builders.Add(main.Builders);
			}

			if (!string.IsNullOrEmpty(merged.Builders))
			{
				builders.Add(merged.Builders);
			}

			if (builders.Count > 0)
			{
				main.Builders = string.Join(", ", builders);
			}

			// Merge content
			main.Rooms.AddRange(merged.Rooms);
			main.Mobiles.AddRange(merged.Mobiles);

			// Remove merged area from the list
			areas.Remove(merged);
		}

		static void Process(SourceType? sourceType, string[] areasNames, string inputFolder, string outputFolder)
		{
			StorageUtility.InitializeStorage(Log);
			DataContext.Load(outputFolder);

			BaseImporter importer;

			if (sourceType != null)
			{
				var settings = new ImporterSettings(inputFolder, sourceType.Value, SubSourceType.Default)
				{
					AreasNames = areasNames,
				};

				importer = new Importer(settings);
			}
			else
			{
				var settings = new DikuLoad.Import.CSL.ImporterSettings(inputFolder)
				{
					AreasNames = areasNames,
				};

				importer = new DikuLoad.Import.CSL.Importer(settings);
			}

			importer.Process();

			/*			var astoria = (from a in importer.Areas where a.Name == "Astoria" select a).First();
						var outskirts = (from a in importer.Areas where a.Name == "Outskirts of Astoria" select a).First();
						MergeAreas(importer.Areas, astoria, outskirts);*/

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
				foreach (var reset in dikuArea.Resets)
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
						if (exit == null || exit.Tag == null)
						{
							continue;
						}

						var roomId = (int)exit.Tag;
						exit.Tag = null;

						var targetRoom = Room.GetRoomById(roomId);
						if (targetRoom == null)
						{
							Console.WriteLine($"Warning: Could not find room with id {roomId}.");
							continue;

						}

						exit.TargetRoom = targetRoom;
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
				/*				Process(null,
									new[] { "Astoria", "Sewers", "Haon Dor", "Arachnos", "Plains" },
									@"D:\Projects\CrimsonStainedLands\master\CrimsonStainedLands\data\areas",
									@"D:\Projects\AbarimMUD\Data");*/
				Process(SourceType.Soulmud,
								new[] { "Kobolds" },
								@"D:\Projects\chaos\soulmud",
								@"D:\Projects\AbarimMUD\Data");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
