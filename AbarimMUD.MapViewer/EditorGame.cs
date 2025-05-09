﻿using Myra.Graphics2D.UI;
using Microsoft.Xna.Framework;
using Myra;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using AbarimMUD.MapViewer.UI;

namespace AbarimMUD.MapViewer
{
	public class EditorGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;

		private Desktop _desktop;
		private MainForm _mainForm;
		private readonly State _state;
		private readonly ConcurrentQueue<Action> _uiActions = new ConcurrentQueue<Action>();

		public static EditorGame Instance { get; private set; }

		public EditorGame()
		{
			Instance = this;

			// Restore state
			_state = State.Load();

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};
			Window.AllowUserResizing = true;
			IsMouseVisible = true;

			if (_state != null)
			{
				_graphics.PreferredBackBufferWidth = _state.Size.X;
				_graphics.PreferredBackBufferHeight = _state.Size.Y;
			}
			else
			{
				_graphics.PreferredBackBufferWidth = 1280;
				_graphics.PreferredBackBufferHeight = 800;
			}
		}

		public void SetStatusMessage(string message)
		{
			QueueUIAction(() => _mainForm._labelStatus.Text = message);
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			MyraEnvironment.Game = this;

			_desktop = new Desktop();
			_mainForm = new MainForm();
			_desktop.Root = _mainForm;

			_desktop.KeyDown += (s, a) =>
			{
				if (_desktop.HasModalWidget || _mainForm._mainMenu.IsOpen)
				{
					return;
				}

				if (_desktop.IsKeyDown(Keys.LeftControl) || _desktop.IsKeyDown(Keys.RightControl))
				{
					if (_desktop.IsKeyDown(Keys.O))
					{
						_mainForm.OnMenuFileOpenSelected();
					}

					if (_desktop.IsKeyDown(Keys.R))
					{
						_mainForm.OnMenuFileReload();
					}
				}
			};

			if (_state != null)
			{
				try
				{
					_mainForm._topSplitPane.SetSplitterPosition(0, _state.SplitPosition);

					if (!string.IsNullOrEmpty(_state.EditedFolder))
					{
						_mainForm.LoadStorage(_state.EditedFolder);
					}
				}
				catch(Exception)
				{
				}
			}
		}

		public void QueueUIAction(Action action)
		{
			_uiActions.Enqueue(action);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			while (!_uiActions.IsEmpty)
			{
				Action action;
				_uiActions.TryDequeue(out action);

				try
				{
					action();
				}
				catch (Exception ex)
				{
					var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
					dialog.ShowModal(_desktop);
				}
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);
			_desktop.Render();
		}

		protected override void EndRun()
		{
			base.EndRun();

			var state = new State
			{
				Size = new Point(GraphicsDevice.PresentationParameters.BackBufferWidth,
					GraphicsDevice.PresentationParameters.BackBufferHeight),
				EditedFolder = _mainForm.Folder,
				SplitPosition = _mainForm._topSplitPane.GetSplitterPosition(0),
			};

			state.Save();
		}
	}
}