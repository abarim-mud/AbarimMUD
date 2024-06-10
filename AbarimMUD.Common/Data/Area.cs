using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum ResetMode
	{
		None,
		ResetIfNoPC,
		ResetAlways
	}

	public class Area
	{
		public static readonly Areas Storage = new Areas();

		private ObservableCollection<Room> _rooms;
		private ObservableCollection<Mobile> _mobiles;

		public string Name { get; set; }

		public string Credits { get; set; }

		public string Builders { get; set; }
		public int Version { get; set; }
		public string ResetMessage { get; set; }

		public string MinimumLevel { get; set; }

		public string MaximumLevel { get; set; }

		public ObservableCollection<Room> Rooms
		{
			get => _rooms;
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (value == _rooms)
				{
					return;
				}

				if (_rooms != null)
				{
					_rooms.CollectionChanged -= OnRoomsChanged;
				}

				_rooms = value;

				_rooms.CollectionChanged += OnRoomsChanged;

				UpdateRooms();
			}
		}

		public ObservableCollection<Mobile> Mobiles
		{
			get => _mobiles;
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (value == _mobiles)
				{
					return;
				}

				if (_mobiles != null)
				{
					_mobiles.CollectionChanged -= OnMobilesChanged;
				}

				_mobiles = value;

				_mobiles.CollectionChanged += OnMobilesChanged;

				UpdateMobiles();
			}
		}

		public List<AreaReset> Resets { get; set; }

		public event EventHandler RoomsChanged, MobilesChanged;

		public Area()
		{
			Rooms = new ObservableCollection<Room>();
			Mobiles = new ObservableCollection<Mobile>();
			Resets = new List<AreaReset>();
		}

		private void OnRoomsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateRooms();
			RoomsChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OnMobilesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateMobiles();
			MobilesChanged?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateRooms()
		{
			foreach (var r in Rooms)
			{
				r.Area = this;
			}
		}

		private void UpdateMobiles()
		{
			foreach (var m in Mobiles)
			{
				m.Area = this;
			}
		}

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public override string ToString() => $"{MinimumLevel}-{MaximumLevel} {Builders} {Name}";

		public static int NextRoomId => Storage.NewRoomId;
		public static int NextMobileId => Storage.NewMobileId;

		public static Area GetAreaByName(string name) => Storage.GetByKey(name);
		public static Area EnsureAreaByName(string name) => Storage.EnsureByKey(name);
		public static Area LookupArea(string name) => Storage.Lookup(name);
		public static Room GetRoomById(int id) => Storage.GetRoomById(id);
		public static Room EnsureRoomById(int id) => Storage.EnsureRoomById(id);
		public static Mobile GetMobileById(int id) => Storage.GetMobileById(id);
		public static Mobile EnsureMobileById(int id) => Storage.EnsureMobileById(id);
	}
}
