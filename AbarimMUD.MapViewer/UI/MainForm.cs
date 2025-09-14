using AbarimMUD.Data;
using AbarimMUD.Storage;
using MUDMapBuilder;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Myra.Graphics2D.UI.Styles;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AbarimMUD.MapViewer.UI
{
	public partial class MainForm
	{
		private string _folder;
		private bool _isDirty;
		private readonly MapViewer _mapViewer;

		private MapBuilderResult Result { get; set; }

		private MMBArea Area
		{
			get
			{
				if (Result == null)
				{
					return null;
				}

				return Result.Last;
			}
		}

		public string Folder
		{
			get => _folder;

			set
			{
				if (value == _folder)
				{
					return;
				}

				_folder = value;

				UpdateTitle();
				UpdateEnabled();
			}
		}

		public bool IsDirty
		{
			get
			{
				return _isDirty;
			}

			set
			{
				if (value == _isDirty)
				{
					return;
				}

				_isDirty = value;
				UpdateTitle();
			}
		}

		public MainForm()
		{
			BuildUI();

			_mapViewer = new MapViewer();
			_panelMap.Content = _mapViewer;

			_menuItemFileOpen.Selected += (s, e) => OnMenuFileOpenSelected();
			_menuItemFileReload.Selected += (s, e) => OnMenuFileReload();

			_menuHelpAbout.Selected += _menuHelpAbout_Selected;

			_listAreas.Widgets.Clear();
			_listAreas.SelectedIndexChanged += _listAreas_SelectedIndexChanged;

			var atlas = Stylesheet.Current.Atlas;
			_gridMobiles.SelectionHoverBackground = atlas["button-over"];
			_gridMobiles.SelectionBackground = atlas["button-down"];

			UpdateEnabled();
		}

		private void _menuHelpAbout_Selected(object sender, EventArgs e)
		{
			var messageBox = Dialog.CreateMessageBox("About", "AbarimMUD.MapViewer " + Utility.Version);
			messageBox.ShowModal(Desktop);
		}

		private void _listAreas_SelectedIndexChanged(object sender, EventArgs e)
		{
			_gridMobiles.Widgets.Clear();
			_mapViewer.Redraw(null, null, false);
			if (_listAreas.SelectedItem == null)
			{
				return;
			}

			var area = (MMBArea)_listAreas.SelectedItem.Tag;
			Rebuild(area);

			var sourceArea = (Area)area.Tag;

			var row = 0;

			// Header
			var label = new Label
			{
				Text = "#",
			};

			Grid.SetColumn(label, 0);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Name",
			};

			Grid.SetColumn(label, 1);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Id",
			};

			Grid.SetColumn(label, 2);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Level",
			};

			Grid.SetColumn(label, 3);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "HP",
			};

			Grid.SetColumn(label, 4);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Mana",
			};

			Grid.SetColumn(label, 5);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Moves",
			};

			Grid.SetColumn(label, 6);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Armor",
			};

			Grid.SetColumn(label, 7);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Attacks",
			};

			Grid.SetColumn(label, 8);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Experience",
			};

			Grid.SetColumn(label, 9);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Gold",
			};

			Grid.SetColumn(label, 10);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			label = new Label
			{
				Text = "Flags",
			};

			Grid.SetColumn(label, 11);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);

			++row;

			// Separator
			label = new Label
			{
				Text = "-------------------------------------------------------------------------------------------------------------------------------",
			};

			Grid.SetColumnSpan(label, 12);
			Grid.SetRow(label, row);
			_gridMobiles.Widgets.Add(label);
			++row;

			var orderedMobiles = (from m in sourceArea.Mobiles orderby m.Level select m).ToList();
			foreach (var mobile in orderedMobiles)
			{
				// Id
				label = new Label
				{
					Text = mobile.Id.ToString(),
					Tag = mobile
				};

				Grid.SetColumn(label, 0);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Name
				label = new Label
				{
					Text = mobile.ShortDescription,
					Tag = mobile
				};

				Grid.SetColumn(label, 1);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Class
				label = new Label
				{
					Text = mobile.Id.ToString()
				};

				Grid.SetColumn(label, 2);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Level
				label = new Label
				{
					Text = mobile.Level.ToString()
				};

				Grid.SetColumn(label, 3);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				var stats = mobile.CreateStats();

				// HP
				label = new Label
				{
					Text = stats.MaxHitpoints.ToString()
				};

				Grid.SetColumn(label, 4);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Mana
				label = new Label
				{
					Text = stats.MaxMana.ToString()
				};

				Grid.SetColumn(label, 5);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Moves
				label = new Label
				{
					Text = stats.MaxMoves.ToString()
				};

				Grid.SetColumn(label, 6);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Armor
				label = new Label
				{
					Text = stats.Armor.ToString()
				};

				Grid.SetColumn(label, 7);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Attacks
				// Merge similar attacks
				var mergedAttacks = new List<Tuple<int, Attack>>();

				Tuple<int, Attack> lastAttack = null;
				for (var i = 0; i < stats.Attacks.Count; ++i)
				{
					var atk = stats.Attacks[i];

					if (lastAttack == null)
					{
						lastAttack = new Tuple<int, Attack>(1, atk);
					}
					else if (lastAttack.Item2.AttackBonus == atk.AttackBonus &&
						lastAttack.Item2.MinimumDamage == atk.MinimumDamage &&
						lastAttack.Item2.MaximumDamage == atk.MaximumDamage)
					{
						lastAttack = new Tuple<int, Attack>(lastAttack.Item1 + 1, atk);
					}
					else
					{
						mergedAttacks.Add(lastAttack);
						lastAttack = new Tuple<int, Attack>(1, atk);
					}
				}

				if (lastAttack != null)
				{
					mergedAttacks.Add(lastAttack);
				}

				var sb = new StringBuilder();
				for (var i = 0; i < mergedAttacks.Count; ++i)
				{
					var atk = mergedAttacks[i].Item2;
					sb.Append($"{mergedAttacks[i].Item1}x({atk.AttackBonus}:{atk.MinimumDamage}-{atk.MaximumDamage})");

					if (i < mergedAttacks.Count - 1)
					{
						sb.Append(", ");
					}
				}

				label = new Label
				{
					Text = sb.ToString()
				};

				Grid.SetColumn(label, 8);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Experience
				label = new Label
				{
					Text = stats.CalculateXpAward().FormatBigNumber()
				};

				Grid.SetColumn(label, 9);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Gold
				label = new Label
				{
					Text = mobile.Gold.FormatBigNumber()
				};

				Grid.SetColumn(label, 10);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);

				// Flags
				label = new Label
				{
					Text = string.Join(", ", mobile.Flags)
				};

				Grid.SetColumn(label, 11);
				Grid.SetRow(label, row);
				_gridMobiles.Widgets.Add(label);
				++row;
			}
		}

		public void OnMenuFileOpenSelected()
		{
			FileDialog dialog = new FileDialog(FileDialogMode.ChooseFolder);

			if (!string.IsNullOrEmpty(Folder))
			{
				dialog.Folder = Path.GetDirectoryName(Folder);
			}

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				// "Ok" or Enter
				LoadStorage(dialog.FilePath);
			};

			dialog.ShowModal(Desktop);
		}

		public void OnMenuFileReload()
		{
			_listAreas.SelectedIndex = null;
			LoadStorage(Folder);
		}

		private void UpdateNumbers()
		{
			var area = Area;
			if (area != null)
			{
				_labelRoomsCount.Text = $"Rooms Count: {area.PositionedRoomsCount}/{area.Count}";
				_labelGridSize.Text = $"Grid Size: {area.Width}x{area.Height}";

				_labelStatus.Text = Area.LogMessage;
			}
			else
			{
				_labelRoomsCount.Text = "";
				_labelGridSize.Text = "";
			}
		}

		private void UpdateEnabled()
		{
			var enabled = Area != null;

			if (enabled)
			{
				Area.ClearMarks();
			}

			_menuItemFileReload.Enabled = !string.IsNullOrEmpty(Folder);

			UpdateNumbers();
		}

		private void InternalRebuildMap(MMBArea area)
		{
			Utility.QueueUIAction(() =>
			{
				_mapViewer.Redraw(null, null, false);
			});

			var project = new MMBProject(area, new BuildOptions());
			var result = MapBuilder.MultiRun(project, Utility.SetStatusMessage);

			Utility.QueueUIAction(() =>
			{
				Result = result;
				_mapViewer.Redraw(Area, project.BuildOptions, false);

				UpdateEnabled();
			});
		}

		private void Rebuild(MMBArea area)
		{
			try
			{
				Task.Run(() => InternalRebuildMap(area));
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}


		public void LoadStorage(string path)
		{
			try
			{
				DataContext.Load(path);

				var areas = Utility.BuildMMBAreas();

				_listAreas.Widgets.Clear();

				foreach (var area in areas)
				{
					var label = new Label
					{
						Text = area.Name,
						Tag = area
					};

					_listAreas.Widgets.Add(label);
				}

				Folder = path;
				IsDirty = false;
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		private void UpdateTitle()
		{
			var title = string.IsNullOrEmpty(_folder) ? "AbarimMUD.MapViewer" : _folder;

			if (_isDirty)
			{
				title += " *";
			}

			EditorGame.Instance.Window.Title = title;
		}
	}
}