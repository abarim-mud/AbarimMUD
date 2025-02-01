using AbarimMUD.Attributes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class AreaEntity : IHasId<int>
	{

		[Browsable(false)]
		[JsonIgnore]
		public Area Area { get; set; }

		[Browsable(false)]
		[OLCIgnore]
		public int Id { get; set; }
	}
}
