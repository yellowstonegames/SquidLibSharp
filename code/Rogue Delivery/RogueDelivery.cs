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

        static void Main() {
            var rd = new RogueDelivery(); // start up the primary system
            rd.Start();
        }

        private RogueDelivery() {
            rng = new RNG();

            width = windowWidth;
            height = windowHeight - logHeight - statusHeight - 2;
            playerLocation = Coord.Get(rng.NextInt(width), rng.NextInt(height));

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
            borders[Coord.Get(windowWidth -1, windowHeight - 1 - statusHeight - 1)] = '┤';
            borders[Coord.Get(windowWidth -1, windowHeight - 1 - statusHeight - 1 - logHeight - 1)] = '┤';

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
            PutOnMap(playerLocation, ' ');
            playerLocation = Utilities.Translate(playerLocation, direction);
        }

        private void DrawMap() {
            Terminal.Color(Terminal.ColorFromName(rng.RandomElement(BltColor.AuroraNames)));
            PaintBorder();

            Terminal.Color(playerColor);
            PutOnMap(playerLocation, playerGlyph);
        }

        private void PaintBorder() {
            foreach (var kvp in borders) {
                Put(kvp.Key, kvp.Value);
            }
        }

        private static void Put(Coord coord, char c) => Terminal.Put(coord.X, coord.Y, c);

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
