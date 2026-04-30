using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Collections.Generic;

namespace AbarimMUD.Commands
{
	internal static class SocialHelper
	{
		public static bool DoSocial(this Social social, ExecutionContext context, string data)
		{
			var player = context.Creature;
			var variables = new Dictionary<string, string>()
			{
				{ "player.name", player.ShortDescription },
				{ "player.pronoun1", player.Sex.GetPronoun1() },
				{ "player.pronoun2", player.Sex.GetPronoun2() },
				{ "player.pronoun3", player.Sex.GetPronoun3() }
			};

			data = data.Trim();

			if (string.IsNullOrEmpty(data))
			{
				// No target
				if (!string.IsNullOrEmpty(social.NoTargetPlayer))
				{
					context.Send(social.NoTargetPlayer.FormatMessage(variables));
				}

				if (!string.IsNullOrEmpty(social.NoTargetRoom))
				{
					context.SendRoomExceptMe(social.NoTargetRoom.FormatMessage(variables));
				}

				return true;
			}

			var targetContext = context.Room.Find(data);
			if (targetContext == null)
			{
				if (!string.IsNullOrEmpty(social.TargetNotFoundPlayer))
				{
					context.Send(social.TargetNotFoundPlayer.FormatMessage(variables));
				}

				return true;
			}

			if (targetContext != context)
			{
				var target = targetContext.Creature;
				variables["target.name"] = target.ShortDescription;
				variables["target.pronoun1"] = target.Sex.GetPronoun1();
				variables["target.pronoun2"] = target.Sex.GetPronoun2();
				variables["target.pronoun3"] = target.Sex.GetPronoun3();

				if (!string.IsNullOrEmpty(social.TargettedPlayer))
				{
					context.Send(social.TargettedPlayer.FormatMessage(variables));
				}

				if (!string.IsNullOrEmpty(social.TargettedTarget))
				{
					targetContext.Send(social.TargettedTarget.FormatMessage(variables));
				}

				if (!string.IsNullOrEmpty(social.TargettedRoom))
				{
					context.SendRoomExceptMeAndTarget(targetContext, social.TargettedRoom.FormatMessage(variables));
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(social.SelfPlayer))
				{
					context.Send(social.SelfPlayer.FormatMessage(variables));
				}
				if (!string.IsNullOrEmpty(social.SelfRoom))
				{
					context.SendRoomExceptMe(social.SelfRoom.FormatMessage(variables));
				}
			}

			return true;
		}
	}
}
