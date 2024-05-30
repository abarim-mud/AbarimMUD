namespace AbarimMUD.ImportAre
{
	public enum SourceType
	{
		ROM,
		Envy
	}

	public class ImporterSettings
	{
		public string InputFolder { get; private set; }
		public string OutputFolder { get; private set; }
		public SourceType SourceType { get; private set; }

		public ImporterSettings(string inputFolder, string outputFolder, SourceType sourceType)
		{
			InputFolder = inputFolder;
			OutputFolder = outputFolder;
			SourceType = sourceType;
		}
	}
}
