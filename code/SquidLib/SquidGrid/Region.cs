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
            if (grid is null)
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
        public static Region FromGreater<TComp>(Grid<TComp> grid, TComp lowerBound) where TComp : IComparable<TComp> {
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
        public static Region FromAtLeast<TComp>(Grid<TComp> grid, TComp lowerBound) where TComp : IComparable<TComp> {
            if (grid is null)
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
        public static Region FromLess<TComp>(Grid<TComp> grid, TComp upperBound) where TComp : IComparable<TComp> {
            if (grid is null)
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
        public static Region FromAtMost<TComp>(Grid<TComp> grid, TComp upperBound) where TComp : IComparable<TComp> {
            if (grid is null)
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
        public static Region FromWithin<TComp>(Grid<TComp> grid, TComp lowerBound, TComp upperBound) where TComp : IComparable<TComp> {
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
        public static Region FromBetween<TComp>(Grid<TComp> grid, TComp lowerBound, TComp upperBound) where TComp : IComparable<TComp> {
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
        /// <summary>
        /// Creates a Region filled with the items from a Grid that are equal to match; match must have a type that implements IEquatable,
        /// but it has no other restrictions.
        /// </summary>
        /// <typeparam name="TEq">A type that implements IEquatable to itself, like int or double.</typeparam>
        /// <param name="grid">A Grid of TEq items; its Width and Height will be used for the result.</param>
        /// <param name="match">The TEq, which should implement IEquatable, to match.</param>
        /// <returns>A new Region with the same Width and Height as grid, and contents drawn from it.</returns>
        public static Region FromExactly<TEq>(Grid<TEq> grid, TEq match) {
            if (grid is null)
                return new Region(1, 1);
            return new Region(grid.Width, grid.Height).RefillExactly(grid, match);
        }

        /// <summary>
        /// Empties the current contents of this Region, sets its Width and Height to match the given Grid, and
        /// fills this Region with all items in grid that are equal to match.
        /// </summary>
        /// <typeparam name="TEq">A type that implements IEquatable to itself, like int or double.</typeparam>
        /// <param name="grid">A Grid of TEq items; its Width and Height will be used for the result.</param>
        /// <param name="match">The TEq, which should implement IEquatable, to match.</param>
        /// <returns>This region, with potentially different width and height, for chaining.</returns>
        public Region RefillExactly<TEq>(Grid<TEq> grid, TEq match) {
            Clear();
            if (grid is null)
                return this;
            Width = grid.Width;
            Height = grid.Height;
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    if (grid[x, y].Equals(match))
                        Add(Coord.Get(x, y));
                }
            }
            return this;
        }
        /// <summary>
        /// Creates a Region filled with the items from a Grid that can be found in the given Set of items to look for.
        /// If an item in grid is not found in matches, or if matches is null or empty, that grid cell will be false;
        /// if the item is found, then that grid cell will be true.
        /// </summary>
        /// <typeparam name="TItem">Any type that can be put into both an ISet and a Grid.</typeparam>
        /// <param name="grid">A Grid of TItem items; its Width and Height will be used for the result.</param>
        /// <param name="matches">The ISet of TItem to look for matches in.</param>
        /// <returns>A new Region with the same Width and Height as grid, and contents drawn from it.</returns>
        public static Region FromSet<TItem>(Grid<TItem> grid, ISet<TItem> matches) {
            if (grid == null)
                return new Region(1, 1);
            Region region = new Region(grid.Width, grid.Height);
            if (matches is null || matches.Count == 0)
                return region;
            for (int x = 0; x < grid.Width; x++) {
                for (int y = 0; y < grid.Height; y++) {
                    if (matches.Contains(grid[x, y]))
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
