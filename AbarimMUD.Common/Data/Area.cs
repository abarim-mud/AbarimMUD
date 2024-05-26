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

	public class Area : Entity
	{
		private ObservableCollection<Room> _rooms;
		private ObservableCollection<Mobile> _mobiles;
		private ObservableCollection<GameObject> _objects;

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
		public ObservableCollection<GameObject> Objects
		{
			get => _objects;
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (value == _objects)
				{
					return;
				}

				if (_objects != null)
				{
					_objects.CollectionChanged -= OnObjectsChanged;
				}

				_objects = value;

				_objects.CollectionChanged += OnObjectsChanged;

				UpdateEntities(Objects);
			}
		}

		public List<AreaReset> Resets { get; set; } = new List<AreaReset>();

		public event EventHandler RoomsChanged, MobilesChanged, ObjectsChanged;

		public Area()
		{
			Rooms = new ObservableCollection<Room>();
			Mobiles = new ObservableCollection<Mobile>();
			Objects = new ObservableCollection<GameObject>();
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

		private void OnObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEntities(Mobiles);
			ObjectsChanged?.Invoke(this, EventArgs.Empty);
		}

		private void UpdateEntities(IReadOnlyList<AreaEntity> entities)
		{
			foreach (var entity in entities)
			{
				entity.Area = this;
			}
		}

		public override string ToString() => $"{Name}";
	}
}
