using System.Text;
using AbarimMUD.Data;

namespace AbarimMUD
{
	public sealed class MainMenuHandler : Handler
	{
		private enum Mode
		{
			Top,
			SelectChar
		}

		private Mode _mode = Mode.Top;

		public MainMenuHandler(Session session)
			: base(session)
		{
		}

		public override void OnSet()
		{
			SendMainMenuPrompt();
		}

		private void SendMainMenuPrompt()
		{
			var sb = new StringBuilder();
			sb.AddTextLine("Main Menu:");
			sb.AddTextLine("1) Select a char to play");
			sb.AddTextLine("2) Change password");
			sb.AddTextLine("3) Exit");
			Send(sb.ToString());
		}

		private void SendCharacterMenuPrompt()
		{
			var i = 1;

			var sb = new StringBuilder();

			var characters = Database.Characters.GetByAccountName(Session.Account.Name);
			if (characters != null)
			{
				foreach (var c in characters)
				{
					sb.AddTextLine(string.Format("{0}) {1}", i, c.Name));

					++i;
				}
			}

			// New character
			sb.AddTextLine(string.Format("{0}) Create new character", i));
			++i;

			// Back
			sb.AddTextLine(string.Format("{0}) Back", i));

			Send(sb.ToString());
		}

		public override void Process(string data)
		{
			switch (_mode)
			{
				case Mode.Top:
					ProcessMainMenu(data);
					break;
				case Mode.SelectChar:
					ProcessCharacterMenu(data);
					break;
			}
		}

		private void ProcessMainMenu(string data)
		{
			switch (data)
			{
				case "1":
					_mode = Mode.SelectChar;
					SendCharacterMenuPrompt();
					break;
				case "2":
					Session.CurrentHandler = new ChangePasswordHandler(Session);
					break;
				case "3":
					Send("Bye");
					Session.Disconnect();
					break;
			}
		}

		private void ProcessCharacterMenu(string data)
		{
			int choice;
			if (int.TryParse(data, out choice))
			{
				var characters = Database.Characters.GetByAccountName(Session.Account.Name);
				var newIndex = characters != null ? characters.Length : 0;
				++newIndex;

				if (characters != null && choice >= 1 && choice < newIndex)
				{
					var character= characters[choice - 1];
					
					character.Tag = Session.Context;
					Session.Character = character;
					Session.CurrentHandler = new GameHandler(Session);
					return;
				}

				if (choice == newIndex)
				{
					Session.CurrentHandler = new NewCharacterHandler(Session);
					return;
				}

				if (choice == newIndex + 1)
				{
					// Back
					_mode = Mode.Top;
					SendMainMenuPrompt();
					return;
				}
			}

			SendTextLine("Invalid choice.");
			SendCharacterMenuPrompt();
		}
	}
}