using AbarimMUD.GameClasses;
using System.Text;
using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD
{
	public sealed class NewCharacterHandler : Handler
	{
		enum Mode
		{
			Name,
			PrimaryClass,
			Gender,
			Confirm
		}

		private readonly Character _character = new Character();
		private Mode _mode = Mode.Name;

		public NewCharacterHandler(Session session)
			: base(session)
		{
		}

		public override void OnSet()
		{
			SendCharacterNamePrompt();
		}

		private void SendCharacterNamePrompt()
		{
			Send("Please, enter you new character name: ");
		}

		private void SendChoosePrimaryClassPrompt()
		{
			var sb = new StringBuilder();
			sb.AddTextLine("Please, choose a primary class:");

			var index = 1;

			foreach (var c in BaseClass.AllClasses)
			{
				sb.AddTextLine(string.Format("{0}) {1} - {2}", index, c.Name, c.Description));
				++index;
			}

			Send(sb.ToString());
		}

		private void SendGenderPrompt()
		{
			Send("Please, choose your gender(m/f): ");
		}

		private void SendConfirmPrompt()
		{
			Send(string.Format("Please, confirm your new character({0}, {1}, {2}): (y/n)",
				_character.Name,
				_character.GameClassName,
				_character.IsMale ? "male" : "female"));
		}

		public override void Process(string data)
		{
			switch (_mode)
			{
				case Mode.Name:
					ProcessName(data);
					break;
				case Mode.PrimaryClass:
					ProcessPrimaryClass(data);
					break;
				case Mode.Gender:
					ProcessGender(data);
					break;
				case Mode.Confirm:
					ProcessConfirm(data);
					break;
			}
		}

		private void ProcessName(string name)
		{
			name = name.CasedName();
			if (string.IsNullOrEmpty(name))
			{
				SendTextLine("Invalid name.");
				SendCharacterNamePrompt();
				return;
			}

			if (Database.Characters.GetById(name) != null)
			{
				SendTextLine("This name is already taken.");
				SendCharacterNamePrompt();
				return;
			}

			_character.Name = name;

			_mode = Mode.PrimaryClass;
			SendChoosePrimaryClassPrompt();
		}

		private void ProcessPrimaryClass(string data)
		{
			int index;
			if (!int.TryParse(data, out index) || index < 1 || index > BaseClass.AllClasses.Length)
			{
				Send("Invalid selection.");
				SendChoosePrimaryClassPrompt();
				return;
			}

			var className = BaseClass.AllClasses[index - 1];
			_character.GameClassName = className.Name;

			_mode = Mode.Gender;
			SendGenderPrompt();
		}

		private void ProcessGender(string data)
		{
			data = data.ToLower().Trim();
			if (string.IsNullOrEmpty(data) &&
			    data != "m" &&
			    data != "f")
			{
				SendTextLine("Invalid gender.");
				SendGenderPrompt();
				return;
			}

			_character.IsMale = (data == "m");

			_mode = Mode.Confirm;
			SendConfirmPrompt();
		}

		private void ProcessConfirm(string data)
		{
			data = data.ToLower().Trim();
			if (string.IsNullOrEmpty(data) &&
			    data != "y" &&
			    data != "n")
			{
				SendTextLine("Invalid choice.");
				SendConfirmPrompt();
				return;
			}

			if (data == "y")
			{
				_character.CurrentRoomId = 0;

				// First character becomes owner
				if (Database.CalculateCharactersAmount() == 0)
				{
					_character.Role = Role.Owner;
					SendTextLine("This character is first in the game. Hence it will become the owner.");
				}

				_character.Account = Session.Account;
				Database.Characters.Update(_character);
				SendTextLine("Character is saved.");
			}

			Session.CurrentHandler = new MainMenuHandler(Session);
		}
	}
}