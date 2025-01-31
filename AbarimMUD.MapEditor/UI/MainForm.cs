using AbarimMUD.Data;
using AbarimMUD.Storage;
using MUDMapBuilder;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AbarimMUD.MapEditor.UI
{
	public partial class MainForm
	{
		private string _folder;
		private bool _isDirty;
		private readonly MapViewer _mapViewer;
		private bool _suspendUi = false;

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
			_menuItemFileSave.Selected += (s, e) => Save(false);
			_menuItemFileSaveAs.Selected += (s, e) => Save(true);

			_mapViewer.SelectedIndexChanged += (s, e) => UpdateEnabled();

			_listAreas.Widgets.Clear();
			_listMobiles.Widgets.Clear();

			_listAreas.SelectedIndexChanged += _listAreas_SelectedIndexChanged;

			UpdateEnabled();
		}

		private void _listAreas_SelectedIndexChanged(object sender, EventArgs e)
		{
			var area = (MMBArea)_listAreas.SelectedItem.Tag;
			Rebuild(area);
		}

		private void ProcessSave(string filePath)
		{
/*			var data = Project.ToJson();
			File.WriteAllText(filePath, data);

			Folder = filePath;
			IsDirty = false;*/
		}

		public void Save(bool setFileName)
		{
			if (string.IsNullOrEmpty(Folder) || setFileName)
			{
				var dlg = new FileDialog(FileDialogMode.SaveFile)
				{
					Filter = "*.json"
				};

				if (!string.IsNullOrEmpty(Folder))
				{
					dlg.FilePath = Folder;
				}

				dlg.ShowModal(Desktop);

				dlg.Closed += (s, a) =>
				{
					if (dlg.Result)
					{
						ProcessSave(dlg.FilePath);
					}
				};
			}
			else
			{
				ProcessSave(Folder);
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

			_menuItemFileSave.Enabled = enabled;
			_menuItemFileSaveAs.Enabled = enabled;

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
			/*			try
						{
							var data = File.ReadAllText(path);
							Project = MMBProject.Parse(data);

							if (Project.Area == null || Project.Area.Rooms == null || Project.Area.Rooms.Length == 0)
							{
								var dialog = Dialog.CreateMessageBox("Error", $"Area '{Project.Area.Name}' has no rooms");
								dialog.ShowModal(Desktop);
								return;
							}

							try
							{
								_suspendUi = true;

								var options = Project.BuildOptions;
								_checkRemoveSolitaryRooms.IsChecked = options.RemoveSolitaryRooms;
								_checkRemoveRoomsWithSingleOutsideExit.IsChecked = options.RemoveRoomsWithSingleOutsideExit;
								_checkAddDebugInfo.IsChecked = options.AddDebugInfo;
							}
							finally
							{
								_suspendUi = false;
							}

							Task.Run(() =>
							{
								InternalRebuild(path, true);
							});
						}
						catch (Exception ex)
						{
							var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
							dialog.ShowModal(Desktop);
						}*/

			try
			{
				DataContext.Load(path);

				var areas = Utility.BuildMMBAreas();

				_listAreas.Widgets.Clear();

				foreach(var area in areas)
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
			var title = string.IsNullOrEmpty(_folder) ? "AbarimMUD.MapEditor" : _folder;

			if (_isDirty)
			{
				title += " *";
			}

			EditorGame.Instance.Window.Title = title;
		}
	}
}