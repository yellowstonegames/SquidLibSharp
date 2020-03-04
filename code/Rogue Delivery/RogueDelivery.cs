using System;
using System.Collections.Generic;
using System.Linq;

using BearLib;
using ColorHelper;
using SquidLib;
using SquidLib.SquidMath;

namespace RogueDelivery {
    class RogueDelivery {
        private bool keepRunning = true;
        private RNG rng;

        private int width = 120, height = 40;

        private Dictionary<Coord, char> borders = new Dictionary<Coord, char>();

        // currently, you can view a rippling water area in ASCII, and can press Escape to close.
        static void Main() {
            var rd = new RogueDelivery(); // start up the primary system
            rd.Start();
        }

        private RogueDelivery() {
            rng = new RNG();

            // init display style
            for (int x = 1; x < width - 1; x++) {
                borders[Coord.Get(x, 0)] = '─';
                borders[Coord.Get(x, height - 1)] = '─';
            }
            for (int y = 1; y < height - 1; y++) {
                borders[Coord.Get(0, y)] = '│';
                borders[Coord.Get(width - 1, y)] = '│';
            }
            borders[Coord.Get(0, 0)] = '┌';
            borders[Coord.Get(width - 1, 0)] = '┐';
            borders[Coord.Get(0, height - 1)] = '└';
            borders[Coord.Get(width - 1, height - 1)] = '┘';

            // Setup terminal
            Terminal.Open();
            Terminal.Set(
                $"window: title='Rogue Delivery', size={width}x{height};" +
                $"output: vsync=false;" +
                $"font: Iosevka.ttf, size=9x21, hinting=autohint"
                );
            ColorHelper.BltColor.LoadAurora();
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
                        case int val:
                            Terminal.Color(Terminal.ColorFromName(rng.RandomElement(BltColor.AuroraNames)));
                            Terminal.Put(rng.NextInt(width), rng.NextInt(height), ArrayTools.LetterAt(rng.NextInt()));
                            break;
                        default:

                    }
                    DrawMap();
                    Terminal.Refresh();
                }
            }
        }

        private void DrawMap() {
            PaintBorder();
        }

        private void PaintBorder() {
            foreach (var kvp in borders) {
                Put(kvp.Key, kvp.Value);
            }
        }

        private static void Put(Coord coord, char c) => Terminal.Put(coord.X, coord.Y, c);

    }
}
