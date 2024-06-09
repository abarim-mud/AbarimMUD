using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	public class Areas : MultipleFilesStorageString<Area>
	{
		private const string AreasFolder = "areas";

		private bool _roomsDirty = true, _mobilesDirty = true, _objectsDirty = true;
		private int _nextRoomId = 0, _nextMobileId = 0, _nextObjectId = 0;
		private readonly Dictionary<int, Room> _allRoomsCache = new Dictionary<int, Room>();
		private readonly Dictionary<int, Mobile> _allMobilesCache = new Dictionary<int, Mobile>();
		private readonly Dictionary<int, GameObject> _allObjectsCache = new Dictionary<int, GameObject>();

		private class RoomExitConverter : JsonConverter<RoomExit>
		{
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

		private class RaceConverter : JsonConverter<Race>
		{
			public override Race Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var name = reader.GetString();
				return new Race
				{
					Name = name
				};
			}

			public override void Write(Utf8JsonWriter writer, Race value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(value.Name);
			}
		}

		internal const string SubfolderName = "areas";

		public int NewRoomId
		{
			get
			{
				var result = _nextRoomId;
				++_nextRoomId;
				return result;
			}
		}

		public int NewMobileId
		{
			get
			{
				var result = _nextMobileId;
				++_nextMobileId;
				return result;
			}
		}

		public int NewObjectId
		{
			get
			{
				var result = _nextObjectId;
				++_nextObjectId;
				return result;
			}
		}

		internal Areas() : base(a => a.Name, AreasFolder)
		{
		}

		public override Area LoadEntity(string filePath)
		{
			var area = base.LoadEntity(filePath);

			area.RoomsChanged += (s, a) => _roomsDirty = true;
			area.MobilesChanged += (s, a) => _mobilesDirty = true;
			area.ObjectsChanged += (s, a) => _objectsDirty = true;

			return area;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var area in this)
			{
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

				foreach (var mobile in area.Mobiles)
				{
					mobile.Race = Race.EnsureRaceByName(mobile.Race.Name);
				}
			}
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();
			result.Converters.Add(new RoomExitConverter());
			result.Converters.Add(new RaceConverter());

			return result;
		}

		protected override void InternalLoad()
		{
			base.InternalLoad();
		}

		private void UpdateAllRooms()
		{
			if (!_roomsDirty)
			{
				return;
			}

			_allRoomsCache.Clear();
			_nextRoomId = int.MinValue;

			foreach (var area in All)
			{
				foreach (var room in area.Rooms)
				{
					if (room.Id > _nextRoomId)
					{
						_nextRoomId = room.Id;
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
			_nextMobileId = int.MinValue;

			foreach (var area in All)
			{
				foreach (var mobile in area.Mobiles)
				{
					if (mobile.Id > _nextMobileId)
					{
						_nextMobileId = mobile.Id;
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
			_nextObjectId = int.MinValue;

			foreach (var area in All)
			{
				foreach (var obj in area.Objects)
				{
					if (obj.Id > _nextObjectId)
					{
						_nextObjectId = obj.Id;
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