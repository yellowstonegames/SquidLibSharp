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
            roomWidth = width / 50.0;
            roomHeight = height / 50.0;

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
            for (int x = 0; x < Dungeon.Width; x++) {
                for (int y = 0; y < Dungeon.Height; y++) {
                    if (BlueNoise.GetSeeded(x, y, seed) < 3)
                        pointSet.Add(Coord.Get(x, y));
                }
            }
            if (pointSet.Count <= 1) {
                pointSet.Add(Coord.Get(Random.NextSignedInt(Dungeon.Width), Random.NextSignedInt(Dungeon.Height)));
                pointSet.Add(Coord.Get(Random.NextSignedInt(Dungeon.Width), Random.NextSignedInt(Dungeon.Height)));
                pointSet.Add(Coord.Get(Random.NextSignedInt(Dungeon.Width), Random.NextSignedInt(Dungeon.Height)));
                pointSet.Add(Coord.Get(Random.NextSignedInt(Dungeon.Width), Random.NextSignedInt(Dungeon.Height)));
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
                Dungeon[m.X, m.Y] = '.';
            }
        }
        private void markEnvironmentCave(int x, int y) {
            if (Environment[x, y] != CellCategory.RoomFloor) Environment[x, y] = CellCategory.CaveFloor;
        }
        private bool mark(Coord position) {
            if (walled.Contains(position))
                return true;
            marked.Add(position);
            return false;
        }

        private void markPiercingCave(Coord position) {
            marked.Add(position);
            markEnvironmentCave(position.X, position.Y);

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
                    if (r < weight * (1.0 / 6) + (1 - weight) * (1.0 / 3)) {
                        dx2 = -1;
                        dy2 = 0;
                    } else if (r < weight * (2.0 / 6) + (1 - weight) * (2.0 / 3)) {
                        dx2 = 1;
                        dy2 = 0;
                    } else {
                        dx2 = 0;
                        dy2 *= -1;
                    }
                }
                dir = DirectionExtensions.GetCardinalDirection(dx2, -dy2);

            } else {
                if (r < weight * 0.5) {
                    dy = 0;
                } else if (r < weight) {
                    dx = 0;
                } else if (r < weight + (1 - weight) * 0.5) {
                    dx *= -1;
                    dy = 0;
                } else {
                    dx = 0;
                    dy *= -1;
                }
                dir = DirectionExtensions.GetCardinalDirection(dx, -dy);
            }
            if (current.X + dir.DeltaX() <= 0 || current.X + dir.DeltaX() >= Dungeon.Width - 1) {
                if (current.Y < target.Y) return Direction.Down;
                else if (current.Y > target.Y) return Direction.Up;
            } else if (current.Y + dir.DeltaY() <= 0 || current.Y + dir.DeltaY() >= Dungeon.Height - 1) {
                if (current.X < target.X) return Direction.Right;
                else if (current.X > target.X) return Direction.Left;
            }
            return dir;
        }

        public Grid<char> Generate() {
            if (connections.Count <= 0)
                PopulateConnections();
            if (roomDict is null)
                roomDict = new IndexedDictionary<DungeonRoom, double>();
            if (roomDict.Count <= 0)
                roomDict[DungeonRoom.Cave] = 1.0;
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
                        marked.Add(end);
                        if (Environment[end.X, end.Y] != CellCategory.RoomFloor) Environment[end.X, end.Y] = CellCategory.CaveFloor;
                        Store();

                        double weight = 0.75;
                        do {
                            bool centerBlocked;
                            if (!mark(start)) {
                                markEnvironmentCave(start.X, start.Y);
                                centerBlocked = true;
                            } else centerBlocked = false;
                            if (!marked.Add(start.ChangeX(1)))
                                markEnvironmentCave(start.X + 1, start.Y);
                            if (!marked.Add(start.ChangeX(-1)))
                                markEnvironmentCave(start.X - 1, start.Y);
                            if (!marked.Add(start.ChangeY(1)))
                                markEnvironmentCave(start.X, start.Y + 1);
                            if (!marked.Add(start.ChangeY(-1)))
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
                        //case BOX:
                        //    markRectangle(end, rng.between(1, 5), rng.between(1, 5));
                        //    markRectangle(start, rng.between(1, 4), rng.between(1, 4));
                        //    store();
                        //    dir = Direction.getDirection(end.x - start.x, end.y - start.y);
                        //    if (dir.isDiagonal())
                        //        dir = rng.nextBoolean() ? Direction.getCardinalDirection(dir.deltaX, 0)
                        //                : Direction.getCardinalDirection(0, -dir.deltaY);
                        //    while (start.x != end.x && start.y != end.y) {
                        //        markPiercing(start);
                        //        markEnvironmentCorridor(start.x, start.y);
                        //        start = start.translate(dir);
                        //    }
                        //    markRectangle(start, 1, 1);
                        //    dir = Direction.getCardinalDirection(end.x - start.x, -(end.y - start.y));
                        //    while (!(start.x == end.x && start.y == end.y)) {
                        //        markPiercing(start);
                        //        markEnvironmentCorridor(start.x, start.y);
                        //        start = start.translate(dir);
                        //    }
                        //    break;
                        //case BOX_WALLED:
                        //    markRectangleWalled(end, rng.between(1, 5), rng.between(1, 5));
                        //    markRectangleWalled(start, rng.between(1, 4), rng.between(1, 4));
                        //    store();
                        //    dir = Direction.getDirection(end.x - start.x, end.y - start.y);
                        //    if (dir.isDiagonal())
                        //        dir = rng.nextBoolean() ? Direction.getCardinalDirection(dir.deltaX, 0)
                        //                : Direction.getCardinalDirection(0, -dir.deltaY);
                        //    while (start.x != end.x && start.y != end.y) {
                        //        markPiercing(start);
                        //        markEnvironmentCorridor(start.x, start.y);
                        //        start = start.translate(dir);
                        //    }
                        //    markRectangleWalled(start, 1, 1);
                        //    dir = Direction.getCardinalDirection(end.x - start.x, -(end.y - start.y));
                        //    while (!(start.x == end.x && start.y == end.y)) {
                        //        markPiercing(start);
                        //        markEnvironmentCorridor(start.x, start.y);
                        //        start = start.translate(dir);
                        //    }
                        //    break;
                        //case ROUND:
                        //    markCircle(end, rng.between(2, 6));
                        //    markCircle(start, rng.between(2, 6));
                        //    store();
                        //    dir = Direction.getDirection(end.x - start.x, end.y - start.y);
                        //    if (dir.isDiagonal())
                        //        dir = rng.nextBoolean() ? Direction.getCardinalDirection(dir.deltaX, 0)
                        //                : Direction.getCardinalDirection(0, -dir.deltaY);
                        //    while (start.x != end.x && start.y != end.y) {
                        //        markPiercing(start);
                        //        markEnvironmentCorridor(start.x, start.y);
                        //        start = start.translate(dir);
                        //    }
                        //    markCircle(start, 2);
                        //    dir = Direction.getCardinalDirection(end.x - start.x, -(end.y - start.y));
                        //    while (!(start.x == end.x && start.y == end.y)) {
                        //        markPiercing(start);
                        //        markEnvironmentCorridor(start.x, start.y);
                        //        start = start.translate(dir);
                        //    }
                        //    break;
                        //case ROUND_WALLED:
                        //    markCircleWalled(end, rng.between(2, 6));
                        //    markCircleWalled(start, rng.between(2, 6));
                        //    store();
                        //    dir = Direction.getDirection(end.x - start.x, end.y - start.y);
                        //    if (dir.isDiagonal())
                        //        dir = rng.nextBoolean() ? Direction.getCardinalDirection(dir.deltaX, 0)
                        //                : Direction.getCardinalDirection(0, -dir.deltaY);
                        //    while (start.x != end.x && start.y != end.y) {
                        //        markPiercing(start);
                        //        markEnvironmentCorridor(start.x, start.y);
                        //        start = start.translate(dir);
                        //    }
                        //    markCircleWalled(start, 2);
                        //    dir = Direction.getCardinalDirection(end.x - start.x, -(end.y - start.y));
                        //    while (!(start.x == end.x && start.y == end.y)) {
                        //        markPiercing(start);
                        //        markEnvironmentCorridor(start.x, start.y);
                        //        start = start.translate(dir);
                        //    }
                        //    break;
                }
                Store();
            }

            Store();
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
