using AbarimMUD.Data;
using MUDMapBuilder;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static MUDMapBuilder.MMBProject;

namespace AbarimMUD.Common.Tools;

public static class ToolsUtility
{
	public static MMBDirection ToMMBDirection(this Direction dir) => (MMBDirection)dir;

	public static MMBRoom ToMMBRoom(this Room room, Area area)
	{
		var result = new MMBRoom(room.Id, $"{room.Name} #{room.Id}");

		// Check if the room is special
		var isSpecial = Configuration.StartRoomId == room.Id ||
			(room.Mobiles != null && (from m in room.Mobiles
									  where
			 m.Info.Guildmaster != null || m.Info.Shop != null ||
			 m.Info.ExchangeShop != null || m.Info.Flags.Contains(MobileFlags.Enchanter)
									  select m).FirstOrDefault() != null);

		if (isSpecial)
		{
			result.Color = result.FrameColor = Color.Brown;
		}

		foreach (var pair in room.Exits)
		{
			var exit = pair.Value;
			if (exit == null || exit.TargetRoom == null)
			{
				continue;
			}

			var conn = new MMBRoomConnection(pair.Key.ToMMBDirection(), exit.TargetRoom.Id);

			/*				foreach (var reset in area.Resets)
							{
								if (reset.ResetType != AreaResetType.Door || reset.Value2 != room.Id || reset.Value3 != (int)pair.Key || reset.Value4 != 2)
								{
									continue;
								}

								conn.IsDoor = true;

								// Add locked door
								if (conn.DoorSigns == null)
								{
									conn.DoorSigns = new List<MMBRoomContentRecord>();
								}

								if (!exit.Flags.Contains(RoomExitFlags.PickProof))
								{
									conn.DoorColor = Color.CornflowerBlue;
								}
								else
								{
									conn.DoorColor = Color.IndianRed;
								}

								conn.Color = conn.DoorColor;

								if (exit.KeyObject != null)
								{
									conn.DoorSigns.Add(new MMBRoomContentRecord($"{exit.KeyObject.ShortDescription} #{exit.KeyObject.Id}", conn.DoorColor));
								}
							}*/

			result.Connections[pair.Key.ToMMBDirection()] = conn;

		}

		return result;
	}

	public static MMBArea ToMMBArea(this Area area)
	{
		var result = new MMBArea
		{
			Name = area.Name,
			Credits = area.Credits,
			MinimumLevel = area.MinimumLevel,
			MaximumLevel = area.MaximumLevel
		};

		foreach (var room in area.Rooms)
		{
			result.Add(room.ToMMBRoom(area));
		}

		return result;
	}

	public static void EnsureFolder(string path)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	private static JsonSerializerOptions CreateOptions()
	{
		var result = new JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			IncludeFields = true,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true
		};

		result.Converters.Add(new JsonStringEnumConverter());

		return result;
	}

	public static void SerializeToFile<T>(string path, T data)
	{
		var s = JsonSerializer.Serialize(data, CreateOptions());
		File.WriteAllText(path, s);
	}

	public static T DeserializeFromFile<T>(string path)
	{
		var data = File.ReadAllText(path);
		return JsonSerializer.Deserialize<T>(data, CreateOptions());
	}
}
