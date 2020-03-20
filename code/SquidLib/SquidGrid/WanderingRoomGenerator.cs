using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SquidLib.SquidMath;

namespace SquidLib.SquidGrid {
    public enum DungeonRoom {
        Cave = 0,
        BoxRoom = 1,
        RoundRoom = 2,
        WalledBoxRoom = 3,
        WalledRoundRoom = 4
    }
    public class WanderingRoomGenerator {
        public bool HasGenerated { get; private set; } = false;
        public Grid<char> Dungeon { get; set; }
        public Grid<CellCategory> Environment { get; set; }
        public RNG Random { get; set; }
        private ProbabilityTable<DungeonRoom> roomTable;
        private IndexedDictionary<DungeonRoom, double> roomDict;
        private Region marked;
        private Region walled;

        private double roomWidth = 4.0;
        private double roomHeight = 4.0;

        private IndexedSet<(Coord from, Coord to)> connections;

        public WanderingRoomGenerator(int width, int height, RNG random) {
            if (random is null)
                Random = new RNG();
            else
                Random = random;
            width = Math.Max(4, width);
            height = Math.Max(4, height);
            Dungeon = new Grid<char>(width, height, '#', '#');
            Environment = new Grid<CellCategory>(width, height, CellCategory.Untouched, CellCategory.Untouched);
            roomDict = new IndexedDictionary<DungeonRoom, double>(5);
            marked = new Region(width, height);
            walled = new Region(width, height);
            roomWidth = width * 0.0175;
            roomHeight = height * 0.0175;

            connections = new IndexedSet<(Coord from, Coord to)>();
        }

        public WanderingRoomGenerator SetRoomType(DungeonRoom roomType, double chance) {
            if (chance <= 0.0)
                roomDict.Remove(roomType);
            else
                roomDict[roomType] = chance;
            return this;
        }

        private class DistanceComparer : Comparer<Coord> {
            internal Coord start;
            public override int Compare(Coord x, Coord y) {
                return Math.Sign(Radius.Circle.Radius(start, x) - Radius.Circle.Radius(start, y));
            }
        }


        private static DistanceComparer distanceSorter = new DistanceComparer();

        public void PopulateConnections() {
            uint seed = Random.NextUInt();
            IndexedSet<Coord> pointSet = new IndexedSet<Coord>();
            int limit = 2;
            for (int x = 1; x < Dungeon.Width - 1; x++) {
                for (int y = 1; y < Dungeon.Height - 1; y++) {
                    if (BlueNoise.GetSeeded(x, y, seed) < limit) {
                        pointSet.Add(Coord.Get(x, y));
                    }
                }
            }
            while (pointSet.Count <= 1) {
                limit++;
                for (int x = 1; x < Dungeon.Width - 1; x++) {
                    for (int y = 1; y < Dungeon.Height - 1; y++) {
                        if (BlueNoise.GetSeeded(x, y, seed) < limit) {
                            pointSet.Add(Coord.Get(x, y));
                        }
                    }
                }
            }
            Random.ShuffleInPlace(pointSet);
            List<Coord> sorted = new List<Coord>(pointSet);
            foreach (Coord start in pointSet) {
                if (sorted.Count <= 1) {
                    break;
                }
                distanceSorter.start = start;
                sorted.Sort(distanceSorter);
                sorted.RemoveAt(0);
                int links = Math.Min(Random.NextInt(1, 4), Random.NextInt(1, 4));
                for (int i = 0; i < sorted.Count && i < links; i++) {
                    connections.Add((start, sorted[i]));
                }
            }
        }
        private void Store() {
            foreach (Coord m in marked) {
                Dungeon.TrySet(m.X, m.Y, '.');
            }
        }
        private void markEnvironmentRoom(int x, int y) {
            Environment[x, y] = CellCategory.RoomFloor;
        }
        private void markEnvironmentCave(int x, int y) {
            if (Environment[x, y] != CellCategory.RoomFloor) Environment[x, y] = CellCategory.CaveFloor;
        }
        private void markEnvironmentCorridor(int x, int y) {
            if (Environment[x, y] != CellCategory.RoomFloor && Environment[x, y] != CellCategory.CaveFloor) Environment[x, y] = CellCategory.CaveFloor;
        }
        private bool mark(Coord position) {
            if (walled.Contains(position))
                return true;
            marked[position.X, position.Y] = true;
            return false;
        }
        private bool mark(int x, int y) {
            if (walled[x, y])
                return true;
            marked[x, y] = true;
            return false;
        }

        private void markPiercing(Coord position) {
            marked[position.X, position.Y] = true;
        }

        private void markPiercingCave(Coord position) {
            marked[position.X, position.Y] = true;
            markEnvironmentCave(position.X, position.Y);
        }

        private void markRectangle(Coord pos, int halfWidth, int halfHeight) {
            halfWidth = Math.Max(1, (int)(0.5 + halfWidth * roomWidth));
            halfHeight = Math.Max(1, (int)(0.5 + halfHeight * roomHeight));
            for (int i = pos.X - halfWidth; i <= pos.X + halfWidth; i++) {
                for (int j = pos.Y - halfHeight; j <= pos.Y + halfHeight; j++) {
                    if (!mark(i, j))
                        markEnvironmentRoom(i, j);
                }
            }
        }
        /**
         * Internal use. Marks a rectangle of points centered on pos, extending halfWidth in both x directions and
         * halfHeight in both vertical directions. Also considers the area just beyond each wall, but not corners, to be
         * a blocking wall that can only be passed by corridors and small cave openings. Marks all cells in the rectangle as
         * room floors.
         * @param pos center position to mark
         * @param halfWidth the distance from the center to extend horizontally
         * @param halfHeight the distance from the center to extend vertically
         * @return null if no points in the rectangle were blocked by walls, otherwise a Coord blocked by a wall
         */
        private void markRectangleWalled(Coord pos, int halfWidth, int halfHeight) {
            halfWidth = Math.Max(1, (int)(0.5 + halfWidth * roomWidth));
            halfHeight = Math.Max(1, (int)(0.5 + halfHeight * roomHeight));
            for (int i = pos.X - halfWidth; i <= pos.X + halfWidth; i++) {
                for (int j = pos.Y - halfHeight; j <= pos.Y + halfHeight; j++) {
                    marked[i, j] = true;
                    markEnvironmentRoom(i, j);
                }
            }
            for (int i = Math.Max(0, pos.X - halfWidth - 1); i <= Math.Min(Environment.Width - 1, pos.X + halfWidth + 1); i++) {
                for (int j = Math.Max(0, pos.Y - halfHeight - 1); j <= Math.Min(Environment.Height - 1, pos.Y + halfHeight + 1); j++) {
                    walled[i, j] = true;
                }
            }
        }

        /**
         * Internal use. Marks a circle of points centered on pos, extending out to radius in Euclidean measurement. Marks
         * all cells in the circle as room floors.
         * @param pos center position to mark
         * @param radius radius to extend in all directions from center
         * @return null if no points in the circle were blocked by walls, otherwise a Coord blocked by a wall
         */
        private void markCircle(Coord pos, int radius) {
            int high;
            radius = Math.Max(1, (int)(0.5 + radius * Math.Min(roomWidth, roomHeight)));
            for (int dx = -radius; dx <= radius; ++dx) {
                high = (int)(Math.Sqrt(radius * radius - dx * dx));
                for (int dy = -high; dy <= high; ++dy) {
                    if (!mark(pos.X + dx, pos.Y + dy))
                        markEnvironmentRoom(pos.X + dx, pos.Y + dy);
                }
            }
        }
        /**
         * Internal use. Marks a circle of points centered on pos, extending out to radius in Euclidean measurement.
         * Also considers the area just beyond each wall, but not corners, to be a blocking wall that can only be passed by
         * corridors and small cave openings. Marks all cells in the circle as room floors.
         * @param pos center position to mark
         * @param radius radius to extend in all directions from center
         * @return null if no points in the circle were blocked by walls, otherwise a Coord blocked by a wall
         */
        private void markCircleWalled(Coord pos, int radius) {
            int high;
            radius = Math.Max(1, (int)(0.5 + radius * Math.Min(roomWidth, roomHeight)));
            for (int dx = -radius; dx <= radius; ++dx) {
                high = (int)(Math.Sqrt(radius * radius - dx * dx));
                for (int dy = -high; dy <= high; ++dy) {
                    marked[pos.X + dx, pos.Y + dy] = true;
                    markEnvironmentRoom(pos.X + dx, pos.Y + dy);
                }
            }
            for (int dx = -radius; dx <= radius; ++dx) {
                high = (int)(Math.Sqrt(radius * radius - dx * dx));
                int dx2 = Math.Max(1, Math.Min(pos.X + dx, Environment.Width - 2));
                for (int dy = -high; dy <= high; ++dy) {
                    int dy2 = Math.Max(1, Math.Min(pos.Y + dy, Environment.Height - 2));

                    walled[dx2, dy2 - 1] = true;
                    walled[dx2 + 1, dy2 - 1] = true;
                    walled[dx2 - 1, dy2 - 1] = true;
                    walled[dx2, dy2] = true;
                    walled[dx2 + 1, dy2] = true;
                    walled[dx2 - 1, dy2] = true;
                    walled[dx2, dy2 + 1] = true;
                    walled[dx2 + 1, dy2 + 1] = true;
                    walled[dx2 - 1, dy2 + 1] =  true;

                }
            }
        }

        private Direction stepWobbly(Coord current, Coord target, double weight) {
            int dx = target.X - current.X;
            int dy = target.Y - current.Y;

            if (dx > 1) dx = 1;
            if (dx < -1) dx = -1;
            if (dy > 1) dy = 1;
            if (dy < -1) dy = -1;

            double r = Random.NextDouble();
            Direction dir;
            if (dx == 0 && dy == 0) {
                return Direction.None;
            } else if (dx == 0 || dy == 0) {
                int dx2 = (dx == 0) ? dx : dy, dy2 = (dx == 0) ? dy : dx;
                if (r >= (weight * 0.5)) {
                    r -= weight * 0.5;
                    if (r < weight * (1.0 / 6.0) + (1 - weight) * (1.0 / 3.0)) {
                        dx2 = -1;
                        dy2 = 0;
                    } else if (r < weight * (2.0 / 6.0) + (1 - weight) * (2.0 / 3.0)) {
                        dx2 = 1;
                        dy2 = 0;
                    } else {
                        dx2 = 0;
                        dy2 = -dy2;
                    }
                }
                dir = DirectionExtensions.GetCardinalDirection(dx2, -dy2);

            } else {
                if (r < weight * 0.5) {
                    dy = 0;
                } else if (r < weight) {
                    dx = 0;
                } else if (r < weight + (1 - weight) * 0.5) {
                    dx = -dx;
                    dy = 0;
                } else {
                    dx = 0;
                    dy = -dy;
                }
                dir = DirectionExtensions.GetCardinalDirection(dx, -dy);
            }
            if (current.X + dir.DeltaX() <= 0 || current.X + dir.DeltaX() >= Dungeon.Width - 1) {
                if (current.Y < target.Y) return Direction.Down;
                else if (current.Y >= target.Y) return Direction.Up;
            } else if (current.Y + dir.DeltaY() <= 0 || current.Y + dir.DeltaY() >= Dungeon.Height - 1) {
                if (current.X < target.X) return Direction.Right;
                else if (current.X >= target.X) return Direction.Left;
            }
            return dir;
        }

        public Grid<char> Generate() {
            if (connections.Count <= 0)
                PopulateConnections();
            if (roomDict is null)
                roomDict = new IndexedDictionary<DungeonRoom, double>();
            if (roomDict.Count <= 0)
                roomDict[DungeonRoom.BoxRoom] = 1.0;
            if (roomTable is null)
                roomTable = new ProbabilityTable<DungeonRoom>(Random, roomDict);
            else
                roomTable.Reset(roomDict);

            foreach (var (from, to) in connections) {
                Coord start = from, end = to;
                DungeonRoom ct = roomTable.GetItem();
                Direction dir;
                switch (ct) {
                    case DungeonRoom.Cave:
                    default:
                        marked[end.X, end.Y] = true;
                        if (Environment[end.X, end.Y] != CellCategory.RoomFloor) Environment[end.X, end.Y] = CellCategory.CaveFloor;
                        Store();

                        double weight = 0.75;
                        do {
                            bool centerBlocked;
                            if (!mark(start)) {
                                markEnvironmentCave(start.X, start.Y);
                                centerBlocked = true;
                            } else centerBlocked = false;
                            if (!marked.Add(start.Add(1, 0)))
                                markEnvironmentCave(start.X + 1, start.Y);
                            if (!marked.Add(start.Add(-1, 0)))
                                markEnvironmentCave(start.X - 1, start.Y);
                            if (!marked.Add(start.Add(0, 1)))
                                markEnvironmentCave(start.X, start.Y + 1);
                            if (!marked.Add(start.Add(0, -1)))
                                markEnvironmentCave(start.X, start.Y - 1);

                            if (!centerBlocked) {
                                markPiercingCave(start);
                                markPiercingCave(start.ChangeX(1));
                                markPiercingCave(start.ChangeX(-1));
                                markPiercingCave(start.ChangeY(1));
                                markPiercingCave(start.ChangeY(-1));
                                weight = 0.95;
                            }
                            dir = stepWobbly(start, end, weight);
                            start = Coord.Add(start, dir.Coord());
                        } while (dir != Direction.None);
                        break;
                    case DungeonRoom.BoxRoom:
                        markRectangle(end, Random.NextInt(1, 5), Random.NextInt(1, 5));
                        markRectangle(start, Random.NextInt(1, 4), Random.NextInt(1, 4));
                        Store();
                        dir = DirectionExtensions.GetOctalDirection(end.X - start.X, end.Y - start.Y);
                        if (dir.IsDiagonal())
                            dir = Random.NextBoolean() ? DirectionExtensions.GetCardinalDirection(dir.DeltaX(), 0)
                                    : DirectionExtensions.GetCardinalDirection(0, -dir.DeltaY());
                        while (start.X != end.X && start.Y != end.Y) {
                            markPiercing(start);
                            markEnvironmentCorridor(start.X, start.Y);
                            start += dir.Coord();
                        }
                        markRectangle(start, 1, 1);
                        dir = DirectionExtensions.GetCardinalDirection(end.X - start.X, -(end.Y - start.Y));
                        while (!(start.X == end.X && start.Y == end.Y)) {
                            markPiercing(start);
                            markEnvironmentCorridor(start.X, start.Y);
                            start += dir.Coord();
                        }
                        break;
                    case DungeonRoom.WalledBoxRoom:
                        markRectangleWalled(end, Random.NextInt(1, 5), Random.NextInt(1, 5));
                        markRectangleWalled(start, Random.NextInt(1, 4), Random.NextInt(1, 4));
                        Store();
                        dir = DirectionExtensions.GetOctalDirection(end.X - start.X, end.Y - start.Y);
                        if (DirectionExtensions.IsDiagonal(dir))
                            dir = Random.NextBoolean() ? DirectionExtensions.GetCardinalDirection(dir.DeltaX(), 0)
                                    : DirectionExtensions.GetCardinalDirection(0, -dir.DeltaY());
                        while (start.X != end.X && start.Y != end.Y) {
                            markPiercing(start);
                            markEnvironmentCorridor(start.X, start.Y);
                            start += dir.Coord();
                        }
                        markRectangleWalled(start, 1, 1);
                        dir = DirectionExtensions.GetCardinalDirection(end.X - start.X, -(end.Y - start.Y));
                        while (!(start.X == end.X && start.Y == end.Y)) {
                            markPiercing(start);
                            markEnvironmentCorridor(start.X, start.Y);
                            start += dir.Coord();
                        }
                        break;
                    case DungeonRoom.RoundRoom:
                        markCircle(end, Random.NextInt(2, 6));
                        markCircle(start, Random.NextInt(2, 6));
                        Store();
                        dir = DirectionExtensions.GetOctalDirection(end.X - start.X, end.Y - start.Y);
                        if (DirectionExtensions.IsDiagonal(dir))
                            dir = Random.NextBoolean() ? DirectionExtensions.GetCardinalDirection(dir.DeltaX(), 0)
                                    : DirectionExtensions.GetCardinalDirection(0, -dir.DeltaY());
                        while (start.X != end.X && start.Y != end.Y) {
                            markPiercing(start);
                            markEnvironmentCorridor(start.X, start.Y);
                            start += dir.Coord();
                        }
                        markCircle(start, 2);
                        dir = DirectionExtensions.GetCardinalDirection(end.X - start.X, -(end.Y - start.Y));
                        while (!(start.X == end.X && start.Y == end.Y)) {
                            markPiercing(start);
                            markEnvironmentCorridor(start.X, start.Y);
                            start += dir.Coord();
                        }
                        break;
                    case DungeonRoom.WalledRoundRoom:
                        markCircleWalled(end, Random.NextInt(2, 6));
                        markCircleWalled(start, Random.NextInt(2, 6));
                        Store();
                        dir = DirectionExtensions.GetOctalDirection(end.X - start.X, end.Y - start.Y);
                        if (DirectionExtensions.IsDiagonal(dir))
                            dir = Random.NextBoolean() ? DirectionExtensions.GetCardinalDirection(dir.DeltaX(), 0)
                                    : DirectionExtensions.GetCardinalDirection(0, -dir.DeltaY());
                        while (start.X != end.X && start.Y != end.Y) {
                            markPiercing(start);
                            markEnvironmentCorridor(start.X, start.Y);
                            start += dir.Coord();
                        }
                        markCircleWalled(start, 2);
                        dir = DirectionExtensions.GetCardinalDirection(end.X - start.X, -(end.Y - start.Y));
                        while (!(start.X == end.X && start.Y == end.Y)) {
                            markPiercing(start);
                            markEnvironmentCorridor(start.X, start.Y);
                            start += dir.Coord();
                        }
                        break;
                }
                Store();
            }

            //Store();
            marked.RefillExactly(Environment, CellCategory.Untouched);
            for (int x = 0; x < Environment.Width; x++) {
                marked.Add(Coord.Get(x, 0));
                Dungeon[x, 0] = '#';
                marked.Add(Coord.Get(x, Environment.Height - 1));
                Dungeon[x, Environment.Height - 1] = '#';
            }
            for (int y = 0; y < Environment.Height; y++) {
                marked.Add(Coord.Get(0, y));
                Dungeon[0, y] = '#';
                marked.Add(Coord.Get(Environment.Width - 1, y));
                Dungeon[Environment.Width - 1, y] = '#';
            }
            walled.RefillExactly(Environment, CellCategory.CorridorFloor).Fringe8Way(1).IntersectWith(marked);
            foreach (Coord c in walled) {
                Environment[c.X, c.Y] = CellCategory.CorridorWall;
            }
            walled.RefillExactly(Environment, CellCategory.CaveFloor).Fringe8Way(1).IntersectWith(marked);
            foreach (Coord c in walled) {
                Environment[c.X, c.Y] = CellCategory.CaveWall;
            }
            walled.RefillExactly(Environment, CellCategory.RoomFloor).Fringe8Way(1).IntersectWith(marked);
            foreach (Coord c in walled) {
                Environment[c.X, c.Y] = CellCategory.RoomWall;
            }
            HasGenerated = true;

            return Dungeon;
        }
    }
}
