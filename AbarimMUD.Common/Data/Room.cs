using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AbarimMUD.Data
{
	public enum SectorType
	{
		Inside,
		City,
		Field,
		Forest,
		Hills,
		Mountain,
		WaterSwim,
		WaterNoSwim,
		Unused,
		Air,
		Desert
	}

	public class Room : AreaEntity
	{
		public int? VNum { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Flags { get; set; }
		public SectorType SectorType { get; set; }
		public int HealRate { get; set; }
		public int ManaRate { get; set; }
		public string ExtraKeyword { get; set; }
		public string ExtraDescription { get; set; }
		public string Owner { get; set; }
		public List<RoomDirection> Exits { get; } = new List<RoomDirection>();

		[NotMapped]
		public List<MobileInstance> Mobiles { get; } = new List<MobileInstance>();

		[NotMapped]
		public List<Character> Characters { get; } = new List<Character>();

		public void Connect(DirectionType direction, Room targetRoom)
		{
			Exits.RemoveAll(rd => rd.DirectionType == direction);

			if (targetRoom != null)
			{
				var rd = new RoomDirection
				{
					SourceRoomId = Id,
					SourceRoom = this,
					TargetRoomId = targetRoom.Id,
					TargetRoom = targetRoom,
					DirectionType = direction,
				};

				Exits.Add(rd);
			}
		}

		public Room GetConnectedRoom(DirectionType direction)
		{
			var roomDirection = (from rd in Exits where rd.DirectionType == direction select rd).FirstOrDefault();
			if (roomDirection == null)
			{
				return null;
			}

			return roomDirection.TargetRoom;
		}
	}
}
