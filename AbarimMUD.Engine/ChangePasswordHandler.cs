using AbarimMUD.Utils;

namespace AbarimMUD
{
	public sealed class ChangePasswordHandler : Handler
	{
		enum Mode
		{
			NewPassword,
			ConfirmNewPassword
		}

		private Mode _mode = Mode.NewPassword;
		private string _newPassword;

		public ChangePasswordHandler(Session session)
			: base(session)
		{
		}

		public override void OnSet()
		{
			SendNewPasswordPrompt();
		}

		private void SendNewPasswordPrompt()
		{
			Send("Please, enter the new password: ");
		}

		private void SendRepeatPasswordPrompt()
		{
			Send("Please, repeat the new password: ");
		}

		private void ProcessNewPassword(string data)
		{
			if (!CheckPassword(data))
			{
				SendNewPasswordPrompt();
				return;
			}

			_newPassword = data;
			_mode = Mode.ConfirmNewPassword;
			SendRepeatPasswordPrompt();
		}

		private void ProcessRepeatPassword(string data)
		{
			if (data != _newPassword)
			{
				Send("Sorry, but passwords do not match.\n");
				SendNewPasswordPrompt();
				_mode = Mode.NewPassword;
				return;
			}

			Send("Updating the password...\n");

			Session.Account.PasswordHash = HashUtils.CalculateMD5Hash(_newPassword);
			Session.Account.Save();

			Session.CurrentHandler = new MainMenuHandler(Session);
		}

		public override void Process(string data)
		{
			switch (_mode)
			{
				case Mode.NewPassword:
					ProcessNewPassword(data);
					break;
				case Mode.ConfirmNewPassword:
					ProcessRepeatPassword(data);
					break;
			}
		}
	}
}