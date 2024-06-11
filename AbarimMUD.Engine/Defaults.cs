namespace AbarimMUD
{
	internal static class Defaults
	{
		public static string DefaultMobileLongDesc(string shortDesc) => $"{shortDesc} is standing here.";
		public static string DefaultItemLongDesc(string shortDesc) => $"{shortDesc} is lying here.";
	}
}
