using System;
using System.Collections.Generic;
using SquidLib.SquidMath;

namespace SquidLib.SquidGrid {
    class ConnectingGenerator {
        public int width;
        public int height;
        public int roomWidth;
        public int roomHeight;
        public int wallThickness;
        public Grid<char> dungeon;
        public Grid<CellCategory> environment;
        public Region region;
        private Region tempRegion;
        public IRNG rng;

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
            this.width = Math.Max(1, width);
            this.height = Math.Max(1, height);
            region = new Region(this.width, this.height);
            tempRegion = new Region(this.width, this.height);
            this.roomWidth = Math.Max(1, roomWidth);
            this.roomHeight = Math.Max(1, roomHeight);
            this.wallThickness = Math.Max(1, wallThickness);
            dungeon = new Grid<char>(this.width, this.height, '#', '#');
            environment = new Grid<CellCategory>(this.width, this.height, CellCategory.Untouched, CellCategory.Untouched);
            rng = random;
        }
        /**
         * Generates a dungeon or other map as a 2D char array. Uses the convention of '#' representing a wall and '.'
         * representing a bare floor, and also fills {@link #environment} with appropriate constants from DungeonUtility,
         * like {@link DungeonUtility#ROOM_FLOOR} and {@link DungeonUtility#ROOM_WALL}.
         * 
         * @return a 2D char array representing a room-based map, using standard conventions for walls/floors
         */
    public Grid<char> Generate() {
            int gridWidth = (width + wallThickness - 2) / (roomWidth + wallThickness), gridHeight = (height + wallThickness - 2) / (roomHeight + wallThickness), gridMax = gridWidth * gridHeight;
            if (gridWidth <= 0 || gridHeight <= 0)
                return dungeon;
            dungeon.DefaultFill = '#';
            environment.DefaultFill = CellCategory.Untouched;
            if (region.Width != width || region.Height != height) {
                region = new Region(this.width, this.height);
                tempRegion = new Region(this.width, this.height);
            }
            IndexedDictionary<uint, int> links = new IndexedDictionary<uint, int>(gridMax), surface = new IndexedDictionary<uint, int>(gridMax);
            List<int> choices = new List<int>(4);
            int dx = rng.NextInt(gridWidth), dy = rng.NextInt(gridHeight);
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
                int choice = rng.RandomElement(choices);
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
                d = rng.RandomKey(surface);
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
                    int choice = rng.RandomElement(choices);
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

                region.InsertRectangle(1 + dx * (roomWidth + wallThickness), 1 + dy * (roomHeight + wallThickness), roomWidth, roomHeight);
                if ((conn & 1) != 0)
                    region.InsertRectangle(1 + dx * (roomWidth + wallThickness) + roomWidth, 1 + dy * (roomHeight + wallThickness), wallThickness, roomHeight);
                if ((conn & 2) != 0)
                    region.InsertRectangle(1 + dx * (roomWidth + wallThickness), 1 + dy * (roomHeight + wallThickness) + roomHeight, roomWidth, wallThickness);
                if ((conn & 4) != 0)
                    region.InsertRectangle(1 + dx * (roomWidth + wallThickness) - wallThickness, 1 + dy * (roomHeight + wallThickness), wallThickness, roomHeight);
                if ((conn & 8) != 0)
                    region.InsertRectangle(1 + dx * (roomWidth + wallThickness), 1 + dy * (roomHeight + wallThickness) - wallThickness, roomWidth, wallThickness);
            }
            foreach(Coord c in region) {
                dungeon[c.X, c.Y] = '.';
                environment[c.X, c.Y] = CellCategory.RoomFloor;
            }
            tempRegion.Clear();
            tempRegion.AddAll(region);
            tempRegion.Fringe8Way(1);
            foreach (Coord c in tempRegion) {
                environment[c.X, c.Y] = CellCategory.RoomWall;
            }
            return dungeon;
        }

    }
}
