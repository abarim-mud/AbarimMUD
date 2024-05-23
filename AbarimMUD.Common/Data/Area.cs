using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum ResetMode
	{
		None,
		ResetIfNoPC,
		ResetAlways
	}

	public class Area : Entity
	{
		private ObservableCollection<Room> _rooms;

		[JsonIgnore]
		public string Name
		{
			get => Id;
			set => Id = value;
		}

		public string Credits { get; set; }

		public string Builders { get; set; }

		public int? MinimumLevel { get; set; }

		public int? MaximumLevel { get; set; }

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

		[JsonIgnore]
		public List<Mobile> Mobiles { get; } = new List<Mobile>();

		[JsonIgnore]
		public List<GameObject> Objects { get; } = new List<GameObject>();

		[JsonIgnore]
		public List<AreaReset> Resets { get; } = new List<AreaReset>();

		public Area()
		{
			Rooms = new ObservableCollection<Room>();
		}

		private void OnRoomsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateRooms();
		}

		public void UpdateRooms()
		{
			for(var i = 0; i < Rooms.Count; ++i)
			{
				var room = Rooms[i];
				room.Area = this;
				room.Id = i;
			}
		}

		public Room GetRoomById(int id)
		{
			if (id < 0 || id >= Rooms.Count)
			{
				return null;
			}

			return Rooms[id];
		}

		public Room EnsureRoomById(int id)
		{
			var result = GetRoomById(id);
			if (result == null)
			{
				throw new Exception($"Unable to find room with id {id} in the area {Name}");
			}

			return result;
		}

		public override string ToString() => $"{Name}";
	}
}
