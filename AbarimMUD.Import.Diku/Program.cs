﻿using AbarimMUD.Data;
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
			DataContext.Initialize(outputFolder, Log);

			DataContext.Register(Configuration.Storage);
			DataContext.Register(LevelInfo.Storage);
			DataContext.Register(Item.Storage);
			DataContext.Register(Skill.Storage);
			DataContext.Register(GameClass.Storage);
			DataContext.Register(Area.Storage);
			DataContext.Register(Account.Storage);
			DataContext.Register(Character.Storage);
			DataContext.Register(Social.Storage);

			DataContext.Load();

			var settings = new ImporterSettings(inputFolder, sourceType)
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
					new[] { "Northern Midgaard", "Newbie Zone", "Sewer, First Level", "Second Sewer" },
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
