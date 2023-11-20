using NLog;

namespace AbarimMUD.Utils
{
	public static class LogUtility
	{
		public const string GlobalLoggerName = "global";

		public static Logger GetGlobalLogger()
		{
			return LogManager.GetLogger(GlobalLoggerName);
		}
	}
}