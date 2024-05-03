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
		private static readonly Direction[] Directions = new Direction[]
		{
			Direction.West, Direction.North, Direction.South, Direction.East,
		};

		private static readonly Point RoomSpace = new Point(32, 32);

		private Area _area;
		private int[] cellsWidths;

		private Room GetRoomByPoint(Point p)
		{
			foreach (var room in _area.Rooms)
			{
				if (room.Tag == null)
				{
					continue;
				}

				var pos = (Point)room.Tag;

				if (pos == p)
				{
					return room;
				}
			}

			return null;
		}

		private void PushRooms(Point pos, Direction dir)
		{
			foreach (var room in _area.Rooms)
			{
				if (room.Tag == null)
				{
					continue;
				}

				var roomPos = (Point)room.Tag;

				switch (dir)
				{
					case Direction.North:
						if (roomPos.Y <= pos.Y)
						{
							--roomPos.Y;
						}
						break;
					case Direction.East:
						if (roomPos.X >= pos.X)
						{
							++roomPos.X;
						}
						break;
					case Direction.South:
						if (roomPos.Y >= pos.Y)
						{
							++roomPos.Y;
						}
						break;
					case Direction.West:
						if (roomPos.X <= pos.X)
						{
							--roomPos.X;
						}
						break;
					case Direction.Up:
						break;
					case Direction.Down:
						break;
				}

				room.Tag = roomPos;
			}
		}

		public byte[] Build(Area area, int? maxSteps = null)
		{
			_area = area;

			var toProcess = new List<Room>();

			area.Rooms[0].Tag = new Point(0, 0);
			toProcess.Add(area.Rooms[0]);

			var step = 1;

			Point pos;
			while (toProcess.Count > 0 && (maxSteps == null || maxSteps.Value > step))
			{
				var room = toProcess[0];
				toProcess.RemoveAt(0);

				pos = (Point)(room.Tag);
				foreach (var direction in Directions)
				{
					var exit = (from e in room.Exits where e.Direction == direction select e).FirstOrDefault();
					if (exit == null || exit.TargetRoom == null || exit.TargetRoom.Tag != null)
					{
						continue;
					}

					var delta = direction.getExitTypeDelta();
					var newPos = new Point(pos.X + delta.X, pos.Y + delta.Y);

					// Check if this pos is used already
					foreach (var room2 in _area.Rooms)
					{
						if (exit.TargetRoom == room2 || room2.Tag == null)
						{
							continue;
						}

						if ((Point)room2.Tag == newPos)
						{
							// It is used
							PushRooms(newPos, direction);
						}
					}

					exit.TargetRoom.Tag = newPos;
					toProcess.Add(exit.TargetRoom);
				}

				++step;
			}

			// Determine minimum point
			var min = new Point();
			var minSet = false;
			foreach (var room in _area.Rooms)
			{
				if (room.Tag == null)
				{
					continue;
				}

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
			foreach (var room in _area.Rooms)
			{
				if (room.Tag == null)
				{
					continue;
				}

				pos = (Point)room.Tag;

				pos.X += shift.X;
				pos.Y += shift.Y;
				room.Tag = pos;
			}

			// Determine size
			Point max = new Point(0, 0);
			foreach (var room in _area.Rooms)
			{
				if (room.Tag == null)
				{
					continue;
				}

				pos = (Point)room.Tag;
				if (pos.X > max.X)
				{
					max.X = pos.X;
				}

				if (pos.Y > max.Y)
				{
					max.Y = pos.Y;
				}
			}

			++max.X;
			++max.Y;

			byte[] imageBytes = null;

			using (SKPaint paint = new SKPaint())
			{
				paint.Color = SKColors.Black;
				paint.IsAntialias = true;
				paint.Style = SKPaintStyle.Stroke;
				paint.TextAlign = SKTextAlign.Center;

				// First grid run - determine cells width
				cellsWidths = new int[max.X];
				for (var x = 0; x < max.X; ++x)
				{
					for (var y = 0; y < max.Y; ++y)
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
				for (var i = 0; i < cellsWidths.Length; ++i)
				{
					imageWidth += cellsWidths[i];
				}

				imageWidth += (max.X + 1) * RoomSpace.X;

				SKImageInfo imageInfo = new SKImageInfo(imageWidth,
														max.Y * RoomHeight + (max.Y + 1) * RoomSpace.Y);


				using (SKSurface surface = SKSurface.Create(imageInfo))
				{
					SKCanvas canvas = surface.Canvas;

					for (var x = 0; x < max.X; ++x)
					{
						for (var y = 0; y < max.Y; ++y)
						{
							var room = GetRoomByPoint(new Point(x, y));
							if (room == null)
							{
								continue;
							}

							// Draw room
							var rect = GetRoomRect(new Point(x, y));
							paint.StrokeWidth = 2;
							canvas.DrawRect(rect.X, rect.Y, cellsWidths[x], RoomHeight, paint);

							// Draw connections
							foreach (var roomExit in room.Exits)
							{
								if (roomExit.TargetRoom == null || roomExit.TargetRoom.Tag == null)
								{
									continue;
								}

								var targetPos = (Point)roomExit.TargetRoom.Tag;
								var targetRect = GetRoomRect(targetPos);

								var sourceScreen = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
								var targetScreen = new Point(targetRect.X + targetRect.Width / 2,
									targetRect.Y + targetRect.Height / 2);

								if (targetPos.X > x && targetPos.Y == y)
								{
									// To the right
									sourceScreen = new Point(rect.Right, rect.Y + rect.Height / 2);
									targetScreen = new Point(targetRect.X, targetRect.Y + targetRect.Height / 2);
								}
								else if (targetPos.X < x && targetPos.Y == y)
								{
									// To the left
									sourceScreen = new Point(rect.Left, rect.Y + rect.Height / 2);
									targetScreen = new Point(targetRect.Right, targetRect.Y + targetRect.Height / 2);
								}
								else if (targetPos.X == x && targetPos.Y > y)
								{
									// To the bottom
									sourceScreen = new Point(rect.X + rect.Width / 2, rect.Bottom);
									targetScreen = new Point(targetRect.X + targetRect.Width / 2, targetRect.Y);
								}
								else if (targetPos.X == x && targetPos.Y < y)
								{
									// To the top
									sourceScreen = new Point(rect.X + rect.Width / 2, rect.Y);
									targetScreen = new Point(targetRect.X + targetRect.Width / 2, targetRect.Bottom);
								}

								canvas.DrawLine(sourceScreen.X, sourceScreen.Y, targetScreen.X, targetScreen.Y, paint);
							}

							paint.StrokeWidth = 1;
							canvas.DrawText(room.Name, rect.X + rect.Width / 2, rect.Y + rect.Height / 2, paint);

						}
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

		private Rectangle GetRoomRect(Point pos)
		{
			var screenX = RoomSpace.X;
			for (var x = 0; x < pos.X; ++x)
			{
				screenX += cellsWidths[x];
				screenX += RoomSpace.X;
			}

			return new Rectangle(screenX, pos.Y * RoomHeight + (pos.Y + 1) * RoomSpace.Y, cellsWidths[pos.X], RoomHeight);
		}
	}
}
