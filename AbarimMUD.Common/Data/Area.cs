using AbarimMUD.Attributes;
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
		public int MobileId { get; set; }
		public int RoomId { get; set; }

		public AreaMobileReset()
		{
		}

		public AreaMobileReset(int mobileId, int roomId)
		{
			MobileId = mobileId;
			RoomId = roomId;
		}
	}

	public class Area : IStoredInFile
	{
		public static readonly Areas Storage = new Areas();

		private ObservableCollection<Room> _rooms;
		private ObservableCollection<Mobile> _mobiles;

		[OLCIgnore]
		public string Id { get; set; }

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

				UpdateEntities(Rooms);
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

				UpdateEntities(Mobiles);
			}
		}

		public List<AreaMobileReset> MobileResets { get; set; }

		public event EventHandler RoomsChanged;
		public event EventHandler MobilesChanged;

		public Area()
		{
			Rooms = new ObservableCollection<Room>();
			Mobiles = new ObservableCollection<Mobile>();
			MobileResets = new List<AreaMobileReset>();
		}

		private void OnRoomsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEntities(Rooms);
			RoomsChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OnMobilesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEntities(Mobiles);
			MobilesChanged?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateEntities(IEnumerable<AreaEntity> entities)
		{
			foreach (var r in entities)
			{
				r.Area = this;
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
	}
}