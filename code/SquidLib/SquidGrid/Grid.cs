using System;
using System.Collections.Generic;
using System.Text;

namespace SquidLib.SquidGrid {
    public class Grid<T> {

        public int Width { get; private set; }
        public int Height { get; private set; }
        private T[] raw;

        /// <summary>
        /// This is the object to be returned when a T is requested that is outside of the grid area.
        /// </summary>
        public T Outside { get; set; }

        /// <summary>
        /// This is the object placed in new Grid locations. This is used during expansion operations
        /// as well as during initial creation if a constructor that takes it in is used.
        /// </summary>
        public T DefaultFill { get; set; }

        /// <summary>
        /// Creates a default sized grid.
        /// 
        /// Uses the default of the object type used to create the grid for items outside the grid space.
        /// </summary>
        public Grid() : this(80, 24, outside: default) { }

        /// <summary>
        /// Creates a grid with the given size.
        /// 
        /// Uses the default of the object type used to create the grid for items outside the grid space.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Grid(int width, int height) : this(width, height, outside: default) { }

        /// <summary>
        /// Creates a grid with the given size and given object to be returned on requests that lie outside of
        /// the grid space.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="outside"></param>
        public Grid(int width, int height, T outside) {
            Width = Math.Max(1, width);
            Height = Math.Max(1, height);
            Outside = outside;
            raw = new T[Width * Height];
        }

        /// <summary>
        /// Creates a grid with the given size and given object to be returned on requests that lie outside
        /// of the grid space, as well as the object to fill the interior of the grid space.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="outside">The object that will be returned on requests outside the grid area</param>
        /// <param name="defaultFill">The object that will be placed in all spaces inside the grid</param>
        public Grid(int width, int height, T outside, T defaultFill) : this(width, height, outside) {
            DefaultFill = defaultFill;
            for (int i = Width * Height - 1; i >= 0; i--) {
                raw[i] = defaultFill;
            }
        }

        /// <summary>
        /// Creates a new Grid based on the passed in Grid.
        /// 
        /// The origional grid's Outside object and tiles are copied by reference and not created anew.
        /// 
        /// If the provided Grid is null then a default Grid object is created.
        /// </summary>
        /// <param name="other"></param>
        public Grid(Grid<T> other) {
            if (other is null) { // NOTE - should this be valid?
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

        /// <summary>
        /// Returns the object at the given location.
        /// 
        /// If the location lies outside of the bounds of the Grid then the singular Outside
        /// object passed in during creation is returned.
        /// 
        /// If one of the constructors that does not take an Outside object was used, then
        /// the Outside object returned is the default type for the Grid's content type.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public T this[int x, int y] {
            get {
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

        /// <summary>
        /// Filles the Grid with the provided sequence, starting at the given location.
        /// 
        /// If the sequence is longer than the grid starting at the provided location, the grid is filled to its
        /// width and then the rest of the sequence is ignored where it would not fit in the grid.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="sequence">The items that are to be placed, in the order to place them</param>
        public void RowEdit(int x, int y, IEnumerable<T> sequence) {
            if (!(sequence is null) && x >= 0 && x < Width && y >= 0 && y < Height) {
                foreach (T item in sequence) {
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
            if (data[data.Length - 1] != '\n')
                data += '\n';
            int width = data.IndexOf('\n');
            int height = 0;
            int idx = width;
            while (idx != -1) {
                idx = data.IndexOf('\n', idx + 1);
                height++;
            }
            Grid<char> grid = new Grid<char>(width, height, '#');
            char[] raw = grid.RawData();
            data.Replace("\n", "").CopyTo(0, raw, 0, width * height);
            return grid;
        }

        /// <summary>
        /// A more specialized counterpart to ToString() that shows the chars in a Grid of char as they often would be rendered,
        /// with (x=0,y=0) in the upper left and rows separated by newlines (not CRLF, just LF).
        /// </summary>
        /// <param name="grid">A Grid of char to get a printable representation for.</param>
        /// <returns>A string composed of the chars in grid, with rows separated by LF newlines.</returns>
        public static string Show(this Grid<char> grid) {
            if (grid is null) return "";
            StringBuilder builder = new StringBuilder(grid.Height * (grid.Width + 1));
            char[] raw = grid.RawData();
            for (int i = 0; i < grid.Height; i++) {
                builder.Append(raw, i * grid.Width, grid.Width).Append('\n');
            }
            return builder.ToString();
        }

        /// <summary>
        /// Adds the provided Grid to the end of this StringBuilder. Newlines are added between rows of the grid so that output
        /// will render in the same presentation manner as the grid itself.
        /// </summary>
        /// <param name="builder">The starting StringBuilder. If this is null then it is treated as an empty builder.</param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static StringBuilder Append(this StringBuilder builder, Grid<char> grid) {
            if (grid is null)
                return builder;
            if (builder is null)
                builder = new StringBuilder(grid.Width * grid.Height + grid.Height); // capacity is size of grid + 1 per row for newline
            char[] raw = grid.RawData();
            for (int i = 0; i < grid.Height; i++) {
                builder.Append(raw, i * grid.Width, grid.Width).Append('\n');
            }
            return builder;
        }

        /// <summary>
        /// Returns a string that is the combination of this string with the grid appended to it.
        /// 
        /// Newlines are added between rows of the grid so that output will render in the same presentation manner as
        /// the grid itself.
        /// 
        /// If possible, using the StringBuilder version of this method should be preferred.
        /// </summary>
        /// <param name="input">The starting string. If this is null then it is treated as an empty string.</param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public static string Append(this string input, Grid<char> grid) {
            if (grid is null)
                return input;
            StringBuilder builder;
            if (input is null)
                builder = new StringBuilder(grid.Width * grid.Height + grid.Height); // capacity is size of grid + 1 per row for newline
            else
                builder = new StringBuilder(input);
            char[] raw = grid.RawData();
            for (int i = 0; i < grid.Height; i++) {
                builder.Append(raw, i * grid.Width, grid.Width).Append('\n');
            }
            return builder.ToString();
        }
    }
}