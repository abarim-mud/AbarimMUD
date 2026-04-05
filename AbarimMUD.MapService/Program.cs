using AbarimMUD.Common.Tools;
using AbarimMUD.Data;
using AbarimMUD.Storage;
using MUDMapBuilder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AbarimMUD.MapService;

internal static class Program
{
	private const string OutputFolder = "output";
	private const string PngFolder = "png";
	private const string JsonFolder = "json";
	private const string HtmlFile = "index.html";
	private const string MapsInfoFile = "mapsInfo.json";

	static void Log(string message) => Console.WriteLine($"{DateTime.Now}: {message}");

	static void Process(string dataFolder)
	{
		StorageUtility.InitializeStorage(Log);

		var outputFolder = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), OutputFolder);

		while (true)
		{
			Log($"Waking up...");

			var mapsInfoPath = Path.Combine(outputFolder, MapsInfoFile);

			Dictionary<string, DateTime> mapsInfo;
			if (!File.Exists(mapsInfoPath))
			{
				mapsInfo = new Dictionary<string, DateTime>();
			}
			else
			{
				mapsInfo = ToolsUtility.DeserializeFromFile<Dictionary<string, DateTime>>(mapsInfoPath);
			}

			DataContext.Load(dataFolder);

			var pngFolder = Path.Combine(outputFolder, PngFolder);
			var jsonFolder = Path.Combine(outputFolder, JsonFolder);

			ToolsUtility.EnsureFolder(outputFolder);
			ToolsUtility.EnsureFolder(pngFolder);
			ToolsUtility.EnsureFolder(jsonFolder);

			var changed = false;

			// Build complete dictionaries of areas,rooms and area exits
			var allAreas = new Dictionary<string, MMBArea>();
			var allRooms = new Dictionary<int, MMBRoom>();
			var allAreaExits = new Dictionary<int, MMBRoom>();
			foreach (var area in Area.Storage)
			{
				var mmbArea = area.ToMMBArea();
				allAreas[area.Id] = mmbArea;
				foreach (var room in mmbArea.Rooms)
				{
					if (allRooms.ContainsKey(room.Id))
					{
						throw new Exception($"Dublicate room id. New room: {room}. Old room: {allRooms[room.Id]}");
					}

					allRooms[room.Id] = room;

					var areaExit = room.Clone();

					areaExit.Name = $"To {area.Name} #{areaExit.Id}";
					areaExit.FrameColor = Color.Blue;
					areaExit.Color = Color.Blue;

					allAreaExits[room.Id] = areaExit;
				}
			}

			// Now add areas exits
			foreach (var pair in allAreas)
			{
				var area = pair.Value;
				var areaExits = new Dictionary<int, MMBRoom>();
				foreach (var room in area.Rooms)
				{
					foreach (var exit in room.Connections)
					{
						MMBRoom inAreaRoom;
						inAreaRoom = (from r in area.Rooms where r.Id == exit.Value.RoomId select r).FirstOrDefault();
						if (inAreaRoom != null)
						{
							continue;
						}

						areaExits[exit.Value.RoomId] = allAreaExits[exit.Value.RoomId];
					}
				}

				foreach (var pair2 in areaExits)
				{
					area.Add(pair2.Value);
				}
			}

			// Finally do the export
			var pngMap = new Dictionary<string, string>();
			foreach (var pair in allAreas)
			{
				var area = pair.Value;
				var skip = true;

				var id = pair.Key;
				var jsonPath = Path.Combine(jsonFolder, $"{id}.json");
				if (!File.Exists(jsonPath))
				{
					Log($"MMB Json File {jsonPath} doesnt exist.");
					skip = false;
				}

				var pngPath = Path.Combine(pngFolder, $"{id}.png");
				pngMap[id] = pngPath;
				if (skip && !File.Exists(pngPath))
				{
					Log($"MMB Png FIle {jsonPath} doesnt exist.");
					skip = false;
				}

				if (skip && !mapsInfo.ContainsKey(id))
				{
					Log($"Maps info file doesnt have area with id '{id}'.");
					skip = false;
				}

				var mapPath = Area.Storage.BuildPath((Area)area.Tag);
				var fi = new FileInfo(mapPath);

				if (skip && fi.LastWriteTime > mapsInfo[id])
				{
					Log($"Maps info area with id '{id}' last write date is less than actual file last write data({mapsInfo[id]} < {fi.LastWriteTime}).");
					skip = false;
				}

				mapsInfo[id] = fi.LastWriteTime;

				if (skip)
				{
					Log($"Maps info area with id '{id}' doesnt need to be regenerated.");
					continue;
				}

				var project = new MMBProject(area);

				File.WriteAllText(jsonPath, project.ToJson());

				var buildResult = MapBuilder.MultiRun(project, Log);
				if (buildResult == null)
				{
					Log("Error: No rooms to process");
				}
				else if (buildResult.ResultType != ResultType.Success)
				{
					Log($"Error: {buildResult.ResultType}. Try raising amount of MaxSteps in the BuildOptions.");
				}
				else
				{
					var pngData = buildResult.Last.BuildPng().PngData;
					File.WriteAllBytes(pngPath, pngData);

					changed = true;
				}
			}

			if (changed)
			{
				// Write html
				var htmlPath = Path.Combine(outputFolder, HtmlFile);

				var sb = new StringBuilder();

				sb.AppendLine("<html><body><table>");

				foreach (var pair in pngMap)
				{
					sb.AppendLine($"<tr><td><a href='{pair.Value}'>{pair.Key}</a></td></tr>");
				}

				sb.AppendLine("</table></body></html>");

				File.WriteAllText(htmlPath, sb.ToString());
			}

			ToolsUtility.SerializeToFile(mapsInfoPath, mapsInfo);

			DataContext.Clear();

			Thread.Sleep(60 * 1000);
		}
	}

	static void Main(string[] args)
	{
		try
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: AbarimMUD.MapService.exe <dataFolder>");
				return;
			}

			Process(args[0]);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}
}
