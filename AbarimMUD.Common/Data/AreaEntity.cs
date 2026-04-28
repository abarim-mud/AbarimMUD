using AbarimMUD.Attributes;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Ur;

namespace AbarimMUD.Data
{
	public interface IAreaEntity : IHasId<int>
	{

		[Browsable(false)]
		[JsonIgnore]
		Area Area { get; set; }
	}

	public class AreaEntity : IAreaEntity
	{

		[Browsable(false)]
		[JsonIgnore]
		public Area Area { get; set; }

		[Browsable(false)]
		[OLCIgnore]
		public int Id { get; set; }
	}
}
