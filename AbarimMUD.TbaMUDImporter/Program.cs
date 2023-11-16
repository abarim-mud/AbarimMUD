using AbarimMUD.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AbarimMUD.TbaMUDImporter
{
	internal class Program
	{
		static void Process()
		{
			var inputDir = "D:\\Projects\\chaos\\tbamud\\lib";

			// Find zone index file
			var indexFiles = Directory.EnumerateFiles(inputDir, "index*", SearchOption.AllDirectories);
			var zoneIndexFile = (from f in indexFiles where f.Replace("\\", "/").EndsWith("zon/index") select f).FirstOrDefault();
			if (string.IsNullOrEmpty(zoneIndexFile))
			{
				throw new Exception("Could not find zone index file('zon/index')");
			}

			var zoneFiles = new List<string>();
			using(var stream = File.OpenRead(zoneIndexFile) )
			using(var reader = new StreamReader(stream))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (line.Trim() == "$")
					{
						break;
					}

					zoneFiles.Add(line);
				}
			}
			
			// Load zones
			var zoneFolder = Path.GetDirectoryName(zoneIndexFile);
			foreach(var zoneFile in zoneFiles)
			{
				var zonePath = Path.Combine(zoneFolder, zoneFile);

				using (var stream = File.OpenRead(zonePath))
				using (var reader = new StreamReader(stream))
				{
					var line = reader.ReadLine();

					// First line is the zone id precedeed by '%'
					line = line.Substring(1);
					var id = int.Parse(line);

					line = reader.ReadLine();
					var builder = line.Substring(0, line.Length - 1).Trim();

					line = reader.ReadLine();
					var name = line.Substring(0, line.Length - 1).Trim();
					using (var db = new DataContext())
					{
						var zone = (from z in db.Zones where z.Id == id select z).FirstOrDefault();
						if (zone == null)
						{
							zone = new Zone
							{
								Id = id
							};

							db.Zones.Add(zone);
						}

						zone.Builder = builder;
						zone.Name = name;

						db.SaveChanges();
					}
				}
			}
		}

		static void Main(string[] args)
		{
			try
			{
				Process();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}