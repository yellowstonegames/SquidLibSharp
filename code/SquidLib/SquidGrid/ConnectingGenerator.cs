using System;
using System.Collections.Generic;

using SquidLib.SquidMath;

namespace SquidLib.SquidGrid {
    public class ConnectingGenerator {
        public int Width { get; set; }
        public int Height { get; set; }
        public int RoomWidth { get; set; }
        public int RoomHeight { get; set; }
        public int WallThickness { get; set; }
        public Grid<char> Dungeon { get; set; }
        public Grid<CellCategory> Environment { get; set; }
        public Region Region { get; private set; }
        private Region tempRegion;
        public IRNG Rng { get; set; }

        /**
         * Calls {@link #ConnectingGenerator(int, int, int, int, IRNG, int)} with width 80, height 80, roomWidth 8,
         * roomHeight 8, a new {@link GWTRNG} for random, and wallThickness 2.
         */
        public ConnectingGenerator() : this(80, 80, 8, 8, new RNG(), 2) {
        }
        /**
         * Determines room width and room height by dividing width or height by 10; wallThickness is 2. 
         * @param width total width of the map, in cells
         * @param height total height of the map, in cells
         * @param random an IRNG to make random choices for connecting rooms
         */

        public ConnectingGenerator(int width, int height, IRNG random) : this(width, height, width / 10, height / 10, random, 2) {
        }
        /**
         * Exactly like {@link #ConnectingGenerator(int, int, int, int, IRNG, int)} with wallThickness 2.
         * @param width total width of the map, in cells
         * @param height total height of the map, in cells
         * @param roomWidth target width of each room, in cells; only counts the center floor area of a room
         * @param roomHeight target height of each room, in cells; only counts the center floor area of a room
         * @param random an IRNG to make random choices for connecting rooms
         */
        public ConnectingGenerator(int width, int height, int roomWidth, int roomHeight, IRNG random) : this(width, height, roomWidth, roomHeight, random, 2) {
        }

        /**
         * 
         * @param width total width of the map, in cells
         * @param height total height of the map, in cells
         * @param roomWidth target width of each room, in cells; only counts the center floor area of a room
         * @param roomHeight target height of each room, in cells; only counts the center floor area of a room
         * @param random an IRNG to make random choices for connecting rooms
         * @param wallThickness how thick a wall between two rooms should be, in cells; 1 is minimum, and this usually
         *                      shouldn't be much more than roomWidth or roomHeight
         */
        public ConnectingGenerator(int width, int height, int roomWidth, int roomHeight, IRNG random, int wallThickness) {
            this.Width = Math.Max(1, width);
            this.Height = Math.Max(1, height);
            Region = new Region(this.Width, this.Height);
            tempRegion = new Region(this.Width, this.Height);
            this.RoomWidth = Math.Max(1, roomWidth);
            this.RoomHeight = Math.Max(1, roomHeight);
            this.WallThickness = Math.Max(1, wallThickness);
            Dungeon = new Grid<char>(this.Width, this.Height, '#', '#');
            Environment = new Grid<CellCategory>(this.Width, this.Height, CellCategory.Untouched, CellCategory.Untouched);
            Rng = random;
        }
        /**
         * Generates a dungeon or other map as a 2D char array. Uses the convention of '#' representing a wall and '.'
         * representing a bare floor, and also fills {@link #environment} with appropriate constants from DungeonUtility,
         * like {@link DungeonUtility#ROOM_FLOOR} and {@link DungeonUtility#ROOM_WALL}.
         * 
         * @return a 2D char array representing a room-based map, using standard conventions for walls/floors
         */
        public Grid<char> Generate() {
            int gridWidth = (Width + WallThickness - 2) / (RoomWidth + WallThickness), gridHeight = (Height + WallThickness - 2) / (RoomHeight + WallThickness), gridMax = gridWidth * gridHeight;
            if (gridWidth <= 0 || gridHeight <= 0)
                return Dungeon;
            Dungeon.DefaultFill = '#';
            Environment.DefaultFill = CellCategory.Untouched;
            if (Region.Width != Width || Region.Height != Height) {
                Region = new Region(this.Width, this.Height);
                tempRegion = new Region(this.Width, this.Height);
            }
            IndexedDictionary<uint, int> links = new IndexedDictionary<uint, int>(gridMax), surface = new IndexedDictionary<uint, int>(gridMax);
            List<int> choices = new List<int>(4);
            int dx = Rng.NextInt(gridWidth), dy = Rng.NextInt(gridHeight);
            uint d = (uint)dy << 16 | (uint)dx;
            links[d] = 0;
            surface[d] = 0;
            for (int i = 0; i < 15 && links.Count < gridMax && surface.Count > 0; i++) {
                choices.Clear();
                if (dx < gridWidth - 1 && !links.ContainsKey(d + 1)) choices.Add(1);
                if (dy < gridHeight - 1 && !links.ContainsKey(d + 0x10000)) choices.Add(2);
                if (dx > 0 && !links.ContainsKey(d - 1)) choices.Add(4);
                if (dy > 0 && !links.ContainsKey(d - 0x10000)) choices.Add(8);
                if (choices.Count == 0) {
                    surface.Remove(d);
                    break;
                }
                int choice = Rng.RandomElement(choices);
                if (links.ContainsKey(d))
                    links[d] = links[d] | choice;
                if (choices.Count == 1)
                    surface.Remove(d);
                switch (choice) {
                    case 1:
                        d += 1;
                        links[d] = 4;
                        surface[d] = 4;
                        break;
                    case 2:
                        d += 0x10000;
                        links[d] = 8;
                        surface[d] = 8;
                        break;
                    case 4:
                        d -= 1;
                        links[d] = 1;
                        surface[d] = 1;
                        break;
                    default:
                        d -= 0x10000;
                        links[d] = 2;
                        surface[d] = 2;
                        break;
                }
                dx = (int)(d & 0xFFFF);
                dy = (int)(d >> 16);
            }
            while (links.Count < gridMax) {
                d = Rng.RandomKey(surface);
                dx = (int)(d & 0xFFFF);
                dy = (int)(d >> 16);
                for (int i = 0; i < 5 && links.Count < gridMax && surface.Count > 0; i++) {
                    choices.Clear();
                    if (dx < gridWidth - 1 && !links.ContainsKey(d + 1)) choices.Add(1);
                    if (dy < gridHeight - 1 && !links.ContainsKey(d + 0x10000)) choices.Add(2);
                    if (dx > 0 && !links.ContainsKey(d - 1)) choices.Add(4);
                    if (dy > 0 && !links.ContainsKey(d - 0x10000)) choices.Add(8);
                    if (choices.Count == 0) {
                        surface.Remove(d);
                        break;
                    }
                    int choice = Rng.RandomElement(choices);
                    if (links.ContainsKey(d))
                        links[d] = links[d] | choice;
                    if (choices.Count == 1)
                        surface.Remove(d);
                    switch (choice) {
                        case 1:
                            d += 1;
                            links[d] = 4;
                            surface[d] = 4;
                            break;
                        case 2:
                            d += 0x10000;
                            links[d] = 8;
                            surface[d] = 8;
                            break;
                        case 4:
                            d -= 1;
                            links[d] = 1;
                            surface[d] = 1;
                            break;
                        default:
                            d -= 0x10000;
                            links[d] = 2;
                            surface[d] = 2;
                            break;
                    }
                    dx = (int)(d & 0xFFFF);
                    dy = (int)(d >> 16);
                }
            }
            for (int i = 0; i < links.Count; i++) {
                d = links[Key.At, i];
                dx = (int)(d & 0xFFFF);
                dy = (int)(d >> 16);
                int conn = links[Value.At, i];

                Region.InsertRectangle(1 + dx * (RoomWidth + WallThickness), 1 + dy * (RoomHeight + WallThickness), RoomWidth, RoomHeight);
                if ((conn & 1) != 0)
                    Region.InsertRectangle(1 + dx * (RoomWidth + WallThickness) + RoomWidth, 1 + dy * (RoomHeight + WallThickness), WallThickness, RoomHeight);
                if ((conn & 2) != 0)
                    Region.InsertRectangle(1 + dx * (RoomWidth + WallThickness), 1 + dy * (RoomHeight + WallThickness) + RoomHeight, RoomWidth, WallThickness);
                if ((conn & 4) != 0)
                    Region.InsertRectangle(1 + dx * (RoomWidth + WallThickness) - WallThickness, 1 + dy * (RoomHeight + WallThickness), WallThickness, RoomHeight);
                if ((conn & 8) != 0)
                    Region.InsertRectangle(1 + dx * (RoomWidth + WallThickness), 1 + dy * (RoomHeight + WallThickness) - WallThickness, RoomWidth, WallThickness);
            }
            foreach (Coord c in Region) {
                Dungeon[c.X, c.Y] = '.';
                Environment[c.X, c.Y] = CellCategory.RoomFloor;
            }
            tempRegion.Clear();
            tempRegion.AddAll(Region);
            tempRegion.Fringe8Way(1);
            foreach (Coord c in tempRegion) {
                Environment[c.X, c.Y] = CellCategory.RoomWall;
            }
            return Dungeon;
        }

    }
}
