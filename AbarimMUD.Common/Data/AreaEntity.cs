using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class AreaEntity
	{
		[JsonIgnore]
		public Area Area { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
		public int Id { get; set; }
	}
}
