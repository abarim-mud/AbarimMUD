using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ur;

namespace AbarimMUD.Storage
{
	public class Areas : MultipleFilesStorage<Area>
	{
		private class EntityCache<T> where T : class, IAreaEntity
		{
			private readonly Areas _areas;
			private readonly Func<Area, IEnumerable<T>> _getter;
			private readonly Dictionary<int, T> _cache = new Dictionary<int, T>();
			private bool _dirty = true;
			private int _nextId = 1;

			public int NextId
			{
				get
				{
					Update();
					var result = _nextId;
					++_nextId;
					return result;
				}
			}

			public IReadOnlyDictionary<int, T> All
			{
				get
				{
					Update();

					return _cache;
				}
			}

			public EntityCache(Areas areas, Func<Area, IEnumerable<T>> getter)
			{
				_areas = areas ?? throw new ArgumentNullException(nameof(areas));
				_getter = getter ?? throw new ArgumentNullException(nameof(getter));
			}

			public void Invalidate()
			{
				_dirty = true;
			}

			private void Update()
			{
				if (!_dirty)
				{
					return;
				}

				_cache.Clear();
				var maxId = 0;
				foreach (var area in _areas)
				{
					var entities = _getter(area);
					foreach (var entity in entities)
					{
						if (entity.Id > maxId)
						{
							maxId = entity.Id;
						}

						_cache[entity.Id] = entity;
					}
				}

				_nextId = maxId + 1;
				_dirty = false;
			}

			public T GetById(int id)
			{
				Update();

				T result;
				if (!_cache.TryGetValue(id, out result))
				{
					return null;
				}

				return result;
			}

			public T EnsureById(int id)
			{
				var result = GetById(id);
				if (result == null)
				{
					throw new Exception($"Could not find {typeof(T).Name.ToLower()} with vnum {id}");
				}

				return result;
			}
		}

		internal const string SubfolderName = "areas";

		private readonly EntityCache<Room> _allRoomsCache;
		private readonly EntityCache<Mobile> _allMobilesCache;

		public IReadOnlyDictionary<int, Room> AllRooms => _allRoomsCache.All;
		public IReadOnlyDictionary<int, Mobile> AllMobiles => _allMobilesCache.All;

		internal Areas() : base(SubfolderName)
		{
			_allRoomsCache = new EntityCache<Room>(this, a => a.Rooms);
			_allMobilesCache = new EntityCache<Mobile>(this, a => a.Mobiles);
		}

		public override Area LoadEntity(string filePath)
		{
			var area = base.LoadEntity(filePath);

			area.RoomsChanged += Area_RoomsChanged;
			area.MobilesChanged += Area_MobilesChanged;

			_allRoomsCache.Invalidate();
			_allMobilesCache.Invalidate();

			return area;
		}

		private void Area_MobilesChanged(object sender, EventArgs e)
		{
			_allMobilesCache.Invalidate();
		}

		private void Area_RoomsChanged(object sender, EventArgs e)
		{
			_allRoomsCache.Invalidate();
		}

		protected override void AddToCache(Area entity)
		{
			base.AddToCache(entity);

			_allMobilesCache.Invalidate();
			_allRoomsCache.Invalidate();
		}

		public override void ClearCache()
		{
			foreach (var area in this)
			{
				area.RoomsChanged -= Area_RoomsChanged;
				area.MobilesChanged -= Area_MobilesChanged;
			}

			_allMobilesCache.Invalidate();
			_allRoomsCache.Invalidate();

			base.ClearCache();
		}

		protected override void SetReferences()
		{
			base.SetReferences();

			foreach (var area in this)
			{
				if (!string.IsNullOrEmpty(area.OwnerName))
				{
					area.Owner = Character.GetCharacterByName(area.OwnerName);
				}

				// Rooms
				foreach (var room in area.Rooms)
				{
					var toDelete = new HashSet<Direction>();
					foreach (var pair2 in room.Exits)
					{
						var exit = pair2.Value;

						exit.Direction = pair2.Key;

						var vnum = exit.TargetRoom.Id;
						var targetRoom = GetRoomById(vnum);
						if (targetRoom == null)
						{
							switch (Configuration.RoomExitNotExistantBehavior)
							{
								case RoomExitNotExistantBehavior.ThrowException:
									throw new Exception($"Unable to find room with id {vnum}");
								case RoomExitNotExistantBehavior.DeleteRoomExit:
									toDelete.Add(pair2.Key);
									Log($"Unable to find room with id {vnum}. Deleting room exit {pair2.Key} for room {room}");
									break;
								case RoomExitNotExistantBehavior.SetNull:
									break;
							}
						}

						exit.TargetRoom = targetRoom;
					}

					foreach (var d in toDelete)
					{
						room.Exits.Remove(d);
					}
				}

				// Mobiles
				for (var i = 0; i < area.Mobiles.Count; ++i)
				{
					var mobile = area.Mobiles[i];
					mobile.SetReferences();
				}

				// Spawns
				foreach (var room in area.Rooms)
				{
					foreach (var mobileSpawn in room.MobileSpawns)
					{
						mobileSpawn.Mobile = Mobile.EnsureMobileById(mobileSpawn.Mobile.Id);
					}
				}
			}
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();
			result.Converters.Add(Common.RoomExitConverter);
			result.Converters.Add(Common.MobileSpawnConverter);
			result.Converters.Add(Common.ItemInstanceConverter);
			result.Converters.Add(Common.InventoryConverter);

			return result;
		}

		public Room GetRoomById(int id) => _allRoomsCache.GetById(id);
		public Room EnsureRoomById(int id) => _allRoomsCache.EnsureById(id);
		public Mobile GetMobileById(int id) => _allMobilesCache.GetById(id);
		public Mobile EnsureMobileById(int id) => _allMobilesCache.GetById(id);
	}
}