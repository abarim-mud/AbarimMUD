using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class AreaEntity: IHasId<int>
	{
		[JsonIgnore]
		public Area Area { get; set; }

		public int Id { get; set; }
	}
}
