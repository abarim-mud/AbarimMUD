using AbarimMUD.Attributes;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class AreaEntity : IHasId<int>
	{

		[JsonIgnore]
		public Area Area { get; set; }

		[OLCIgnore]
		public int Id { get; set; }
	}
}
