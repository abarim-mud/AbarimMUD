using AbarimMUD.Data;
using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AbarimMUD.ImportCSL
{
	internal class Importer
	{
		private readonly Dictionary<int, Room> _roomsByVnums = new Dictionary<int, Room>();
		private readonly Dictionary<int, Mobile> _mobilesByVnums = new Dictionary<int, Mobile>();
		private readonly Dictionary<int, GameObject> _objectsByVnums = new Dictionary<int, GameObject>();

		private class RoomExitInfo
		{
			public Room SourceRoom { get; set; }
			public RoomExit RoomExit { get; set; }
			public int? TargetRoomVNum { get; set; }
			public int? KeyObjectVNum { get; set; }
		}

		private readonly List<RoomExitInfo> _tempDirections = new List<RoomExitInfo>();

		public static void Log(string message) => Console.WriteLine(message);

		private Room GetRoomByVnum(int vnum) =>
			(from m in _roomsByVnums where m.Key == vnum select m.Value).FirstOrDefault();

		private Room EnsureRoomByVnum(int vnum) =>
			(from m in _roomsByVnums where m.Key == vnum select m.Value).First();

		private Mobile GetMobileByVnum(int vnum) =>
			(from m in _mobilesByVnums where m.Key == vnum select m.Value).FirstOrDefault();

		private Mobile EnsureMobileByVnum(int vnum) =>
			(from m in _mobilesByVnums where m.Key == vnum select m.Value).First();

		private GameObject GetObjectByVnum(int vnum) =>
			(from m in _objectsByVnums where m.Key == vnum select m.Value).FirstOrDefault();

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

					_tempDirections.Add(exitInfo);
				}

				// Extra description
				var extraDescsElement = roomElement.Element("ExtraDescriptions");
				if (extraDescsElement != null && extraDescsElement.Nodes().OfType<XElement>().Count() > 0)
				{
					var extraDesc = extraDescsElement.Nodes().OfType<XElement>().First();
					room.ExtraKeyword = extraDesc.GetString("Keyword");
					room.ExtraDescription = extraDesc.GetString("Description");
				}

				_roomsByVnums[vnum] = room;

				area.Rooms.Add(room);
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
					HitRoll = mobileElement.EnsureInt("hitroll"),
					HitDice = mobileElement.EnsureDice("HitPointDice"),
					ManaDice = mobileElement.EnsureDice("ManaPointDice"),
					DamageDice = mobileElement.GetDice("DamageDice", new Dice(1, 1, 1)),
					AttackType = mobileElement.EnsureEnum<AttackType>("WeaponDamageMessage"),
					AcPierce = mobileElement.EnsureInt("ArmorPierce"),
					AcBash = mobileElement.EnsureInt("ArmorBash"),
					AcSlash = mobileElement.EnsureInt("ArmorSlash"),
					AcExotic = mobileElement.EnsureInt("ArmorExotic"),
					ImmuneFlags = mobileElement.ParseFlags<ResistanceFlags>("immune"),
					ResistanceFlags = mobileElement.ParseFlags<ResistanceFlags>("resist"),
					VulnerableFlags = mobileElement.ParseFlags<ResistanceFlags>("vulnerable"),
					DefaultPosition = mobileElement.GetEnum("DefaultPosition", MobilePosition.Stand),
					Sex = mobileElement.GetEnum("Sex", Sex.None),
					Size = mobileElement.GetEnum("Size", MobileSize.Medium),
				};

				var gold = mobileElement.GetInt("gold", 0);
				var silver = mobileElement.GetInt("silver", 0);

				mobile.Wealth = gold * 10 + silver;

				var averageAc = (mobile.AcPierce + mobile.AcBash + mobile.AcSlash) / 3;
				mobile.ArmorClass = -(averageAc - 100);

				if (mobile.ArmorClass < 0)
				{
					Log($"WARNING: {mobile.Name} has negative armor class. Clamped to zero");
				}

				var attacksCount = mobile.GetAttacksCount();
				var accuracy = mobile.GetAccuracy() + mobile.HitRoll * 10;
				for (var i = 0; i < attacksCount; ++i)
				{
					var expr = GoRogue.DiceNotation.Dice.Parse(mobile.DamageDice.ToString());
					var min = expr.MinRoll();
					var max = expr.MaxRoll();

					var attack = new Attack(mobile.AttackType, accuracy, min, max);
					mobile.Attacks.Add(attack);
				}

				area.Mobiles.Add(mobile);

				_mobilesByVnums[vnum] = mobile;
			}
		}

		private static void SetIds<T>(Dictionary<int, T> data) where T : AreaEntity
		{
			var id = 1;
			foreach (var pair in data)
			{
				pair.Value.Id = id;
				++id;
			}
		}

		public void Process()
		{
			var inputDir = @"D:\Projects\CrimsonStainedLands\master\CrimsonStainedLands\data\areas";
			var areaFiles = Directory.EnumerateFiles(inputDir, "*.xml", SearchOption.AllDirectories).ToArray();

			var outputDir = Path.Combine(Utility.ExecutingAssemblyDirectory, "../../../../Data");
			var areasFolder = Path.Combine(outputDir, "areas");
			if (Directory.Exists(areasFolder))
			{
				Directory.Delete(areasFolder, true);
			}

			var db = new DataContext(outputDir, Log);
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
					Name = areaData.GetString("Name"),
					Credits = areaData.GetString("Credits"),
					Builders = areaData.GetString("Builders")
				};

				ProcessRooms(root, area);
				ProcessMobiles(root, area);

				db.Areas.Update(area);
			}

			// Set ids
			SetIds(_roomsByVnums);
			SetIds(_mobilesByVnums);
			SetIds(_objectsByVnums);

			// Process directions
			Log("Updating directions");
			foreach (var dir in _tempDirections)
			{
				var exit = dir.RoomExit;
				if (dir.TargetRoomVNum != null)
				{
					var targetRoom = GetRoomByVnum(dir.TargetRoomVNum.Value);
					if (targetRoom == null)
					{
						Log($"WARNING: Unable to set target room for exit. Room with vnum {dir.TargetRoomVNum.Value} doesnt exist.");
					}

					exit.TargetRoom = targetRoom;
				}

				if (dir.KeyObjectVNum != null)
				{
					var keyObj = GetObjectByVnum(dir.KeyObjectVNum.Value);
					if (keyObj != null)
					{
						exit.KeyObjectId = keyObj.Id;
					}
				}

				dir.SourceRoom.Exits[exit.Direction] = exit;
			}

			// Update resets
			foreach (var area in db.Areas)
			{
				foreach (var reset in area.Resets)
				{
					switch (reset.ResetType)
					{
						case AreaResetType.Mobile:
							reset.Value2 = EnsureMobileByVnum(reset.Value2).Id;
							reset.Value4 = EnsureRoomByVnum(reset.Value4).Id;
							break;
						case AreaResetType.GameObject:
							break;
						case AreaResetType.Put:
							break;
						case AreaResetType.Give:
							break;
						case AreaResetType.Equip:
							break;
						case AreaResetType.Door:
							break;
						case AreaResetType.Randomize:
							break;
					}
				}
			}

			// Update all areas
			foreach (var area in db.Areas)
			{
				db.Areas.Update(area);
			}
		}
	}
}
