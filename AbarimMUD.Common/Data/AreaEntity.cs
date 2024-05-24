using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class AreaEntity
	{
		[JsonIgnore]
		public Area Area { get; set; }

		[JsonIgnore]
		public int Id { get; set; }
	}
}
