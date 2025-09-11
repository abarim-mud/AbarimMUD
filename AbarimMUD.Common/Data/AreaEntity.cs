using AbarimMUD.Attributes;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public abstract class AreaEntity : IHasId<int>
	{

		[Browsable(false)]
		[JsonIgnore]
		public Area Area { get; set; }

		[Browsable(false)]
		[OLCIgnore]
		public abstract int Id { get; set; }
	}

	public class AreaEntityWithId : AreaEntity
	{
		public override int Id { get; set; }
	}
}
