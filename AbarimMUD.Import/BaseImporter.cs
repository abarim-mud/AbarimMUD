using AbarimMUD.Data;
using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AbarimMUD.Import
{
	public class BaseImporter
	{
		private readonly Dictionary<int, Room> _roomsByVnums = new Dictionary<int, Room>();
		private readonly Dictionary<int, Mobile> _mobilesByVnums = new Dictionary<int, Mobile>();
		private readonly Dictionary<int, GameObject> _objectsByVnums = new Dictionary<int, GameObject>();
		private readonly Dictionary<int, Shop> _shopsByKeepersVnums = new Dictionary<int, Shop>();

		private readonly List<RoomExitInfo> _tempDirections = new List<RoomExitInfo>();

		public static void Log(string message) => ImportUtility.Log(message);

		public Room GetRoomByVnum(int vnum) =>
			(from m in _roomsByVnums where m.Key == vnum select m.Value).FirstOrDefault();

		public Room EnsureRoomByVnum(int vnum) =>
			(from m in _roomsByVnums where m.Key == vnum select m.Value).First();

		public Mobile GetMobileByVnum(int vnum) =>
			(from m in _mobilesByVnums where m.Key == vnum select m.Value).FirstOrDefault();

		public Mobile EnsureMobileByVnum(int vnum) =>
			(from m in _mobilesByVnums where m.Key == vnum select m.Value).First();

		public GameObject GetObjectByVnum(int vnum) =>
			(from m in _objectsByVnums where m.Key == vnum select m.Value).FirstOrDefault();

		public void AddRoomToCache(int vnum, Room room) => _roomsByVnums[vnum] = room;
		public void AddMobileToCache(int vnum, Mobile mobile) => _mobilesByVnums[vnum] = mobile;
		public void AddObjectToCache(int vnum, GameObject obj) => _objectsByVnums[vnum] = obj;
		public void AddRoomExitToCache(RoomExitInfo exitInfo) => _tempDirections.Add(exitInfo);
		public void AddShopToCache(int keeperVNum, Shop shop) => _shopsByKeepersVnums[keeperVNum] = shop;

		public void InitializeDb(string folder)
		{
			DataContext.Initialize(folder, Log);

			DataContext.Register(Area.Storage);
			DataContext.Register(Account.Storage);
			DataContext.Register(Character.Storage);
			DataContext.Register(Social.Storage);

			DataContext.Load();
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

		public void SetIdsInCache()
		{
			SetIds(_roomsByVnums);
			SetIds(_mobilesByVnums);
			SetIds(_objectsByVnums);
		}

		public void UpdateShops()
		{
			Log("Updating shops");
			foreach (var pair in _shopsByKeepersVnums)
			{
				var keeper = GetMobileByVnum(pair.Key);
				if (keeper == null)
				{
					throw new Exception($"Could not find shop keeper with vnum {pair.Key}");
				}

				keeper.Shop = pair.Value;
			}
		}

		public void UpdateRoomExitsReferences()
		{
			Log("Updating rooms exits references");
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
		}

		public void UpdateResets()
		{
			Log("Updating resets");
			foreach (var area in Area.Storage)
			{
				var toDelete = new List<AreaReset>();
				for (var i = 0; i < area.Resets.Count; ++i)
				{
					var reset = area.Resets[i];
					switch (reset.ResetType)
					{
						case AreaResetType.NPC:
							var room = GetRoomByVnum(reset.Id1);
							if (room == null)
							{
								Log($"WARNING: Unable to find room with vnum {reset.Id2} for #{i} reset of area {area.Name}");
								toDelete.Add(reset);
								break;
							}

							var mobile = GetMobileByVnum(reset.Id2);
							if (mobile == null)
							{
								Log($"WARNING: Unable to find mobile with vnum {reset.Id1} for #{i} reset of area {area.Name}");
								toDelete.Add(reset);
								break;
							}
							reset.Id1 = mobile.Id;
							reset.Id2 = room.Id;
							break;
						case AreaResetType.Item:
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

				foreach (var reset in toDelete)
				{
					area.Resets.Remove(reset);
				}
			}
		}

		public void UpdateAllAreas()
		{
			Log("Updating all areas");
			Area.Storage.SaveAll();
		}
	}
}
