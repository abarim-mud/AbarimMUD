using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	public class StoredInFile
	{
		[JsonIgnore]
		public string Filename { get; set; }
	}
}
