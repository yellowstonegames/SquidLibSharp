using System;
using System.Collections.Generic;
using System.Drawing;
using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace RogueDelivery {
    public class Representation {
        public char Glyph { get; set; }
        public Color Color { get; set; }
    }

    public class Mob {
        public Coord Location { get; set; }
        public Representation Rep { get; set; }
    }

    public class MultiTileRepresentation {
        public Coord ControlPoint { get; set; }
        public Dictionary<Coord, Representation> Tiles { get; private set; } = new Dictionary<Coord, Representation>();
    }

    public class BigMob {
        public Color DefaultColor { get; set; }
        public Coord Location { get; set; }
        public Direction Facing { get; set; } = Direction.Up;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Dictionary<Direction, MultiTileRepresentation> Reps { get; private set; } = new Dictionary<Direction, MultiTileRepresentation>();

        /// <summary>
        /// Takes the input strings and converts them into a mapping for the object. Whitespaces are ignored.
        /// 
        /// The input will be internally value rotated so that the output looks the same orientation as normal input reads.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public void SetGlyphs(Direction dir, int controlX, int controlY, string[] input) {
            if (input is null) {
                throw new ArgumentNullException(nameof(input));
            } else if (input.Length == 0) {
                throw new ArgumentException("Cannot set glyphs without input.");
            }
            Reps[dir] = new MultiTileRepresentation {
                ControlPoint = Coord.Get(controlX, controlY)
            };
            Height = input.Length;
            Width = 0;
            for (int y = 0; y < input.Length; y++) {
                for (int x = 0; x < input[y].Length; x++) {
                    Width = Math.Max(Width, input[y].Length);
                    Reps[dir].Tiles[Coord.Get(x, y)] = new Representation() { Glyph = input[y][x], Color = DefaultColor };
                }
            }
            if (Width == 0) {
                throw new ArgumentException("Cannot set glyphs with empty input.");
            }
        }

        public Coord DrawingOffset() => Location - Reps[Facing].ControlPoint;
    }
}
