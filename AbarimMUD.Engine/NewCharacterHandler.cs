using System.Linq;
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
			_character.PlayerLevel = 1;
		}

		public override void OnSet()
		{
			SendCharacterNamePrompt();
		}

		private void SendCharacterNamePrompt()
		{
			Send("Please, enter you new character name: ");
		}

		private static PlayerClass[] GetAvailablePlayerClasses()
		{
			return PlayerClass.Storage.ToArray();
		}

		private void SendChoosePrimaryClassPrompt()
		{
			var sb = new StringBuilder();
			sb.AppendLine("Please, choose a primary class:");

			var index = 1;

			var classes = GetAvailablePlayerClasses();
			foreach (var c in classes)
			{
				sb.AppendLine(string.Format("{0}) {1} - {2}", index, c.Name, c.Description));
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
				_character.Class.Name,
				_character.Sex == Sex.Male ? "male" : "female"));
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
				SendLine("Invalid name.");
				SendCharacterNamePrompt();
				return;
			}

			if (Character.GetCharacterByName(name) != null)
			{
				SendLine("This name is already taken.");
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
			if (!int.TryParse(data, out index) || index < 1 || index > PlayerClass.Storage.Count)
			{
				Send("Invalid selection.");
				SendChoosePrimaryClassPrompt();
				return;
			}

			var classes = GetAvailablePlayerClasses();
			_character.Class = classes[index - 1];

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
				SendLine("Invalid gender.");
				SendGenderPrompt();
				return;
			}

			_character.PlayerSex = data == "m" ? Sex.Male : Sex.Female;

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
				SendLine("Invalid choice.");
				SendConfirmPrompt();
				return;
			}

			if (data == "y")
			{
				// Grant initial gear
				var cls = _character.Class;
				if (cls.StartingEquipment != null)
				{
					foreach(var item in cls.StartingEquipment)
					{
						var newItem = new ItemInstance(item);
						if (_character.Wear(newItem) != true)
						{
							// If an item can't be worn, put it to the inventory
							_character.Inventory.AddItem(newItem, 1);
						}
					}
				}

				if (cls.StartingSkills != null && cls.StartingSkills.Length > 0)
				{
					foreach(var skill in cls.StartingSkills)
					{
						_character.Train(skill);
					}
				}

				// First character becomes owner
				if (Character.Storage.Count == 0)
				{
					_character.Role = Role.Owner;
					SendLine("This character is first in the game. Hence it will become the owner.");
				}

				_character.Account = Session.Account;
				_character.Save();
				SendLine("Character is saved.");
			}

			Session.CurrentHandler = new MainMenuHandler(Session);
		}
	}
}