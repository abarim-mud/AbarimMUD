namespace AbarimMUD.Commands
{
	public static class CommandUtils
	{
		public static void ParseCommand(this string cmd, out string cmdText, out string cmdData)
		{
			cmdText = string.Empty;
			cmdData = string.Empty;
			var i = 0;
			for (; i < cmd.Length; ++i)
			{
				if (cmd[i] == ' ')
				{
					break;
				}

				cmdText += cmd[i];
			}

			cmdText = cmdText.Trim().ToLower();

			if (i < cmd.Length)
			{
				cmdData = cmd.Substring(i + 1);
			}
		}
	}
}