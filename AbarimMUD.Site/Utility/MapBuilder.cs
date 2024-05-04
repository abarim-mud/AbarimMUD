using AbarimMUD.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AbarimMUD.Site.Utility.Utility
{
	public static class MapBuilderExtensions
	{
		public static Point GetDelta(this Direction direction)
		{
			switch (direction)
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
					return new Point(1, -1);
				case Direction.Down:
					return new Point(-1, 1);
			}

			throw new Exception($"Unknown direction {direction}");
		}
	}

	public class MapBuilder
	{
		private const int RoomHeight = 32;
		private const int TextPadding = 8;
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

		private void PushRoom(Room room, Direction dir)
		{
			var roomPos = (Point)room.Tag;
			switch (dir)
			{
				case Direction.North:
					--roomPos.Y;
					break;
				case Direction.East:
					++roomPos.X;
					break;
				case Direction.South:
					++roomPos.Y;
					break;
				case Direction.West:
					--roomPos.X;
					break;
				case Direction.Up:
					++roomPos.X;
					--roomPos.Y;
					break;
				case Direction.Down:
					--roomPos.X;
					++roomPos.Y;
					break;
			}

			room.Tag = roomPos;
		}

		private void PushRooms(Room firstRoom, Direction dir, bool strongPush)
		{
			var pos = (Point)firstRoom.Tag;

			// Push other rooms
			foreach(var room in _area.Rooms)
			{ 
				// Push this room
				if (room.Tag == null)
				{
					continue;
				}

				var roomPos = (Point)room.Tag;
				var push = false;

				if (room == firstRoom)
				{
					push = true;
				}
				else if (strongPush)
				{
					switch (dir)
					{
						case Direction.North:
							push = roomPos.Y <= pos.Y;
							break;
						case Direction.East:
							push = roomPos.X >= pos.X;
							break;
						case Direction.South:
							push = roomPos.Y >= pos.Y;
							break;
						case Direction.West:
							push = roomPos.X <= pos.X;
							break;
						case Direction.Up:
							push = roomPos.X >= pos.X && roomPos.Y <= pos.Y;
							break;
						case Direction.Down:
							push = roomPos.X <= pos.X && roomPos.Y >= pos.Y;
							break;
					}
				}
				else
				{
					switch (dir)
					{
						case Direction.North:
							push = roomPos.Y < pos.Y;
							break;
						case Direction.East:
							push = roomPos.X > pos.X;
							break;
						case Direction.South:
							push = roomPos.Y > pos.Y;
							break;
						case Direction.West:
							push = roomPos.X < pos.X;
							break;
						case Direction.Up:
							push = roomPos.X > pos.X && roomPos.Y < pos.Y;
							break;
						case Direction.Down:
							push = roomPos.X < pos.X && roomPos.Y > pos.Y;
							break;
					}
				}

				if (push)
				{
					PushRoom(room, dir);
				}
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
				foreach (var exit in room.Exits)
				{
					if (exit.TargetRoom == null || exit.TargetRoom.AreaId != _area.Id || exit.TargetRoom.Tag != null)
					{
						continue;
					}

					var delta = exit.Direction.GetDelta();
					var newPos = new Point(pos.X + delta.X, pos.Y + delta.Y);

					while(true)
					{
						// Check if this pos is used already
						var intersectRoom = (from r in _area.Rooms where r != exit.TargetRoom && r.Tag != null && ((Point)r.Tag) == newPos select r).FirstOrDefault();
						if (intersectRoom == null)
						{
							break;
						}

						switch (exit.Direction)
						{
							case Direction.North:
								PushRooms(intersectRoom, Direction.North, true);
								break;
							case Direction.East:
								{
									var horizontalConnection = (from ex in intersectRoom.Exits
																where
																(ex.Direction == Direction.West || ex.Direction == Direction.East) &&
																ex.TargetRoom != null && ex.TargetRoom.AreaId == _area.Id && ex.TargetRoom.Tag != null
																select ex).FirstOrDefault();
									if (horizontalConnection == null)
									{
										// We arent breaking any horizontal line
										// Hence doing vertical push
										PushRooms(intersectRoom, Direction.North, false);
									}
									else
									{
										PushRooms(intersectRoom, Direction.East, true);
									}
								}
								break;
							case Direction.South:
								PushRooms(intersectRoom, Direction.South, true);
								break;
							case Direction.West:
								{
									var horizontalConnection = (from ex in intersectRoom.Exits
																where
																(ex.Direction == Direction.West || ex.Direction == Direction.East) &&
																ex.TargetRoom != null && ex.TargetRoom.AreaId == _area.Id && ex.TargetRoom.Tag != null
																select ex).FirstOrDefault();
									if (horizontalConnection == null)
									{
										// We arent breaking any horizontal line
										// Hence doing vertical push
										PushRooms(intersectRoom, Direction.North, false);
									}
									else
									{
										PushRooms(intersectRoom, Direction.West, true);
									}
								}
								break;
							case Direction.Up:
								PushRooms(intersectRoom, Direction.North, true);
								break;
							case Direction.Down:
								PushRooms(intersectRoom, Direction.South, true);
								break;
						}
					}

					exit.TargetRoom.Tag = newPos;
					toProcess.Add(exit.TargetRoom);
				}

				++step;
			}

			// Next run: if it is possible to place interconnected rooms with single exits next to each other, do it
			foreach (var room in _area.Rooms)
			{
				if (room.Tag == null)
				{
					continue;
				}

				pos = (Point)room.Tag;

				foreach (var exit in room.Exits)
				{
					if (exit.TargetRoom == null || exit.TargetRoom.AreaId != _area.Id || exit.TargetRoom.Tag == null)
					{
						continue;
					}

					var targetExitsCount = (from ex in exit.TargetRoom.Exits where ex.TargetRoom != null && ex.TargetRoom.AreaId == _area.Id select ex).Count();
					if (targetExitsCount > 1)
					{
						continue;
					}

					var targetPos = (Point)exit.TargetRoom.Tag;
					var delta = exit.Direction.GetDelta();
					var desiredPos = new Point(pos.X + delta.X, pos.Y + delta.Y);

					if (targetPos == desiredPos)
					{
						// Target room is already next to the source
						continue;
					}

					// Check if the spot is free
					var usedByRoom = GetRoomByPoint(desiredPos);
					if (usedByRoom != null)
					{
						// Spot is occupied
						continue;
					}

					// Place target room next to source
					exit.TargetRoom.Tag = desiredPos;
				}
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

								var sourceScreen = GetConnectionPoint(rect, roomExit.Direction);
								var targetScreen = GetConnectionPoint(targetRect, roomExit.Direction.GetOppositeDirection());

								var delta = roomExit.Direction.GetDelta();
								//if (targetPos == new Point(x + delta.X, y + delta.Y))
								{
									canvas.DrawLine(sourceScreen.X, sourceScreen.Y, targetScreen.X, targetScreen.Y, paint);
								}
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

		private static Point GetConnectionPoint(Rectangle rect, Direction direction)
		{
			switch (direction)
			{
				case Direction.North:
					return new Point(rect.X + rect.Width / 2, rect.Y);
				case Direction.East:
					return new Point(rect.Right, rect.Y + rect.Height / 2);
				case Direction.South:
					return new Point(rect.X + rect.Width / 2, rect.Bottom);
				case Direction.West:
					return new Point(rect.Left, rect.Y + rect.Height / 2);
				case Direction.Up:
					return new Point(rect.Right, rect.Y);
				case Direction.Down:
					return new Point(rect.X, rect.Bottom);
			}

			throw new Exception($"Unknown direction {direction}");
		}
	}
}
