using AbarimMUD.Data;
using AbarimMUD.Import;
using AbarimMUD.Storage;
using System;
using System.IO;
using System.Linq;

namespace AbarimMUD.ImportAre
{
	internal class Importer : BaseImporter
	{
		public ImporterSettings Settings { get; private set; }

		public Importer(ImporterSettings settings)
		{
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		private void ProcessArea(Stream stream, Area area)
		{
			var fileName = stream.ReadDikuString();
			area.Name = stream.ReadDikuString();
			area.Credits = stream.ReadDikuString();
			area.ParseLevelsBuilds();

			stream.ReadNumber(); // Start vnum
			stream.ReadNumber(); // End vnum

			Log($"Created area {area.Name}");
		}

		private void ProcessAreaData(Stream stream, Area area)
		{
			while (true)
			{
				var word = stream.EndOfStream() ? "End" : stream.ReadWord();

				switch (char.ToUpper(word[0]))
				{
					case 'C':
						area.Credits = stream.ReadDikuString();
						area.ParseLevelsBuilds();
						break;
					case 'N':
						area.Name = stream.ReadDikuString();
						break;
					case 'S':
						stream.ReadNumber(); // Security
						break;
					case 'V':
						if (word == "VNUMs")
						{
							stream.ReadNumber();
							stream.ReadNumber();
						}
						break;
					case 'E':
						if (word == "End")
						{
							goto finish;
						}
						break;
					case 'B':
						area.Builders = stream.ReadDikuString();
						break;
				}
			}

		finish:;
			Log($"Processed area data for {area.Name}");
		}

		private void ProcessMobiles(Stream stream, Area area)
		{
			while (!stream.EndOfStream())
			{
				var vnum = int.Parse(stream.ReadId());
				if (vnum == 0)
				{
					break;
				}

				var name = stream.ReadDikuString().Replace("oldstyle ", "");
				Log($"Processing mobile {name}...");

				var mobile = new Mobile
				{
					Name = name,
					ShortDescription = stream.ReadDikuString(),
					LongDescription = stream.ReadDikuString(),
					Description = stream.ReadDikuString(),
				};

				if (Settings.SourceType == SourceType.ROM)
				{
					mobile.Race = stream.ReadEnumFromDikuString<Race>();
				}

				var flags = (OldMobileFlags)stream.ReadFlag();
				var affectedByFlags = (OldAffectedByFlags)stream.ReadFlag();

				mobile.Alignment = stream.ReadNumber().ToAlignment();

				if (Settings.SourceType == SourceType.ROM)
				{
					var group = stream.ReadNumber();
				}
				else
				{
					var c = stream.ReadSpacedLetter();
				}
				mobile.Level = stream.ReadNumber();

				var hitRoll = stream.ReadNumber();

				int ac = 0;
				if (Settings.SourceType == SourceType.Envy)
				{
					ac = stream.ReadNumber();
				}

				var hitDice = stream.ReadDice();
				mobile.HitpointsRange = stream.ToRandomRange(hitDice);

				if (Settings.SourceType == SourceType.ROM)
				{
					var manaDice = stream.ReadDice();
					mobile.ManaRange = stream.ToRandomRange(manaDice);
				}

				var damageDice = stream.ReadDice();

				var attackType = AttackType.Hit;

				var offenseFlags = OldMobileOffensiveFlags.None;
				var immuneFlags = OldResistanceFlags.None;
				var resistanceFlags = OldResistanceFlags.None;
				var vulnerableFlags = OldResistanceFlags.None;
				var formsFlags = FormFlags.None;
				var partsFlags = PartFlags.None;
				if (Settings.SourceType == SourceType.ROM)
				{
					attackType = stream.ReadEnumFromWord<AttackType>();
					var acPierce = stream.ReadNumber();
					var acBash = stream.ReadNumber();
					var acSlash = stream.ReadNumber();
					var acExotic = stream.ReadNumber();
					ac = (acPierce + acBash + acSlash) / 3;
					offenseFlags = (OldMobileOffensiveFlags)stream.ReadFlag();

					immuneFlags = (OldResistanceFlags)stream.ReadFlag();
					resistanceFlags = (OldResistanceFlags)stream.ReadFlag();
					vulnerableFlags = (OldResistanceFlags)stream.ReadFlag();
					var startPosition = stream.ReadEnumFromWord<MobilePosition>();
					mobile.Position = stream.ReadEnumFromWord<MobilePosition>();
					mobile.Sex = stream.ReadEnumFromWord<Sex>();
					mobile.Wealth = stream.ReadNumber();
					formsFlags = (FormFlags)stream.ReadFlag();
					partsFlags = (PartFlags)stream.ReadFlag();

					mobile.Size = stream.ReadEnumFromWord<MobileSize>();
					var material = stream.ReadEnumFromWord<Material>();
				}
				else if (Settings.SourceType == SourceType.Envy)
				{
					mobile.Wealth = stream.ReadNumber();
					var xp = stream.ReadNumber();
					mobile.Position = stream.ReadEnumFromWord<MobilePosition>();
					mobile.Race = stream.ReadEnumFromDikuString<Race>();
					mobile.Sex = stream.ReadEnumFromWord<Sex>();
				}

				mobile.ArmorClass = -(ac - 10) * 10;

				if (mobile.ArmorClass < 0)
				{
					Log($"WARNING: {mobile.Name} has negative armor class. Clamped to zero");
				}

				var attacksCount = mobile.GetAttacksCount("");
				var accuracy = mobile.GetAccuracy("", hitRoll);
				for (var i = 0; i < attacksCount; ++i)
				{
					var range = stream.ToRandomRange(damageDice);
					var attack = new Attack(attackType, accuracy, range);
					mobile.Attacks.Add(attack);
				}

				area.Mobiles.Add(mobile);
				AddMobileToCache(vnum, mobile);

				// Add race flags
				var raceInfo = mobile.Race.GetRaceInfo();

				flags |= raceInfo.Flags;
				affectedByFlags |= raceInfo.AffectedByFlags;
				offenseFlags |= raceInfo.OffensiveFlags;
				immuneFlags |= raceInfo.ImmuneFlags;
				resistanceFlags |= raceInfo.ResistanceFlags;
				vulnerableFlags |= raceInfo.VulnerableFlags;
				formsFlags |= raceInfo.FormFlags;
				partsFlags |= raceInfo.PartFlags;

				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'F')
					{
						var word = stream.ReadWord();
						var vector = stream.ReadFlag();

						switch (word.Substring(0, 3).ToLower())
						{
							case "act":
								flags &= (OldMobileFlags)(~vector);
								break;
							case "aff":
								affectedByFlags &= (OldAffectedByFlags)(~vector);
								break;
							case "off":
								offenseFlags &= (OldMobileOffensiveFlags)(~vector);
								break;
							case "imm":
								immuneFlags &= (OldResistanceFlags)(~vector);
								break;
							case "res":
								resistanceFlags &= (OldResistanceFlags)(~vector);
								break;
							case "vul":
								vulnerableFlags &= (OldResistanceFlags)(~vector);
								break;
							case "for":
								formsFlags &= (FormFlags)(~vector);
								break;
							case "par":
								partsFlags &= (PartFlags)(~vector);
								break;
							default:
								stream.RaiseError($"Unknown flag {word}");
								break;
						}
					}
					else if (c == 'M')
					{
						var word = stream.ReadWord();
						var mnum = stream.ReadNumber();
						var trig = stream.ReadDikuString();

						Log("Warning: mob triggers are ignored.");
					}
					else
					{
						stream.GoBackIfNotEOF();
						break;
					}
				}

				// Set flags
				mobile.Flags = flags.ToNewFlags();
				var newOffensiveFlags = offenseFlags.ToNewFlags();
				foreach (var f in newOffensiveFlags)
				{
					mobile.Flags.Add(f);
				}

				mobile.AffectedByFlags = affectedByFlags.ToNewFlags();
				mobile.ImmuneFlags = immuneFlags.ToNewFlags();
				mobile.ResistanceFlags = resistanceFlags.ToNewFlags();
				mobile.VulnerableFlags = vulnerableFlags.ToNewFlags();
			}
		}

		private void ProcessObjects(Stream stream, Area area)
		{
			while (!stream.EndOfStream())
			{
				var vnum = int.Parse(stream.ReadId());
				if (vnum == 0)
				{
					break;
				}

				var name = stream.ReadDikuString();
				Log($"Processing object {name}...");

				var obj = new GameObject
				{
					Name = name,
					ShortDescription = stream.ReadDikuString(),
					Description = stream.ReadDikuString(),
					Material = stream.ReadEnumFromDikuStringWithDef(Material.Generic)
				};

				obj.ItemType = stream.ReadEnumFromWord<ItemType>();
				obj.ExtraFlags = (ItemExtraFlags)stream.ReadFlag();
				obj.WearFlags = (ItemWearFlags)stream.ReadFlag();

				area.Objects.Add(obj);
				AddObjectToCache(vnum, obj);

				if (Settings.SourceType == SourceType.ROM)
				{

					switch (obj.ItemType)
					{
						case ItemType.Weapon:
							obj.Value1 = (int)stream.ReadEnumFromWord<WeaponType>();
							obj.Value2 = stream.ReadNumber();
							obj.Value3 = stream.ReadNumber();
							obj.Value4 = (int)stream.ReadEnumFromWord<AttackType>();
							obj.Value5 = stream.ReadFlag();
							break;
						case ItemType.Container:
							obj.Value1 = stream.ReadNumber();
							obj.Value2 = stream.ReadFlag();
							obj.Value3 = stream.ReadNumber();
							obj.Value4 = stream.ReadNumber();
							obj.Value5 = stream.ReadNumber();
							break;
						case ItemType.Drink:
							obj.Value1 = stream.ReadNumber();
							obj.Value2 = stream.ReadNumber();
							obj.Value3 = (int)stream.ReadEnumFromWord<LiquidType>();
							obj.Value4 = stream.ReadNumber();
							obj.Value5 = stream.ReadNumber();
							break;
						case ItemType.Fountain:
							obj.Value1 = stream.ReadNumber();
							obj.Value2 = stream.ReadNumber();
							obj.Value3 = (int)stream.ReadEnumFromWord<LiquidType>();
							obj.Value4 = stream.ReadNumber();
							obj.Value5 = stream.ReadNumber();
							break;
						case ItemType.Wand:
							obj.Value1 = stream.ReadNumber();
							obj.Value2 = stream.ReadNumber();
							obj.Value3 = stream.ReadNumber();
							obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value5 = stream.ReadNumber();
							break;
						case ItemType.Staff:
							obj.Value1 = stream.ReadNumber();
							obj.Value2 = stream.ReadNumber();
							obj.Value3 = stream.ReadNumber();
							obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value5 = stream.ReadNumber();
							break;
						case ItemType.Potion:
							obj.Value1 = stream.ReadNumber();
							obj.Value2 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value3 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value5 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							break;
						case ItemType.Pill:
							obj.Value1 = stream.ReadNumber();
							obj.Value2 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value3 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value5 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							break;
						case ItemType.Scroll:
							obj.Value1 = stream.ReadNumber();
							obj.Value2 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value3 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							obj.Value5 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
							break;
						default:
							obj.Value1 = stream.ReadFlag();
							obj.Value2 = stream.ReadFlag();
							obj.Value3 = stream.ReadFlag();
							obj.Value4 = stream.ReadFlag();
							obj.Value5 = stream.ReadFlag();
							break;
					}

					obj.Level = stream.ReadNumber();
				}
				else
				{
					obj.S1 = stream.ReadDikuString();
					obj.S2 = stream.ReadDikuString();
					obj.S3 = stream.ReadDikuString();
					obj.S4 = stream.ReadDikuString();
				}

				obj.Weight = stream.ReadNumber();
				obj.Cost = stream.ReadNumber();

				if (Settings.SourceType == SourceType.Envy)
				{
					var costPerDay = stream.ReadNumber();
				}
				else
				{
					var letter = stream.ReadSpacedLetter();
					switch (letter)
					{
						case 'P':
							obj.Condition = 100;
							break;
						case 'G':
							obj.Condition = 90;
							break;
						case 'A':
							obj.Condition = 75;
							break;
						case 'W':
							obj.Condition = 50;
							break;
						case 'D':
							obj.Condition = 25;
							break;
						case 'B':
							obj.Condition = 10;
							break;
						case 'R':
							obj.Condition = 0;
							break;
						default:
							obj.Condition = 100;
							break;
					}
				}

				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'A')
					{
						var effect = new GameObjectEffect
						{
							EffectBitType = EffectBitType.Object,
							EffectType = (EffectType)stream.ReadNumber(),
							Modifier = stream.ReadNumber()
						};

						obj.Effects.Add(effect);
					}
					else if (c == 'F')
					{
						var effect = new GameObjectEffect();
						c = stream.ReadSpacedLetter();
						switch (c)
						{
							case 'A':
								effect.EffectBitType = EffectBitType.None;
								break;
							case 'I':
								effect.EffectBitType = EffectBitType.Immunity;
								break;
							case 'R':
								effect.EffectBitType = EffectBitType.Resistance;
								break;
							case 'V':
								effect.EffectBitType = EffectBitType.Vulnerability;
								break;
							default:
								stream.RaiseError($"Unable to parse effect bit '{c}'");
								break;
						}

						effect.EffectType = (EffectType)stream.ReadNumber();
						effect.Modifier = stream.ReadNumber();
						effect.Bits = (AffectedByFlags)stream.ReadFlag();
						obj.Effects.Add(effect);
					}
					else if (c == 'E')
					{
						obj.ExtraKeyword = stream.ReadDikuString();
						obj.ExtraDescription = stream.ReadDikuString();
					}
					else if (c == 'L' || c == 'C')
					{
						var n = stream.ReadFlag();
					}
					else if (c == 'R' || c == 'D' || c == 'O' || c == 'X' || c == 'M' ||
						c == 'Y' || c == 'J' || c == 'G' || c == 'K' || c == 'V' || c == 'P' || c == 'd')
					{
						var k = 5;
					}
					else
					{
						stream.GoBackIfNotEOF();
						break;
					}
				}
			}
		}

		private void ProcessRooms(Stream stream, Area area)
		{
			while (!stream.EndOfStream())
			{
				var vnum = int.Parse(stream.ReadId());
				if (vnum == 0)
				{
					break;
				}

				var name = stream.ReadDikuString();
				Log($"Processing room {name} (# {vnum})...");

				var room = new Room
				{
					Name = name,
					Description = stream.ReadDikuString(),
				};

				stream.ReadNumber(); // Area Number

				room.Flags = ((OldRoomFlags)stream.ReadFlag()).ToNewFlags();
				room.SectorType = stream.ReadEnumFromWord<SectorType>();

				// Save room to set its id
				area.Rooms.Add(room);
				AddRoomToCache(vnum, room);

				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'S')
					{
						break;
					}
					else if (c == 'H')
					{
						room.HealRate = stream.ReadNumber();
					}
					else if (c == 'M')
					{
						room.ManaRate = stream.ReadNumber();
					}
					else if (c == 'C')
					{
						string clan = stream.ReadDikuString();
					}
					else if (c == 'D')
					{
						var exit = new RoomExit
						{
							Direction = (Direction)stream.ReadNumber(),
							Description = stream.ReadDikuString(),
							Keyword = stream.ReadDikuString(),
						};

						var locks = stream.ReadNumber();

						var exitFlags = OldRoomExitFlags.None;
						switch (locks)
						{
							case 1:
								exitFlags = OldRoomExitFlags.Door;
								break;
							case 2:
								exitFlags = OldRoomExitFlags.Door | OldRoomExitFlags.PickProof;
								break;
							case 3:
								exitFlags = OldRoomExitFlags.Door | OldRoomExitFlags.NoPass;
								break;
							case 4:
								exitFlags = OldRoomExitFlags.Door | OldRoomExitFlags.PickProof | OldRoomExitFlags.NoPass;
								break;
							default:
								break;
						}

						exit.Flags = exitFlags.ToNewFlags();

						var exitInfo = new RoomExitInfo
						{
							SourceRoom = room,
							RoomExit = exit
						};

						var keyVNum = stream.ReadNumber();
						if (exitFlags != OldRoomExitFlags.None && keyVNum != -1)
						{
							exitInfo.KeyObjectVNum = keyVNum;
						}

						var targetVnum = stream.ReadNumber();
						if (targetVnum != -1)
						{
							exitInfo.TargetRoomVNum = targetVnum;
						}

						AddRoomExitToCache(exitInfo);
					}
					else if (c == 'E')
					{
						room.ExtraKeyword = stream.ReadDikuString();
						room.ExtraDescription = stream.ReadDikuString();
					}
					else if (c == 'O')
					{
						room.Owner = stream.ReadDikuString();
					}
					else
					{
						throw new Exception($"Unknown room command '{c}'");
					}
				}
			}
		}

		private void ProcessResets(Stream stream, Area area)
		{
			while (!stream.EndOfStream())
			{
				var c = stream.ReadSpacedLetter();
				if (c == 'S')
				{
					break;
				}

				if (c == '*')
				{
					stream.ReadLine();
					continue;
				}

				var reset = new OldAreaReset();
				switch (c)
				{
					case 'M':
						reset.ResetType = AreaResetType.NPC;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();

						if (Settings.SourceType == SourceType.ROM)
						{
							reset.Value5 = stream.ReadNumber();
						}
						break;

					case 'O':
						reset.ResetType = AreaResetType.Item;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();
						break;

					case 'P':
						reset.ResetType = AreaResetType.Put;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();

						if (Settings.SourceType == SourceType.ROM)
						{
							reset.Value5 = stream.ReadNumber();
						}
						break;

					case 'G':
						reset.ResetType = AreaResetType.Give;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						break;

					case 'E':
						reset.ResetType = AreaResetType.Equip;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();
						break;

					case 'D':
						reset.ResetType = AreaResetType.Door;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();

						break;

					case 'R':
						reset.ResetType = AreaResetType.Randomize;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();

						break;
				}

				stream.ReadLine();
				area.Resets.Add(reset.ToNewAreaReset());
			}
		}

		private void ProcessShops(Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var keeperVnum = stream.ReadNumber();
				if (keeperVnum == 0)
				{
					break;
				}

				var shop = new Shop
				{
					BuyType1 = stream.ReadNumber(),
					BuyType2 = stream.ReadNumber(),
					BuyType3 = stream.ReadNumber(),
					BuyType4 = stream.ReadNumber(),
					BuyType5 = stream.ReadNumber(),
					ProfitBuy = stream.ReadNumber(),
					ProfitSell = stream.ReadNumber(),
					OpenHour = stream.ReadNumber(),
					CloseHour = stream.ReadNumber(),
				};

				AddShopToCache(keeperVnum, shop);

				stream.ReadLine();

				Log($"Added shop for mobile {keeperVnum}");
			}
		}

		private void ProcessSpecials(Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var c = stream.ReadSpacedLetter();
				switch (c)
				{
					case 'S':
						return;

					case '*':
						break;

					case 'M':
						var mobVnum = stream.ReadNumber();
						var mobile = GetMobileByVnum(mobVnum);
						if (mobile == null)
						{
							throw new Exception($"Could not find mobile with vnum {mobVnum}");
						}

						var special = new MobileSpecialAttack
						{
							AttackType = stream.ReadWord()
						};

						mobile.SpecialAttacks.Add(special);
						break;
				}

				stream.ReadLine();
			}
		}

		private void ProcessSocials(Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var temp = stream.ReadWord();
				if (temp == "#0")
				{
					break;
				}

				var social = new Social
				{
					Name = temp,
				};

				Log($"Processing social {temp}...");

				do
				{
					string s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.CharNoArgument = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.OthersNoArgument = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.CharFound = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.OthersFound = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.VictimFound = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.CharAuto = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.CharAuto = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.OthersAuto = s;
				}
				while (false);

				social.Create();
			}

			Social.SaveSocials();
		}

		private void SkipSection(Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var c = stream.ReadLetter();
				if (c == '$')
				{
					// Skip ~
					stream.ReadLetter();
					break;
				}
			}
		}

		private void ProcessFile(string areaFile)
		{
			Log($"Processing {areaFile}...");

			var jsonFileName = Path.ChangeExtension(Path.GetFileName(areaFile), "json");

			Area area = null;
			using (var stream = File.OpenRead(areaFile))
			{
				while (!stream.EndOfStream())
				{
					var type = stream.ReadId();

					if (type.StartsWith("AREA") && type.EndsWith("~"))
					{
						var credits = type.Substring(4).RemoveTrailingTilda().Trim();
						area = new Area
						{
							Filename = jsonFileName,
							Credits = credits
						};

						if (!area.ParseLevelsBuilds())
						{
							Log($"WARNING: Couldn't parse levels/builders info from '{credits}'");
						} else
						{
							// Area name should be after buildes
							var i = credits.IndexOf(area.Builders);
							credits = credits.Substring(i + area.Builders.Length + 1);
						}

						area.Name = credits.Trim();

						continue;
					}
					else if (type.StartsWith("RECALL"))
					{
						Log($"Skipping '{type}'");
						continue;
					}
					else if (type.StartsWith("VERSION"))
					{
						var data = type.Substring(8);
						if (string.IsNullOrWhiteSpace(data))
						{
							stream.RaiseError($"No area version specified");
						}

						area.Version = int.Parse(data.Trim());
						Log($"Area version: {area.Version}");
						continue;
					}
					else if(type.StartsWith("AUTHOR"))
					{
						var data = type.Substring(7);
						if (string.IsNullOrWhiteSpace(data))
						{
							stream.RaiseError($"No area author specified");
						}

						area.Credits = data.RemoveTrailingTilda().Trim();
						Log($"Area Author: {area.Credits}");
						continue;
					}
					else if(type.StartsWith("RESETMSG"))
					{
						var data = type.Substring(9);
						if (string.IsNullOrWhiteSpace(data))
						{
							stream.RaiseError($"No area author specified");
						}

						area.ResetMessage = data.RemoveTrailingTilda().Trim();
						Log($"Area Reset Message: {area.ResetMessage}");
						continue;
					}

					switch (type)
					{
						case "AREA":
							area = new Area
							{
								Filename = jsonFileName
							};
							ProcessArea(stream, area);
							break;
						case "AREADATA":
							area = new Area
							{
								Filename = jsonFileName
							};
							ProcessAreaData(stream, area);
							break;
						case "MOBILES":
							ProcessMobiles(stream, area);
							break;
						case "OBJECTS":
							ProcessObjects(stream, area);
							break;
						case "ROOMS":
							ProcessRooms(stream, area);
							break;
						case "RESETS":
							ProcessResets(stream, area);
							break;
						case "SHOPS":
							ProcessShops(stream);
							break;
						case "SPECIALS":
							ProcessSpecials(stream);
							break;
						case "SOCIALS":
							ProcessSocials(stream);
							goto finish;
						case "RANGES":
							{
								var r1 = stream.ReadNumber();
								var r2 = stream.ReadNumber();
								var r3 = stream.ReadNumber();
								var r4 = stream.ReadNumber();

								Log($"Parsed levels range: {r1} {r2} {r3} {r4}");
								area.MinimumLevel = r1.ToString();
								area.MaximumLevel = r2.ToString();

								// Skip $
								var c = stream.ReadSpacedLetter();
							}
							break;
						case "$":
							if (area != null)
							{
								area.Create();
							}
							goto finish;
						default:
							Log($"Skipping section {type}");
							SkipSection(stream);
							break;
					}
				}

			finish:;
			}
		}

		public void Process()
		{
			var areasFolder = Path.Combine(Settings.OutputFolder, "areas");
			if (Directory.Exists(areasFolder))
			{
				Directory.Delete(areasFolder, true);
			}

			InitializeDb(Settings.OutputFolder);

			var areaFiles = Directory.EnumerateFiles(Settings.InputFolder, "*.are", SearchOption.AllDirectories).ToArray();
			foreach (var areaFile in areaFiles)
			{
				var fn = Path.GetFileName(areaFile);
				if (fn == "proto.are")
				{
					Log($"Skipping prototype area {areaFile}");
					continue;
				}

				ProcessFile(areaFile);
			}

			// Set ids
			SetIdsInCache();

			// Process shops
			UpdateShops();

			// Process directions
			UpdateRoomExitsReferences();

			// Update resets
			UpdateResets();

			// Update all areas
			UpdateAllAreas();


			/*						Log("Locking doors");
									using (var db = new DataContext())
									{
										var areas = (from a in db.Areas select a).ToArray();
										foreach (var area in areas)
										{
											var resets = (from r in db.AreaResets where r.Area == area select r).ToArray();
											foreach (var reset in resets)
											{
												if (reset.ResetType != AreaResetType.Door)
												{
													continue;
												}

												if (reset.Value3 < 0 || reset.Value3 >= 6)
												{
													throw new Exception($"Reset {reset.Id}. Room direction with value {reset.Value3} is outside of range.");
												}

												var dir = (Direction)reset.Value3;

												var roomVnum = reset.Value2;

												var room = (from r in db.Rooms where r.Id == roomVnum select r).FirstOrDefault();
												if (room == null)
												{
													throw new Exception($"Reset {reset.Id}. Can't find room with vnum {roomVnum}");
												}

												var exit = (from e in db.RoomsExits where e.SourceRoomId == room.Id && 
															e.Direction == (Direction)reset.Value3 
															select e).FirstOrDefault();
												if (exit == null || !exit.Flags.HasFlag(RoomExitFlags.Door))
												{
													throw new Exception($"Reset {reset.Id}. Can't find exit {dir}");
												}

												switch (reset.Value4)
												{
													case 0:
														break;
													case 1:
														exit.Flags |= RoomExitFlags.Closed;
														db.SaveChanges();
														break;
													case 2:
														exit.Flags |= RoomExitFlags.Closed | RoomExitFlags.Locked;
														db.SaveChanges();
														break;
													default:
														throw new Exception($"Reset {reset.Id}. Bad locks {reset.Value4}");

												}
											}
										}
									}*/

			Log("Success");
		}
	}
}