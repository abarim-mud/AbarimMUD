using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	public class Areas : MultipleFilesStorageString<Area>
	{
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

		internal const string SubfolderName = "areas";

		private bool _roomsDirty = true;
		private int _nextRoomId = 1;
		private readonly Dictionary<int, Room> _allRoomsCache = new Dictionary<int, Room>();

		public int NewRoomId
		{
			get
			{
				var result = _nextRoomId;
				++_nextRoomId;
				return result;
			}
		}

		public IReadOnlyDictionary<int, Room> AllRooms => _allRoomsCache;

		internal Areas() : base(a => a.Name, SubfolderName)
		{
		}

		public override Area LoadEntity(string filePath)
		{
			var area = base.LoadEntity(filePath);

			area.RoomsChanged += Area_RoomsChanged;

			_roomsDirty = true;

			return area;
		}

		private void Area_RoomsChanged(object sender, EventArgs e)
		{
			_roomsDirty = true;
		}

		protected override void ClearCache()
		{
			foreach (var area in this)
			{
				area.RoomsChanged -= Area_RoomsChanged;
			}

			base.ClearCache();
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
			}

			UpdateAllRooms();
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();
			result.Converters.Add(new RoomExitConverter());

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
			var maxRoomId = 0;
			foreach (var area in All)
			{
				foreach (var room in area.Rooms)
				{
					if (room.Id > maxRoomId)
					{
						maxRoomId = room.Id;
					}

					_allRoomsCache[room.Id] = room;
				}
			}

			_nextRoomId = maxRoomId + 1;
			_roomsDirty = false;
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
	}
}