using System;
using System.Collections.Generic;
using System.Text;
using SquidLib.SquidMath;

namespace SquidLib.SquidGrid {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Current name indicates correct level of specificity.")]
    public class Region : IndexedSet<Coord>, ICollection<Coord>, IOrdered<Coord>, IEnumerable<Coord> {
        private static IndexedSet<Coord> working = new IndexedSet<Coord>();
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Region(int width, int height) {
            Width = width;
            Height = height;
        }
        public Region() : this(80, 24) {
        }

        public static Region FromChars(Grid<char> grid, char match) {
            if (grid == null)
                return new Region(1, 1);
            Region region = new Region(grid.Width, grid.Height);
            for (int x = 0; x < grid.Width; x++) {
                for (int y = 0; y < grid.Height; y++) {
                    if (grid[x, y] == match)
                        region.Add(Coord.Get(x, y));
                }
            }
            return region;
        }
        public bool this[int x, int y] {
            get {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return false;
                return Contains(Coord.Get(x, y));
            }
            set {
                if (x >= 0 && x < Width && y >= 0 && y < Height) {
                    if (value)
                        Remove(Coord.Get(x, y));
                    else
                        Add(Coord.Get(x, y));
                }
            }
        }

        public Region Expand(int distance) {
            working.ResetTo(this);
            foreach (Coord point in working.Ordering) {
                for (int x = Math.Max(0, point.X - distance), xd = Math.Min(Width - 1, point.X + distance); x < xd; x++) {
                    for (int y = Math.Max(0, point.Y - distance), yd = Math.Min(Height - 1, point.Y + distance); y < yd; y++) {
                        if (Math.Abs(point.X - x) + Math.Abs(point.Y - y) <= distance)
                            Add(Coord.Get(x, y));
                    }
                }
            }
            return this;
        }
        public Region Expand8Way(int distance) {
            working.ResetTo(this);
            foreach (Coord point in working.Ordering) {
                for (int x = Math.Max(0, point.X - distance), xd = Math.Min(Width - 1, point.X + distance); x < xd; x++) {
                    for (int y = Math.Max(0, point.Y - distance), yd = Math.Min(Height - 1, point.Y + distance); y < yd; y++) {
                        Add(Coord.Get(x, y));
                    }
                }
            }
            return this;
        }
        public Region Fringe(int distance) {
            Expand(distance).ExceptWith(working);
            return this;
        }
        public Region Fringe8Way(int distance) {
            Expand8Way(distance).ExceptWith(working);
            return this;
        }
        public Region Retract(int distance) {
            working.ResetTo(this);
            foreach (Coord point in working.Ordering) {
                for (int x = Math.Max(0, point.X - distance), xd = Math.Min(Width - 1, point.X + distance); x < xd; x++) {
                    for (int y = Math.Max(0, point.Y - distance), yd = Math.Min(Height - 1, point.Y + distance); y < yd; y++) {
                        if (Math.Abs(point.X - x) + Math.Abs(point.Y - y) <= distance && !working.Contains(Coord.Get(x, y))) {
                            Remove(point);
                            goto NEXT;
                        }
                    }
                }
            NEXT:
                continue;
            }
            return this;
        }
        public Region Retract8Way(int distance) {
            working.ResetTo(this);
            foreach (Coord point in working.Ordering) {
                for (int x = Math.Max(0, point.X - distance), xd = Math.Min(Width - 1, point.X + distance); x < xd; x++) {
                    for (int y = Math.Max(0, point.Y - distance), yd = Math.Min(Height - 1, point.Y + distance); y < yd; y++) {
                        if(!working.Contains(Coord.Get(x, y))) {
                            Remove(point);
                            goto NEXT;
                        }
                    }
                }
            NEXT:
                continue;
            }
            return this;
        }
        public Region Surface(int distance) {
            Retract(distance).SymmetricExceptWith(working);
            return this;
        }
        public Region Surface8Way(int distance) {
            Retract8Way(distance).SymmetricExceptWith(working);
            return this;
        }
    }
}
