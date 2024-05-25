using AbarimMUD.Data;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal class Areas : CRUD<Area>
	{
		private class RoomReference
		{
			public string AreaId { get; private set; }
			public int RoomId { get; private set; }

			public RoomReference(string areaId, int roomId)
			{
				AreaId = areaId;
				RoomId = roomId;
			}
		}

		private class RoomExitConverter : JsonConverter<RoomExit>
		{
			public Area Area { get; set; }

			public override RoomExit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				RoomReference roomReference = null;
				var r = reader.GetString();
				if (r.Contains("/"))
				{
					var parts = r.Split('/');
					roomReference = new RoomReference(parts[0], int.Parse(parts[1]));
				}
				else
				{
					roomReference = new RoomReference(null, int.Parse(r));
				}

				return new RoomExit
				{
					Tag = roomReference
				};
			}

			public override void Write(Utf8JsonWriter writer, RoomExit value, JsonSerializerOptions options)
			{
				if (value.TargetRoom != null)
				{
					if (value.TargetRoom.Area == Area)
					{
						// Write just id
						writer.WriteStringValue(value.TargetRoom.Id.ToString());
					}
					else
					{
						// Write area and id
						writer.WriteStringValue($"{value.TargetRoom.Area.Id}/{value.TargetRoom.Id}");
					}
				}
				else
				{
					writer.WriteNullValue();
				}
			}
		}

		internal const string SubfolderName = "areas";

		private string Folder => Path.Combine(BaseFolder, SubfolderName);

		public Areas(DataContextSettings context) : base(context)
		{
			Load();
		}


		private void Load()
		{
			var areasFolder = Folder;
			if (!Directory.Exists(areasFolder))
			{
				return;
			}

			var files = Directory.GetFiles(areasFolder, "*.json");
			foreach (var path in files)
			{
				Log($"Loading {path}");

				var data = File.ReadAllText(path);
				var options = Utility.CreateDefaultOptions();
				options.Converters.Add(new RoomExitConverter());
				var area = JsonSerializer.Deserialize<Area>(data, options);
				AddToCache(area);
			}
		}

		internal override void SetReferences(DataContext db)
		{
			base.SetReferences(db);

			foreach (var pair in _cache)
			{
				var area = pair.Value;
				foreach (var room in area.Rooms)
				{
					foreach (var pair2 in room.Exits)
					{
						var exit = pair2.Value;
						if (exit == null)
						{
							continue;
						}

						exit.Direction = pair2.Key;

						var roomReference = (RoomReference)exit.Tag;
						if (string.IsNullOrEmpty(roomReference.AreaId))
						{
							// Same area
							exit.TargetRoom = area.Rooms[roomReference.RoomId];
						} else
						{
							// Different area
							exit.TargetRoom = EnsureById(roomReference.AreaId).Rooms[roomReference.RoomId];
						}

						exit.Tag = null;
					}
				}
			}
		}

		internal override void Save(Area area)
		{
			var areasFolder = Folder;
			if (!Directory.Exists(areasFolder))
			{
				Directory.CreateDirectory(areasFolder);
			}

			var options = Utility.CreateDefaultOptions();
			var converter = new RoomExitConverter
			{
				Area = area
			};
			options.Converters.Add(converter);
			var data = JsonSerializer.Serialize(area, options);

			var accountPath = Path.Combine(areasFolder, $"{area.Id}.json");
			File.WriteAllText(accountPath, data);
		}
	}
}
