using AbarimMUD.Data;
using AbarimMUD.Import;
using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AbarimMUD.ImportCSL
{
	internal class Importer : BaseImporter
	{
		private void ProcessRooms(XElement root, Area area)
		{
			Log("Rooms");

			var roomsElement = root.Element("Rooms");
			foreach (var roomElement in roomsElement.Nodes().OfType<XElement>())
			{
				var vnum = roomElement.EnsureInt("VNum");
				var name = roomElement.GetString("Name");
				Log($"Room {name} (#{vnum})");

				var room = new Room
				{
					Name = name,
					Description = roomElement.GetString("Description"),
					SectorType = Enum.Parse<SectorType>(roomElement.GetString("Sector")),
					Flags = roomElement.ParseFlags<RoomFlags>("Flags")
				};

				room.InitializeLists();
				var exitsElement = roomElement.Element("Exits");
				foreach (var exitElement in exitsElement.Nodes().OfType<XElement>())
				{
					var exit = new RoomExit
					{
						Direction = exitElement.EnsureEnum<Direction>("Direction"),
						Display = exitElement.GetString("Display"),
						Description = exitElement.GetString("Description"),
						Keyword = exitElement.GetString("Keywords"),
						Flags = exitElement.ParseFlags<RoomExitFlags>("Flags")
					};

					var exitInfo = new RoomExitInfo
					{
						SourceRoom = room,
						RoomExit = exit
					};

					var keyVNum = exitElement.GetInt("Keys", -1);
					if (keyVNum != -1)
					{
						exitInfo.KeyObjectVNum = keyVNum;
					}

					var targetVnum = exitElement.EnsureInt("Destination");
					if (targetVnum != -1 && targetVnum != 0)
					{
						exitInfo.TargetRoomVNum = targetVnum;
					}

					AddRoomExitToCache(exitInfo);
				}

				// Extra description
				var extraDescsElement = roomElement.Element("ExtraDescriptions");
				if (extraDescsElement != null && extraDescsElement.Nodes().OfType<XElement>().Count() > 0)
				{
					var extraDesc = extraDescsElement.Nodes().OfType<XElement>().First();
					room.ExtraKeyword = extraDesc.GetString("Keyword");
					room.ExtraDescription = extraDesc.GetString("Description");
				}

				area.Rooms.Add(room);
				AddRoomToCache(vnum, room);
			}
		}

		private void ProcessMobiles(XElement root, Area area)
		{
			Log("Mobiles");

			var mobilesElement = root.Element("NPCs");
			foreach (var mobileElement in mobilesElement.Nodes().OfType<XElement>())
			{
				var vnum = mobileElement.EnsureInt("Vnum");
				var name = mobileElement.GetString("name");
				Log($"Mobile {name} (#{vnum})");

				var mobile = new Mobile
				{
					Name = name,
					ShortDescription = mobileElement.GetString("shortDescription"),
					LongDescription = mobileElement.GetString("longDescription"),
					Description = mobileElement.GetString("description"),
					Race = mobileElement.GetEnum("race", Race.Human),
					Flags = mobileElement.ParseFlags<MobileFlags>("flags"),
					AffectedByFlags = mobileElement.ParseFlags<AffectedByFlags>("affectedBy"),
					Alignment = mobileElement.EnsureEnum<Alignment>("alignment"),
					Level = mobileElement.EnsureInt("level"),
					HitpointsRange = mobileElement.EnsureDice("HitPointDice").ToRandomRange(),
					ManaRange = mobileElement.EnsureDice("ManaPointDice").ToRandomRange(),
					ImmuneFlags = mobileElement.ParseFlags<ResistanceFlags>("immune"),
					ResistanceFlags = mobileElement.ParseFlags<ResistanceFlags>("resist"),
					VulnerableFlags = mobileElement.ParseFlags<ResistanceFlags>("vulnerable"),
					Position = mobileElement.GetEnum("DefaultPosition", MobilePosition.Stand),
					Sex = mobileElement.GetEnum("Sex", Sex.None),
					Size = mobileElement.GetEnum("Size", MobileSize.Medium),
				};
				mobile.InitializeLists();

				var damageDice = mobileElement.GetDice("DamageDice", new Dice(1, 1, 1));
				var attackType = mobileElement.EnsureEnum<AttackType>("WeaponDamageMessage");

				var gold = mobileElement.GetInt("gold", 0);
				var silver = mobileElement.GetInt("silver", 0);

				mobile.Wealth = gold * 10 + silver;

				var AcPierce = mobileElement.EnsureInt("ArmorPierce");
				var AcBash = mobileElement.EnsureInt("ArmorBash");
				var AcSlash = mobileElement.EnsureInt("ArmorSlash");
				var AcExotic = mobileElement.EnsureInt("ArmorExotic");
				var averageAc = (AcPierce + AcBash + AcSlash) / 3;
				mobile.ArmorClass = -(averageAc - 100);

				if (mobile.ArmorClass < 0)
				{
					Log($"WARNING: {mobile.Name} has negative armor class. Clamped to zero");
				}

				var guild = mobileElement.GetString("Guild");

				var attacksCount = mobile.GetAttacksCount(guild);

				var hitRoll = mobileElement.EnsureInt("hitroll");
				var accuracy = mobile.GetAccuracy(guild, hitRoll);
				for (var i = 0; i < attacksCount; ++i)
				{
					var attack = new Attack(attackType, accuracy, damageDice.ToRandomRange());
					mobile.Attacks.Add(attack);
				}

				area.Mobiles.Add(mobile);
				AddMobileToCache(vnum, mobile);
			}
		}

		private void ProcessResets(XElement root, Area area)
		{
			Log("Resets");

			var resetsElement = root.Element("Resets");
			foreach (var resetElement in resetsElement.Nodes().OfType<XElement>())
			{
				var reset = new AreaReset
				{
					ResetType = resetElement.EnsureEnum<AreaResetType>("Type"),
					Id1 = resetElement.EnsureInt("Destination"),
					Count = resetElement.EnsureInt("Count"),
					Max = resetElement.EnsureInt("Max"),
					Id2 = resetElement.EnsureInt("Vnum"),
				};

				area.Resets.Add(reset);
			}
		}

		public void Process()
		{
			var inputDir = @"D:\Projects\CrimsonStainedLands\master\CrimsonStainedLands\data\areas";
			var areaFiles = Directory.EnumerateFiles(inputDir, "*.xml", SearchOption.AllDirectories).ToArray();

			var outputDir = Path.Combine(ImportUtility.ExecutingAssemblyDirectory, "../../../../Data");
			var areasFolder = Path.Combine(outputDir, "areas");
			if (Directory.Exists(areasFolder))
			{
				Directory.Delete(areasFolder, true);
			}

			InitializeDb(outputDir);
			foreach (var areaFile in areaFiles)
			{
				var xDoc = XDocument.Load(areaFile);
				var root = xDoc.Root;

				if (root.Name != "Area")
				{
					continue;
				}

				Log($"Processing {areaFile}...");

				// Area data
				var areaData = root.Element("AreaData");
				var area = new Area
				{
					Id = Path.GetFileNameWithoutExtension(areaFile),
					Name = areaData.GetString("Name"),
					Credits = areaData.GetString("Credits"),
					Builders = areaData.GetString("Builders")
				};
				area.InitializeLists();

				// Try to get levels range from the credits
				area.ParseLevelsBuilds();

				ProcessRooms(root, area);
				ProcessMobiles(root, area);
				ProcessResets(root, area);

				DB.Areas.Update(area);
			}

			// Set ids
			SetIdsInCache();

			// Process directions
			UpdateRoomExitsReferences();

			// Update resets
			UpdateResets();

			// Update all areas
			UpdateAllAreas();
		}
	}
}
