using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace RogueDelivery {
    public static class Prototype {
        private static BigMob wagon;
        public static BigMob Wagon {
            get {
                if (wagon is null) {
                    wagon = new BigMob {
                        DefaultColor = Color.SandyBrown,
                        Blocking = true
                    };
                    wagon.SetGlyphs(Direction.Up, 1, 1, new string[] {
                        " Ŏ ",
                        "┌╨┐",
                        "⍬∣⍬",
                        "└─┘"
                    });
                    wagon.SetGlyphs(Direction.Down, 1, 2, new string[] {
                        "┌─┐",
                        "⍬∣⍬",
                        "└╥┘",
                        " Ŏ "
                    });
                    wagon.SetGlyphs(Direction.Right, 2, 1, new string[] {
                        "┌⍬┐ ",
                        "│-╞Ŏ",
                        "└⍬┘ "
                    });
                    wagon.SetGlyphs(Direction.Left, 1, 1, new string[] {
                        " ┌⍬┐",
                        "Ŏ╡-│",
                        " └⍬┘"
                    });
                    wagon.Reps[Direction.Up].Tiles[Coord.Get(1, 0)].Color = Color.White;
                    wagon.Reps[Direction.Down].Tiles[Coord.Get(1, 3)].Color = Color.White;
                    wagon.Reps[Direction.Right].Tiles[Coord.Get(3, 1)].Color = Color.White;
                    wagon.Reps[Direction.Left].Tiles[Coord.Get(0, 1)].Color = Color.White;
                }
                return wagon;
            }
            private set => wagon = value;
        }

        private static BigMob log;
        public static BigMob Log {
            get {
                if (log is null) {
                    log = new BigMob {
                        DefaultColor = Color.BurlyWood,
                        Blocking = true
                    };
                    log.SetGlyphs(Direction.Up, 0, 0, new string[] {
                        "©",
                        "⋃"
                    });
                    log.SetGlyphs(Direction.Down, 0, 1, new string[] {
                        "⋂",
                        "©"
                    });
                    log.SetGlyphs(Direction.Right, 1, 0, new string[] {
                        "⊂©"
                    });
                    log.SetGlyphs(Direction.Left, 0, 0, new string[] {
                        "©⊃"
                    });
                }
                return log;
            }
            private set => log = value;
        }

        public static readonly Mob Grass = new Mob { Rep = new Representation { Glyph = ',', Color = Color.Green } };
        public static readonly Mob Pebble = new Mob { Rep = new Representation { Glyph = '*', Color = Color.SlateBlue } };
        public static readonly Mob Bush = new Mob { Rep = new Representation { Glyph = '"', Color = Color.GreenYellow } };
    }
}
