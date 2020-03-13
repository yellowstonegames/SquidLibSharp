using System;
using System.Collections.Generic;
using System.Text;

namespace SquidLib.SquidGrid {
    public class Grid<T> {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private T[] raw;
        public T Outside { get; set; }

        public Grid() : this(80, 24, default) { }

        public Grid(int width, int height) : this(width, height, default) { }

        public Grid(int width, int height, T outside) {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Outside = outside;
            raw = new T[Width * Height];
        }
        public Grid(int width, int height, T outside, T initialFill) : this(width, height, outside){
            for(int i = Width * Height - 1; i >= 0; i--) {
                raw[i] = initialFill;
            }
        }

        public Grid(Grid<T> other) {
            if(other is null) {
                Width = 80;
                Height = 24;
                Outside = default;
                raw = new T[Width * Height];
                return;
            }
            Width = other.Width;
            Height = other.Height;
            Outside = other.Outside;
            raw = new T[Width * Height];
            Array.Copy(other.raw, raw, raw.Length);
        }

        public T this[int x, int y] { get {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return Outside;
                return raw[y * Width + x];
            }
            set {
                if (x >= 0 && x < Width && y >= 0 && y < Height)
                    raw[y * Width + x] = value;
            }
        }

        /// <summary>
        /// If the given x,y position is in-bounds, this sets the T at that position to item and returns true; otherwise it returns false.
        /// This will return true even when the given position already contains item, as long as it is in-bounds.
        /// </summary>
        /// <param name="x">X position, will be bounds checked.</param>
        /// <param name="y">Y position, will be bounds checked.</param>
        /// <param name="item">The T item to try to place at x,y.</param>
        /// <returns>true if x,y is in-bounds, false otherwise.</returns>
        public bool TrySet(int x, int y, T item) {
            if (x >= 0 && x < Width && y >= 0 && y < Height) {
                raw[y * Width + x] = item;
                return true;
            }
            return false;
        }

        public void RowEdit(int x, int y, IEnumerable<T> sequence) {
            if(!(sequence is null) && x >= 0 && x < Width && y >= 0 && y < Height) {
                foreach(T item in sequence) {
                    raw[y * Width + x++] = item;
                    if (x >= Width) break;
                }
            }
        }
        /// <summary>
        /// The data this uses internally, as a row-major 1D array of T with length <code>Width * Height</code>.
        /// You can place rows directly into this, but they will not be validated for bounds.
        /// </summary>
        /// <returns>The row-major 1D T array this uses internally.</returns>
        public T[] RawData() => raw;
    }
    public static class GridExtensions {
        /// <summary>
        /// Given a "rectangular string" with some form of newlines separating equal-length rows, this creates a Grid of char with its contents.
        /// </summary>
        /// <param name="data">A string that should have equal-length rows separated by newlines (Unix or Win32 conventions are both permitted).</param>
        /// <returns>A Grid of char containing the contents of this string.</returns>
        public static Grid<char> ToGrid(this string data) {
            if (data is null) return null;
            data = data.Replace("\r\n", "\n");
            int width = data.IndexOf('\n');
            int height = 1;
            if (width == -1) {
                width = data.Length;
                height = 0;
            }
            int idx = width;
            while(idx != -1) {
                idx = data.IndexOf('\n', idx + 1);
                height++;
            }
            Grid<char> grid = new Grid<char>(width, height, '#');
            char[] raw = grid.RawData();
            data.Replace("\n", "").CopyTo(0, raw, 0, width * height);
            return grid;
        }
    }
}