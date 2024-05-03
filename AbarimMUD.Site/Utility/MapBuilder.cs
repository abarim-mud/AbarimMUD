using AbarimMUD.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace AbarimMUD.Site.Utility.Utility
{
	public static class MapBuilderExtensions
	{
		public static Point getExitTypeDelta(this Direction exitType)
		{
			switch (exitType)
			{
				case Direction.East:
					return new Point(1, 0);
				case Direction.West:
					return new Point(-1, 0);
				case Direction.North:
					return new Point(0, -1);
				case Direction.South:
					return new Point(0, 1);
				case Direction.Up:
					return new Point(0, -1);
				case Direction.Down:
					return new Point(0, 1);
			}

			throw new Exception($"Unknown direction {exitType}");
		}
	}

	public class MapBuilder
	{
		private const int RoomHeight = 32;
		private const int TextPadding = 8;
		private static readonly Point RoomSpace = new Point(16, 8);
		private readonly List<List<Room>> _roomsChains = new List<List<Room>>();

		private List<Room> FindChainForRoom(Room room)
		{
			List<Room> result = null;

			foreach (var chain in _roomsChains)
			{
				foreach (var cell in chain)
				{
					if (room == cell)
					{
						// Found
						result = chain;
						break;
					}
				}
			}

			return result;
		}

		private void ProcessRoomChain(Room room)
		{
			// Find the current chain of room
			var currentChain = FindChainForRoom(room);

			if (currentChain == null)
			{
				// Create new chain
				currentChain = new List<Room>
				{

					room
				};

				_roomsChains.Add(currentChain);
			}

			// Now process the exits
			foreach (Direction exitType in Enum.GetValues(typeof(Direction)))
			{
				var exit = (from e in room.Exits where e.Direction == exitType select e).FirstOrDefault();
				if (exit == null || exit.TargetRoom == null)
				{
					continue;
				}
				var exitRoom = exit.TargetRoom;

				// Find chain of the exit room
				var exitRoomChain = FindChainForRoom(exitRoom);
				if (exitRoomChain == currentChain)
				{
					// Already in chain
					continue;
				}

				if (exitRoomChain == null)
				{
					// Simply add to the current chain
					currentChain.Add(exitRoom);
					continue;
				}

				// Merge chains into the current room
				for (int i = 0; i < exitRoomChain.Count; ++i)
				{
					currentChain.Add(exitRoomChain[i]);
				}

				exitRoomChain.Clear();
			}
		}

		private void AssignCoordinates(List<Room> chain, Room room)
		{
			var pos = (Point)room.Tag;
			foreach (Direction exitType in Enum.GetValues(typeof(Direction)))
			{
				var exit = (from e in room.Exits where e.Direction == exitType select e).FirstOrDefault();
				if (exit == null || exit.TargetRoom == null || exit.TargetRoom.Tag != null)
				{
					continue;
				}

				var exitRoom = exit.TargetRoom;
				Point delta = exitType.getExitTypeDelta();

				var newPos = new Point(pos.X + delta.X, pos.Y + delta.Y);
				exitRoom.Tag = newPos;

				AssignCoordinates(chain, exitRoom);
			}

			// Now check if any other room has connection to this room
			foreach (var otherRoom in chain)
			{
				if (otherRoom == room || otherRoom.Tag != null)
				{
					continue;
				}

				foreach (Direction et in Enum.GetValues(typeof(Direction)))
				{
					var exit = (from e in room.Exits where e.Direction == et select e).FirstOrDefault();
					if (exit == null || exit.TargetRoom == null)
					{
						continue;
					}

					var exitRoom = exit.TargetRoom;
					if (exitRoom == room)
					{
						// Connected
						Point delta = et.getExitTypeDelta();

						var newPos = new Point(pos.X - delta.X, pos.Y - delta.Y);
						otherRoom.Tag = newPos;

						break;
					}
				}
			}
		}

		private Room GetRoomByPoint(Point p)
		{
			foreach (var chain in _roomsChains)
			{
				foreach (var room in chain)
				{
					var pos = (Point)room.Tag;

					if (pos == p)
					{
						return room;
					}
				}
			}

			return null;
		}

		public byte[] Build(Area area)
		{
			_roomsChains.Clear();

			// Build up room chains
			foreach (var room in area.Rooms)
			{
				ProcessRoomChain(room);
			}

			// Remove empty chains
			_roomsChains.RemoveAll(chain => chain.Count == 0);

			// Sort by size
			_roomsChains.Sort((x, y) =>
			{
				if (x.Count > y.Count)
				{
					return -1;
				}
				else if (x.Count == y.Count)
				{
					return 0;
				}

				return 1;
			});

			// Clear room tags
			foreach (var chain in _roomsChains)
			{
				foreach (var room in chain)
				{
					room.Tag = null;
				}
			}

			// Next run assign every room it's coordinate
			int y = 0;
			foreach (var chain in _roomsChains)
			{
				// First room in chain is 0, 0
				var point = new Point(0, y);

				chain[0].Tag = point;
				AssignCoordinates(chain, chain[0]);

				// Sort by y,x
				chain.Sort((x, y) =>
				{
					var a = (Point)x.Tag;
					var b = (Point)y.Tag;

					if (a.Y < b.Y)
					{
						return -1;
					}
					else if (a.Y == b.Y)
					{
						if (a.X < b.X)
						{
							return -1;
						}
						else if (a.X == b.X)
						{
							return 0;
						}
					}

					return 1;
				});

				// Determine minimum point
				var min = new Point();
				var minSet = false;
				Point pos;
				foreach (var room in chain)
				{
					pos = (Point)room.Tag;
					if (!minSet)
					{
						min = new Point(pos.X, pos.Y);
						minSet = true;
					}

					if (pos.X < min.X)
					{
						min.X = pos.X;
					}

					if (pos.Y < min.Y)
					{
						min.Y = pos.Y;
					}
				}

				// Shift everything so it begins from 0,0
				Point shift = new Point(min.X < 0 ? -min.X : 0, min.Y < 0 ? -min.Y : 0);
				foreach (var room in chain)
				{
					pos = (Point)room.Tag;

					pos.X += shift.X;
					pos.Y += shift.Y;
					room.Tag = pos;
				}

				//
				pos = (Point)chain[chain.Count - 1].Tag;
				y = pos.Y + 1;
			}

			// Determine size
			Point max = new Point(0, 0);
			foreach (var chain in _roomsChains)
			{
				foreach (var room in chain)
				{
					var pos = (Point)room.Tag;
					if (pos.X > max.X)
					{
						max.X = pos.X;
					}

					if (pos.Y > max.Y)
					{
						max.Y = pos.Y;
					}
				}
			}

			byte[] imageBytes = null;

			using (SKPaint paint = new SKPaint())
			{
				paint.Color = SKColors.Black;
				paint.IsAntialias = true;
				paint.Style = SKPaintStyle.Stroke;
				paint.TextAlign = SKTextAlign.Center;
				
				// First grid run - determine cells width
				var cellsWidths = new int[max.X];
				for (var x = 0; x < max.X; ++x)
				{
					for (y = 0; y < max.Y; ++y)
					{
						var room = GetRoomByPoint(new Point(x, y));
						if (room == null)
						{
							continue;
						}

						var sz = (int)(paint.MeasureText(room.Name) + TextPadding * 2 + 0.5f);
						if (sz > cellsWidths[x])
						{
							cellsWidths[x] = sz;
						}
					}
				}


				// Second run - draw the map
				var imageWidth = 0;
				for(var i = 0; i < cellsWidths.Length; ++i)
				{
					imageWidth += cellsWidths[i];
				}

				imageWidth += (max.X + 1) * RoomSpace.X;

				SKImageInfo imageInfo = new SKImageInfo(imageWidth,
														max.Y * RoomHeight + (max.Y + 1) * RoomSpace.Y);


				using (SKSurface surface = SKSurface.Create(imageInfo))
				{
					SKCanvas canvas = surface.Canvas;

					var screenX = RoomSpace.X;
					for (var x = 0; x < max.X; ++x)
					{
						for (y = 0; y < max.Y; ++y)
						{
							var room = GetRoomByPoint(new Point(x, y));
							if (room == null)
							{
								continue;
							}

							var screenPos = new Point(screenX,
													  y * RoomHeight + (y + 1) * RoomSpace.Y);
							paint.StrokeWidth = 2;
							canvas.DrawRect(screenPos.X, screenPos.Y, cellsWidths[x], RoomHeight, paint);

							paint.StrokeWidth = 1;
							canvas.DrawText(room.Name, screenPos.X + cellsWidths[x] / 2, screenPos.Y + RoomHeight / 2, paint);
						}

						screenX += cellsWidths[x];
						screenX += RoomSpace.X;
					}
					using (SKImage image = surface.Snapshot())
					using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
					using (MemoryStream mStream = new MemoryStream(data.ToArray()))
					{
						imageBytes = data.ToArray();
					}
				}

			}

			return imageBytes;
		}
	}
}
