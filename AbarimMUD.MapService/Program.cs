using System.IO;
using System;
using AbarimMUD.Storage;
using AbarimMUD.Data;
using MUDMapBuilder;
using AbarimMUD.Common.Tools;
using System.Collections.Generic;
using System.Threading;

namespace AbarimMUD.MapService;

internal static class Program
{
	private const string OutputFolder = "output";
	private const string PngFolder = "png";
	private const string JsonFolder = "json";
	private const string MapsInfoPath = "mapsInfo.json";

	static void Log(string message) => Console.WriteLine($"{DateTime.Now}: {message}");

	static void Process(string dataFolder)
	{
		StorageUtility.InitializeStorage(Log);

		while (true)
		{
			Log($"Waking up...");

			var mapsInfoPath = Path.Combine(OutputFolder, MapsInfoPath);

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

			var pngFolder = Path.Combine(OutputFolder, PngFolder);
			var jsonFolder = Path.Combine(OutputFolder, JsonFolder);

			ToolsUtility.EnsureFolder(OutputFolder);
			ToolsUtility.EnsureFolder(pngFolder);
			ToolsUtility.EnsureFolder(jsonFolder);

			foreach (var area in Area.Storage)
			{
				var skip = true;

				var jsonPath = Path.Combine(jsonFolder, $"{area.Id}.json");
				if (!File.Exists(jsonPath))
				{
					Log($"MMB Json File {jsonPath} doesnt exist.");
					skip = false;
				}

				var pngPath = Path.Combine(pngFolder, $"{area.Id}.png");
				if (skip && !File.Exists(pngPath))
				{
					Log($"MMB Png FIle {jsonPath} doesnt exist.");
					skip = false;
				}

				if (skip && !mapsInfo.ContainsKey(area.Id))
				{
					Log($"Maps info file doesnt have area with id '{area.Id}'.");
					skip = false;
				}

				var mapPath = Area.Storage.BuildPath(area);
				var fi = new FileInfo(mapPath);

				if (skip && fi.LastWriteTime > mapsInfo[area.Id])
				{
					Log($"Maps info area with id '{area.Id}' last write date is less than actual file last write data({mapsInfo[area.Id]} < {fi.LastWriteTime}).");
					skip = false;
				}

				mapsInfo[area.Id] = fi.LastWriteTime;

				if (skip)
				{
					Log($"Maps info area with id '{area.Id}' doesnt need to be regenerated.");
					continue;
				}

				var project = new MMBProject(area.ToMMBArea());

				File.WriteAllText(jsonPath, project.ToJson());

				var buildResult = MapBuilder.MultiRun(project, Log);
				if (buildResult == null)
				{
					Log("Error: No rooms to process");
					return;
				}

				var pngData = buildResult.Last.BuildPng().PngData;
				File.WriteAllBytes(pngPath, pngData);

				if (buildResult.ResultType != ResultType.Success)
				{
					Log($"Error: {buildResult.ResultType}. Try raising amount of MaxSteps in the BuildOptions.");
				}
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
