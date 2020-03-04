using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;

using BearLib;
using ColorHelper;
using SquidLib;
using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace RogueDelivery {
    class RogueDelivery {
        private bool keepRunning = true;
        private RNG rng;

        private int windowWidth = 120,
            windowHeight = 40,
            logHeight = 3,
            statusHeight = 1;
        private int width, height; // for the map area

        private Dictionary<Coord, char> borders = new Dictionary<Coord, char>();

        private char[][] map;
        private Coord playerLocation;
        private char playerGlyph = '@';
        private Color playerColor = Color.AliceBlue;
        private Physical playerPhysical = new Physical() {
            Name = "Rogue",
            Class = "Delivery Driver",
            Health = 23,
            Strength = 4,
            Luck = 12,
            Wiles = 2,
            XP = 0
        };

        private Dictionary<Direction, char[][]> wagonGlyphs = new Dictionary<Direction, char[][]>() { // do to array processing, these look rotated to their direction names
            {
                Direction.Left, new char[][]{
                    " Ŏ ".ToCharArray(),
                    "∣∣∣".ToCharArray(),
                    "⍬∣⍬".ToCharArray(),
                    "∣∣∣".ToCharArray()
                }
            },
            {
                Direction.Right, new char[][]{
                    "∣∣∣".ToCharArray(),
                    "⍬∣⍬".ToCharArray(),
                    "∣∣∣".ToCharArray(),
                    " Ŏ ".ToCharArray()
                }
            },
            {
                Direction.Down, new char[][]{
                    "-⍬- ".ToCharArray(),
                    "---Ŏ".ToCharArray(),
                    "-⍬- ".ToCharArray()
                }
            },
            {
                Direction.Up, new char[][]{
                    " -⍬-".ToCharArray(),
                    "Ŏ---".ToCharArray(),
                    " -⍬-".ToCharArray()
                }
            }
        };
        private char[][] wagonGlyph;
        private Coord wagonLocation;

        static void Main() {
            var rd = new RogueDelivery(); // start up the primary system
            rd.Start();
        }

        private RogueDelivery() {
            rng = new RNG();

            width = windowWidth;
            height = windowHeight - logHeight - statusHeight - 2;
            playerLocation = Coord.Get(rng.NextInt(width), rng.NextInt(height));
            wagonGlyph = wagonGlyphs[Direction.Right];
            wagonLocation = Coord.Get(width / 2, height / 2);

            // init display style
            for (int x = 1; x < windowWidth - 1; x++) {
                borders[Coord.Get(x, 0)] = '─';
                borders[Coord.Get(x, windowHeight - 1)] = '─';
                borders[Coord.Get(x, windowHeight - 1 - statusHeight - 1)] = '─';
                borders[Coord.Get(x, windowHeight - 1 - statusHeight - 1 - logHeight - 1)] = '─';
            }
            for (int y = 1; y < windowHeight - 1; y++) {
                borders[Coord.Get(0, y)] = '│';
                borders[Coord.Get(windowWidth - 1, y)] = '│';
            }
            borders[Coord.Get(0, 0)] = '┌';
            borders[Coord.Get(windowWidth - 1, 0)] = '┐';
            borders[Coord.Get(0, windowHeight - 1)] = '└';
            borders[Coord.Get(windowWidth - 1, windowHeight - 1)] = '┘';
            borders[Coord.Get(0, windowHeight - 1 - statusHeight - 1)] = '├';
            borders[Coord.Get(0, windowHeight - 1 - statusHeight - 1 - logHeight - 1)] = '├';
            borders[Coord.Get(windowWidth - 1, windowHeight - 1 - statusHeight - 1)] = '┤';
            borders[Coord.Get(windowWidth - 1, windowHeight - 1 - statusHeight - 1 - logHeight - 1)] = '┤';

            // Setup terminal
            Terminal.Open();
            Terminal.Set(
                $"window: title='Rogue Delivery', size={windowWidth}x{windowHeight};" +
                $"output: vsync=false;" +
                $"font: Iosevka.ttf, size=9x21, hinting=autohint"
                );
            BltColor.LoadAurora();
            Terminal.Color(Terminal.ColorFromName(rng.RandomElement(BltColor.AuroraNames)));
            Terminal.Refresh();
        }

        private void Start() {
            DrawMap();
            Terminal.Refresh();
            while (keepRunning) {
                if (Terminal.HasInput()) {
                    switch (Terminal.Read()) {
                        case Terminal.TK_ESCAPE:
                        case Terminal.TK_CLOSE:
                            keepRunning = false;
                            break;
                        case Terminal.TK_LEFT:
                            MovePlayer(Direction.Left);
                            break;
                        case Terminal.TK_UP:
                            MovePlayer(Direction.Up);
                            break;
                        case Terminal.TK_DOWN:
                            MovePlayer(Direction.Down);
                            break;
                        case Terminal.TK_RIGHT:
                            MovePlayer(Direction.Right);
                            break;
                        default:
                            // ignore unknown commands
                            break;
                    }
                    DrawMap();
                    Terminal.Refresh();
                }
            }
        }

        private void MovePlayer(Direction direction) {
            playerLocation = Utilities.Translate(playerLocation, direction);
            wagonGlyph = wagonGlyphs[direction];
        }

        private void DrawMap() {
            Terminal.Clear();
            Terminal.Color(Terminal.ColorFromName(rng.RandomElement(BltColor.AuroraNames)));
            PaintBorder();

            Terminal.Color(Color.SandyBrown);
            for (int x = 0; x < wagonGlyph.Length; x++) {
                for (int y = 0; y < wagonGlyph[x].Length; y++) {
                    PutOnMap(Coord.Get(wagonLocation.X + x, wagonLocation.Y + y), wagonGlyph[x][y]);
                }
            }

            Terminal.Color(playerColor);
            PutOnMap(playerLocation, playerGlyph);

            // Status section
            int startY = windowHeight - statusHeight - 1;
            Terminal.Color(playerColor);
            int startX = Put(2, startY, playerPhysical.Name);
            Terminal.Color(Color.White);
            startX = Put(startX + 1, startY, "The");
            Terminal.Color(Color.LightCoral);
            startX = Put(startX + 1, startY, playerPhysical.Class);
            Terminal.Color(Color.White);
            startX = Put(startX + 1, startY, "--");
            Terminal.Color(Color.Silver);
            startX = Put(startX + 1, startY, "Strength: " + playerPhysical.Strength);
            Terminal.Color(Color.HotPink);
            startX = Put(startX + 2, startY, "Luck: " + playerPhysical.Luck);
            Terminal.Color(Color.MediumPurple);
            startX = Put(startX + 2, startY, "Wiles: " + playerPhysical.Wiles);
            Terminal.Color(Color.RosyBrown);
            startX = Put(startX + 2, startY, "XP: " + playerPhysical.XP);
            Terminal.Color(Color.SeaGreen);
            startX = Put(startX + 2, startY, "Health: " + playerPhysical.Health);
        }

        private void PaintBorder() {
            foreach (var kvp in borders) {
                Put(kvp.Key, kvp.Value);
            }
        }

        private static void Put(Coord coord, char c) => Terminal.Put(coord.X, coord.Y, c);

        private static int Put(int x, int y, string s) {
            for (int i = 0; i < s.Length; i++) {
                Terminal.Put(x + i, y, s[i]);
            }
            return x + s.Length;
        }

        /// <summary>
        /// Puts the character on the map if the coordinate is valid, otherwise takes no action.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="c"></param>
        private void PutOnMap(Coord coord, char c) {
            if (InMapBounds(coord)) {
                Terminal.Put(coord.X + 1, coord.Y + 1, c); // handle window offset
            }
        }

        private bool InMapBounds(Coord coord) => coord.X >= 0 && coord.X < width && coord.Y >= 0 && coord.Y < height;

    }
}
