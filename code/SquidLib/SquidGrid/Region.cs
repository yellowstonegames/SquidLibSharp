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
        public Region(Region other) : base(other) {
            if (other is Region) {
                Width = other.Width;
                Height = other.Height;
            } else throw new ArgumentNullException(nameof(other));
        }
        public Region Copy() => new Region(this);
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
        public static Region FromGreater<TComp>(Grid<IComparable<TComp>> grid, TComp lowerBound) {
            if (grid == null)
                return new Region(1, 1);
            Region region = new Region(grid.Width, grid.Height);
            for (int x = 0; x < grid.Width; x++) {
                for (int y = 0; y < grid.Height; y++) {
                    if (grid[x, y].CompareTo(lowerBound) > 0)
                        region.Add(Coord.Get(x, y));
                }
            }
            return region;
        }
        public static Region FromAtLeast<TComp>(Grid<IComparable<TComp>> grid, TComp lowerBound) {
            if (grid == null)
                return new Region(1, 1);
            Region region = new Region(grid.Width, grid.Height);
            for (int x = 0; x < grid.Width; x++) {
                for (int y = 0; y < grid.Height; y++) {
                    if (grid[x, y].CompareTo(lowerBound) >= 0)
                        region.Add(Coord.Get(x, y));
                }
            }
            return region;
        }
        public static Region FromLess<TComp>(Grid<IComparable<TComp>> grid, TComp upperBound) {
            if (grid == null)
                return new Region(1, 1);
            Region region = new Region(grid.Width, grid.Height);
            for (int x = 0; x < grid.Width; x++) {
                for (int y = 0; y < grid.Height; y++) {
                    if (grid[x, y].CompareTo(upperBound) < 0)
                        region.Add(Coord.Get(x, y));
                }
            }
            return region;
        }
        public static Region FromAtMost<TComp>(Grid<IComparable<TComp>> grid, TComp upperBound) {
            if (grid == null)
                return new Region(1, 1);
            Region region = new Region(grid.Width, grid.Height);
            for (int x = 0; x < grid.Width; x++) {
                for (int y = 0; y < grid.Height; y++) {
                    if (grid[x, y].CompareTo(upperBound) <= 0)
                        region.Add(Coord.Get(x, y));
                }
            }
            return region;
        }
        /// <summary>
        /// Creates a Region filled with the items from a Grid that are greater than lowerBound and less than upperBound.
        /// </summary>
        /// <typeparam name="TComp">A type that implements IComparable to itself, like int or double.</typeparam>
        /// <param name="grid">A Grid of TComp items; its Width and Height will be used for the result.</param>
        /// <param name="lowerBound">The lower exclusive bound for TComp items.</param>
        /// <param name="upperBound">The upper exclusive bound for TComp items.</param>
        /// <returns>A new Region with the same Width and Height as grid, and contents drawn from it.</returns>
        public static Region FromWithin<TComp>(Grid<IComparable<TComp>> grid, TComp lowerBound, TComp upperBound) {
            if (grid == null)
                return new Region(1, 1);
            Region region = new Region(grid.Width, grid.Height);
            for (int x = 0; x < grid.Width; x++) {
                for (int y = 0; y < grid.Height; y++) {
                    if (grid[x, y].CompareTo(lowerBound) > 0 && grid[x, y].CompareTo(upperBound) < 0)
                        region.Add(Coord.Get(x, y));
                }
            }
            return region;
        }
        /// <summary>
        /// Creates a Region filled with the items from a Grid that are greater than or equal to lowerBound and less than or equal to upperBound.
        /// </summary>
        /// <typeparam name="TComp">A type that implements IComparable to itself, like int or double.</typeparam>
        /// <param name="grid">A Grid of TComp items; its Width and Height will be used for the result.</param>
        /// <param name="lowerBound">The lower inclusive bound for TComp items.</param>
        /// <param name="upperBound">The upper inclusive bound for TComp items.</param>
        /// <returns>A new Region with the same Width and Height as grid, and contents drawn from it.</returns>
        public static Region FromBetween<TComp>(Grid<IComparable<TComp>> grid, TComp lowerBound, TComp upperBound) {
            if (grid == null)
                return new Region(1, 1);
            Region region = new Region(grid.Width, grid.Height);
            for (int x = 0; x < grid.Width; x++) {
                for (int y = 0; y < grid.Height; y++) {
                    if (grid[x, y].CompareTo(lowerBound) >= 0 && grid[x, y].CompareTo(upperBound) <= 0)
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
                        Add(Coord.Get(x, y));
                    else
                        Remove(Coord.Get(x, y));
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

        public Region Insert(Region other, int offsetX, int offsetY) {
            if (other is null)
                return this;
            foreach(Coord point in other.Ordering) {
                this[point.X + offsetX, point.Y + offsetY] = true;
            }
            return this;
        }
        public Region Translate(int offsetX, int offsetY) {
            working.ResetTo(this);
            Clear();
            foreach(Coord point in working.Ordering) {
                this[point.X + offsetX, point.Y + offsetY] = true;
            }
            return this;
        }
    }
}
