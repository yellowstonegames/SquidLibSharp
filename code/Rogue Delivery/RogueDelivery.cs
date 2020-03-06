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

        private char[][] wagonGlyph;
        private BigMob wagon;

        static void Main() {
            var rd = new RogueDelivery(); // start up the primary system
            rd.Start();
        }

        private void InitObjects() {
            wagon = new BigMob {
                DefaultColor = Color.SandyBrown
            };
            wagon.SetGlyphs(Direction.Up, 0, 2, new string[] {
                " Ŏ ",
                "∣∣∣",
                "⍬∣⍬",
                "∣∣∣"
            });
            wagon.SetGlyphs(Direction.Down, 0, 1, new string[] {
                "∣∣∣",
                "⍬∣⍬",
                "∣∣∣",
                " Ŏ "
            });
            wagon.SetGlyphs(Direction.Right, 1, 1, new string[] {
                "-⍬- ",
                "---Ŏ",
                "-⍬- "
            });
            wagon.SetGlyphs(Direction.Left, 2, 1, new string[] {
                " -⍬-",
                "Ŏ---",
                " -⍬-"
            });
            wagon.Reps[Direction.Up].Tiles[Coord.Get(1, 0)].Color = Color.White;
            wagon.Reps[Direction.Down].Tiles[Coord.Get(1, 3)].Color = Color.White;
            wagon.Reps[Direction.Right].Tiles[Coord.Get(3, 1)].Color = Color.White;
            wagon.Reps[Direction.Left].Tiles[Coord.Get(0, 1)].Color = Color.White;
        }

        private RogueDelivery() {
            rng = new RNG();
            InitObjects();

            width = windowWidth;
            height = windowHeight - logHeight - statusHeight - 2;

            map = ArrayTools.Create<char>(' ', width, height);

            playerLocation = Coord.Get(rng.NextInt(width), rng.NextInt(height));
            wagon.Location = Coord.Get(width / 2, height / 2);

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

            //playerLocation = Utilities.Translate(playerLocation, direction);
            if (wagon.Facing == direction) {
                wagon.Location = Utilities.Translate(wagon.Location, direction);
            } else {
                wagon.Facing = direction;
            }
        }

        private void DrawMap() {
            Terminal.Clear();

            Terminal.Color(Color.LightSlateGray);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Terminal.Put(x, y, map[x][y]);
                }
            }

            Terminal.Color(Terminal.ColorFromName(rng.RandomElement(BltColor.AuroraNames)));
            PaintBorder();

            Coord center = wagon.Reps[wagon.Facing].ControlPoint;
            int localX = wagon.Location.X - center.X;
            int localY = wagon.Location.Y - center.Y;
            foreach (var rep in wagon.Reps[wagon.Facing].Tiles) {
                Terminal.Color(rep.Value.Color);
                PutOnMap(localX + rep.Key.X, localY + rep.Key.Y, rep.Value.Glyph);
            }

            //Terminal.Color(playerColor);
            //PutOnMap(playerLocation, playerGlyph);

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
            PutOnMap(coord.X, coord.Y, c);
        }

        /// <summary>
        /// Puts the character on the map if the coordinate is valid, otherwise takes no action.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="c"></param>
        private void PutOnMap(int x, int y, char c) {
            if (InMapBounds(x, y)) {
                Terminal.Put(x + 1, y + 1, c); // handle window offset
            }
        }

        private bool InMapBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

        private bool InMapBounds(Coord coord) => InMapBounds(coord.X, coord.Y);

    }
}
