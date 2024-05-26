using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	public class Areas : CRUD<Area>
	{
		private bool _roomsDirty = true, _mobilesDirty = true, _objectsDirty = true;
		private int _maxRoomsId = int.MinValue, _maxMobilesId = int.MinValue, _maxObjectsId = int.MinValue;
		private readonly Dictionary<int, Room> _allRoomsCache = new Dictionary<int, Room>();
		private readonly Dictionary<int, Mobile> _allMobilesCache = new Dictionary<int, Mobile>();
		private readonly Dictionary<int, GameObject> _allObjectsCache = new Dictionary<int, GameObject>();

		private class RoomExitConverter : JsonConverter<RoomExit>
		{
			public Area Area { get; set; }

			public override RoomExit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var vnum = reader.GetInt32();
				return new RoomExit
				{
					Tag = vnum
				};
			}

			public override void Write(Utf8JsonWriter writer, RoomExit value, JsonSerializerOptions options)
			{
				if (value.TargetRoom != null)
				{
					writer.WriteNumberValue(value.TargetRoom.Id);
				}
				else
				{
					writer.WriteNullValue();
				}
			}
		}

		internal const string SubfolderName = "areas";

		public int NewRoomId
		{
			get
			{
				UpdateAllRooms();
				return _maxRoomsId + 1;
			}
		}

		public int NewMobileId
		{
			get
			{
				UpdateAllMobiles();
				return _maxMobilesId + 1;
			}
		}

		public int NewObjectId
		{
			get
			{
				UpdateAllObjects();
				return _maxObjectsId + 1;
			}
		}

		private string Folder => Path.Combine(BaseFolder, SubfolderName);

		internal Areas(DataContextSettings context) : base(context)
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

				area.RoomsChanged += (s, a) => _roomsDirty = true;
				area.MobilesChanged += (s, a) => _mobilesDirty = true;
				area.ObjectsChanged += (s, a) => _objectsDirty = true;
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

						var vnum = (int)exit.Tag;
						exit.TargetRoom = GetRoomById(vnum);
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

		private void UpdateAllRooms()
		{
			if (!_roomsDirty)
			{
				return;
			}

			_allRoomsCache.Clear();
			_maxRoomsId = int.MinValue;

			foreach (var area in All)
			{
				foreach (var room in area.Rooms)
				{
					if (room.Id > _maxRoomsId)
					{
						_maxRoomsId = room.Id;
					}

					_allRoomsCache[room.Id] = room;
				}
			}

			_roomsDirty = false;
		}

		private void UpdateAllMobiles()
		{
			if (!_mobilesDirty)
			{
				return;
			}

			_allMobilesCache.Clear();
			_maxMobilesId = int.MinValue;

			foreach (var area in All)
			{
				foreach (var mobile in area.Mobiles)
				{
					if (mobile.Id > _maxMobilesId)
					{
						_maxMobilesId = mobile.Id;
					}

					_allMobilesCache[mobile.Id] = mobile;
				}
			}

			_mobilesDirty = false;
		}

		private void UpdateAllObjects()
		{
			if (!_objectsDirty)
			{
				return;
			}

			_allObjectsCache.Clear();
			_maxObjectsId = int.MinValue;

			foreach (var area in All)
			{
				foreach (var obj in area.Objects)
				{
					if (obj.Id > _maxObjectsId)
					{
						_maxObjectsId = obj.Id;
					}

					_allObjectsCache[obj.Id] = obj;
				}
			}

			_objectsDirty = false;
		}


		public Room GetRoomById(int id)
		{
			UpdateAllRooms();

			Room result;
			if (!_allRoomsCache.TryGetValue(id, out result))
			{
				return null;
			}

			return result;
		}

		public Room EnsureRoomById(int id)
		{
			var result = GetRoomById(id);
			if (result == null)
			{
				throw new Exception($"Could not find room with vnum {id}");
			}

			return result;
		}

		public Mobile GetMobileById(int id)
		{
			UpdateAllMobiles();

			Mobile result;
			if (!_allMobilesCache.TryGetValue(id, out result))
			{
				return null;
			}

			return result;
		}

		public Mobile EnsureMobileById(int id)
		{
			var result = GetMobileById(id);
			if (result == null)
			{
				throw new Exception($"Could not find mobile with vnum {id}");
			}

			return result;
		}

		public GameObject GetObjectById(int id)
		{
			UpdateAllObjects();

			GameObject result;
			if (!_allObjectsCache.TryGetValue(id, out result))
			{
				return null;
			}

			return result;
		}

		public GameObject EnsureObjectById(int id)
		{
			var result = GetObjectById(id);
			if (result == null)
			{
				throw new Exception($"Could not find object with vnum {id}");
			}

			return result;
		}
	}
}