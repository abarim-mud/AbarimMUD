using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class RoomReference
	{
		public string TargetAreaName { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int TargetRoomId { get; set; }
	}
}
