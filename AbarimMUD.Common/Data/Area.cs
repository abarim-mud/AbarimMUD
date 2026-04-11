using AbarimMUD.Attributes;
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


		[OLCAlias("owner")]
		public string OwnerName { get; set; }

		[OLCIgnore]
		[JsonIgnore]
		public Character Owner { get; set; }

		public int StartId { get; set; }
		public int IdCount { get; set; } = 1000;

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

		public float RespawnTimeInMinutes { get; set; } = 30;

		[JsonIgnore]
		public int NextRoomId
		{
			get
			{
				if (Rooms.Count == 0)
				{
					return StartId;
				}

				var newId = StartId;
				foreach (var r in Rooms)
				{
					if (r.Id > newId)
					{
						newId = r.Id;
					}
				}

				++newId;

				return newId;
			}
		}

		public int NextMobileId
		{
			get
			{
				if (Mobiles.Count == 0)
				{
					return StartId;
				}

				var newId = StartId;
				foreach (var r in Mobiles)
				{
					if (r.Id > newId)
					{
						newId = r.Id;
					}
				}

				++newId;

				return newId;
			}
		}

		[JsonIgnore]
		public DateTime LastSpawn { get; set; }

		public event EventHandler RoomsChanged;
		public event EventHandler MobilesChanged;

		public Area()
		{
			Rooms = new ObservableCollection<Room>();
			Mobiles = new ObservableCollection<Mobile>();
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

		private void UpdateEntities(IEnumerable<IAreaEntity> entities)
		{
			foreach (var r in entities)
			{
				r.Area = this;
			}
		}

		public void Create() => Storage.Create(this);
		public void Save()
		{
			foreach (var mobile in Mobiles)
			{
				mobile.Experience = mobile.CalculateXpAward();
			}

			Storage.Save(this);
		}

		public override string ToString() => $"{MinimumLevel}-{MaximumLevel} {Builders} {Name}";

		public static Area GetAreaByName(string name) => Storage.GetByKey(name);
		public static Area EnsureAreaByName(string name) => Storage.EnsureByKey(name);
		public static Area LookupArea(string name) => Storage.Lookup(name);
	}
}