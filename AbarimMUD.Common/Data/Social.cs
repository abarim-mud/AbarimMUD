using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class Social: Entity
	{
		[JsonIgnore]
		public string Name
		{
			get => Id;
			set => Id = value;
		}

		public string CharNoArg { get; set; }
		public string OthersNoArg { get; set; }
		public string CharFound { get; set; }
		public string OthersFound { get; set; }
		public string VictFound {  get; set; }
		public string CharNotFound { get; set; }
		public string CharAuto { get; set; }
		public string OthersAuto { get; set; }
	}
}
