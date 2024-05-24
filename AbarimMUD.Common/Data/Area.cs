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
		private ObservableCollection<Mobile> _mobiles;

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

		[JsonIgnore]
		public List<GameObject> Objects { get; } = new List<GameObject>();

		[JsonIgnore]
		public List<AreaReset> Resets { get; } = new List<AreaReset>();

		public Area()
		{
			Rooms = new ObservableCollection<Room>();
			Mobiles = new ObservableCollection<Mobile>();
		}

		private void OnRoomsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEntities(Rooms);
		}

		private void OnMobilesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEntities(Mobiles);
		}

		private void UpdateEntities(IReadOnlyList<AreaEntity> entities)
		{
			for (var i = 0; i < entities.Count; ++i)
			{
				var entity = entities[i];
				entity.Area = this;
				entity.Id = i;
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

		public Mobile GetMobileById(int id)
		{
			if (id < 0 || id >= Mobiles.Count)
			{
				return null;
			}

			return Mobiles[id];
		}

		public Mobile EnsureMobileById(int id)
		{
			var result = GetMobileById(id);
			if (result == null)
			{
				throw new Exception($"Unable to find mobile with id {id} in the area {Name}");
			}

			return result;
		}

		public override string ToString() => $"{Name}";
	}
}
