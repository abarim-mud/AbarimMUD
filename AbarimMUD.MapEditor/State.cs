using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace AbarimMUD.MapEditor
{
	public class State
	{
		public const string StateFileName = "AbarimMUD.MapEditor.config";

		public static string StateFilePath
		{
			get
			{
				var result = Path.Combine(Utility.ExecutingAssemblyDirectory, StateFileName);
				return result;
			}
		}

		public Point Size { get; set; }
		public string EditedFolder { get; set; }
		public float SplitPosition { get; set; } = 0.25f;

		public State()
		{
		}

		public void Save()
		{
			using (var fileStream = File.Create(StateFilePath))
			{
				var xmlWriter = new XmlTextWriter(fileStream, Encoding.UTF8)
				{
					Formatting = Formatting.Indented
				};
				var serializer = new XmlSerializer(typeof(State));
				serializer.Serialize(xmlWriter, this);
			}
		}

		public static State Load()
		{
			if (!File.Exists(StateFilePath))
			{
				return null;
			}

			State state;
			using (var stream = new StreamReader(StateFilePath))
			{
				var serializer = new XmlSerializer(typeof(State));
				state = (State)serializer.Deserialize(stream);
			}

			return state;
		}
	}
}