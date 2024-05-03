using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class AreaEntity: Entity
	{
		[JsonIgnore]
		public int AreaId { get; set; }

		[JsonIgnore]
		public Area Area { get; set; }
	}
}
