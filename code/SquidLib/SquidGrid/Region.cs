using System;
using System.Collections.Generic;
using System.Text;
using SquidLib.SquidMath;

namespace SquidLib.SquidGrid {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Current name indicates correct level of specificity.")]
    public class Region : IndexedSet<Coord>, ICollection<Coord> {
        private static IndexedSet<Coord> working = new IndexedSet<Coord>();
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Region(int width, int height) {
            Width = width;
            Height = height;
        }
        public Region () : this(80, 24) {
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

        public Region Expand(int distance, bool eightWay = false) {
            working.ResetTo(this);
            int size = working.Count;
            if (eightWay) {
                foreach (Coord point in working.Ordering) {
                    for (int x = Math.Max(0, point.X - distance), xd = Math.Min(Width - 1, point.X + distance); x < xd; x++) {
                        for (int y = Math.Max(0, point.Y - distance), yd = Math.Min(Height - 1, point.Y + distance); y < yd; y++) {
                            Add(Coord.Get(x, y));
                        }
                    }
                }
            } else {
                foreach (Coord point in working.Ordering) {
                    for (int x = Math.Max(0, point.X - distance), xd = Math.Min(Width - 1, point.X + distance); x < xd; x++) {
                        for (int y = Math.Max(0, point.Y - distance), yd = Math.Min(Height - 1, point.Y + distance); y < yd; y++) {
                            if(Math.Abs(point.X - x) + Math.Abs(point.Y - y) <= distance)
                                Add(Coord.Get(x, y));
                        }
                    }
                }
            }
            return this;
        }
    }
}
