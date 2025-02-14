using System.Reflection;

namespace AbarimMUD.Utils
{
	internal static class Common
	{
		public static string Version
		{
			get
			{
				var assembly = typeof(Common).Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}
	}
}
