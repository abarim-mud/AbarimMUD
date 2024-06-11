using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AbarimMUD.Data
{
	public enum ResetMode
	{
		None,
		ResetIfNoPC,
		ResetAlways
	}

	public class AreaMobileReset
	{
		public string MobileId { get; set; }
		public int RoomId { get; set; }

		public AreaMobileReset()
		{
		}

		public AreaMobileReset(string mobileId, int roomId)
		{
			MobileId = mobileId;
			RoomId = roomId;
		}
	}

	public class Area
	{
		public static readonly Areas Storage = new Areas();

		private ObservableCollection<Room> _rooms;

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

		public List<AreaMobileReset> MobileResets { get; set; }

		public event EventHandler RoomsChanged;

		public Area()
		{
			Rooms = new ObservableCollection<Room>();
			MobileResets = new List<AreaMobileReset>();
		}

		private void OnRoomsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateRooms();
			RoomsChanged?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateRooms()
		{
			foreach (var r in Rooms)
			{
				r.Area = this;
			}
		}

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public override string ToString() => $"{MinimumLevel}-{MaximumLevel} {Builders} {Name}";

		public static int NextRoomId => Storage.NewRoomId;

		public static Area GetAreaByName(string name) => Storage.GetByKey(name);
		public static Area EnsureAreaByName(string name) => Storage.EnsureByKey(name);
		public static Area LookupArea(string name) => Storage.Lookup(name);
	}
}