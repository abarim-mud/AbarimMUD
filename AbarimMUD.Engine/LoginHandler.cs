using System;
using System.IO;
using System.Linq;
using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD
{
	public sealed class LoginHandler : Handler
	{
		enum Mode
		{
			Account,
			Password,
			ConfirmAccountName,
			ConfirmCreation,
			NewPassword,
			ConfirmNewPassword
		}

		private Mode _mode = Mode.Account;
		private Account _account;
		private int _passwordTries;
		private string _newName;
		private string _newPassword;

		public LoginHandler(Session session)
			: base(session)
		{
		}

		public override void OnSet()
		{
			var splash = "Welcome to AbarimMUD v0.1\n\r";
			if (string.IsNullOrEmpty(Configuration.SplashFile))
			{
				Logger.Warn("{0}SplashFile option is not set. Sending default splath");
			}
			else
			{
				if (!File.Exists(Configuration.SplashFile))
				{
					Logger.Warn("Splash file {0} couldn't be found. Sending default splath", Configuration.SplashFile);
				}
				else
				{
					try
					{
						splash = File.ReadAllText(Configuration.SplashFile);
					}
					catch (Exception ex)
					{
						Logger.Error(ex);
						Logger.Warn("Splash file {0} couldn't be found. Sending default splath",
							Configuration.SplashFile);
					}
				}
			}

			Send(splash);
			SwitchToAccount();
		}

		public override void Process(string data)
		{
			switch (_mode)
			{
				case Mode.Account:
					ProcessAccount(data);
					break;
				case Mode.Password:
					ProcessPassword(data);
					break;
				case Mode.ConfirmAccountName:
					ProcessConfirmAccountName(data);
					break;
				case Mode.ConfirmCreation:
					ProcessConfirmCreation(data);
					break;
				case Mode.NewPassword:
					ProcessNewPassword(data);
					break;
				case Mode.ConfirmNewPassword:
					ProcessRepeatPassword(data);
					break;
			}
		}

		private void Login()
		{
			SendLine(string.Format("Welcome, {0}!", _account.Name));

			Session.Account = _account;

			// Check if there are already sessions with that account
			var accountSessions = (from s in Server.Instance.Sessions
				where s != Session &&
				      s.Account != null && s.Account.Name == _account.Name
				select s).ToList();

			// Find non-reconnecting session among them
			var session =
				(from s in accountSessions where !(s.CurrentHandler is ReconnectHandler) select s).FirstOrDefault();
			Logger.Info("{0} active sessions with that account has been found.", accountSessions.Count);

			if (session != null)
			{
				accountSessions.Remove(session);
			}

			// Close all others
			foreach (var s in accountSessions)
			{
				s.Disconnect();
			}

			if (session != null)
			{
				// Handle reconnection
				Session.CurrentHandler = new ReconnectHandler(Session, session);
			}
			else
			{
				Session.CurrentHandler = new MainMenuHandler(Session);
			}
		}

		private void SwitchToAccount()
		{
			_mode = Mode.Account;
			SendAccountPrompt();
		}

		private void SendAccountPrompt()
		{
			Send("Please, enter you account name: ");
		}

		private void SendPasswordPrompt()
		{
			Send("Please, enter you password: ");
		}

		private void SendNewPasswordPrompt()
		{
			Send("Please, enter password for the new account: ");
		}

		private void SendRepeatPasswordPrompt()
		{
			Send("Please, repeat the password: ");
		}

		private void ProcessAccount(string name)
		{
			name = name.CasedName();
			if (string.IsNullOrEmpty(name))
			{
				SendLine("Invalid account name.");
				SendAccountPrompt();
				return;
			}

			var acc = Account.GetAccountByName(name);
			if (acc == null)
			{
				SendLine(string.Format("Account '{0}' is unknown.", name));
				Send("Did you type it right (y/n)? ");
				_newName = name;
				_mode = Mode.ConfirmAccountName;
				return;
			}

			_account = acc;

			SendPasswordPrompt();
			_passwordTries = 3;
			_mode = Mode.Password;
		}

		private void ProcessPassword(string pwd)
		{
			var passwordCorrect = false;

			do
			{
				if (!CheckPassword(pwd))
				{
					break;
				}

				var inputHash = HashUtils.CalculateMD5Hash(pwd);
				if (inputHash != _account.PasswordHash)
				{
					SendLine("Incorrect password.");
					break;
				}

				passwordCorrect = true;
			} while (false);

			if (!passwordCorrect)
			{
				--_passwordTries;
				if (_passwordTries <= 0)
				{
					// After running out of password tries return to account mode
					SwitchToAccount();
				}
				else
				{
					SendPasswordPrompt();
				}
			}
			else
			{
				Login();
			}
		}

		private void ProcessConfirmAccountName(string data)
		{
			if (string.Compare(data, "y", StringComparison.OrdinalIgnoreCase) == 0)
			{
				Send("Would you like to create a new account (y/n)? ");
				_mode = Mode.ConfirmCreation;
				return;
			}

			SwitchToAccount();
		}

		private void ProcessConfirmCreation(string data)
		{
			if (string.Compare(data, "y", StringComparison.OrdinalIgnoreCase) == 0)
			{
				SendNewPasswordPrompt();
				_mode = Mode.NewPassword;
				return;
			}

			SwitchToAccount();
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
				SendLine("Sorry, but passwords do not match.");
				SendNewPasswordPrompt();
				_mode = Mode.NewPassword;
				return;
			}

			SendLine("Creating new account...");
			_account = new Account
			{
				Name = _newName,
				PasswordHash = HashUtils.CalculateMD5Hash(_newPassword)
			};

			_account.Save();
			Login();
		}
	}
}