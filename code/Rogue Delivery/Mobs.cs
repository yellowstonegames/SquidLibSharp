using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using SquidLib.SquidGrid;
using SquidLib.SquidMath;

namespace RogueDelivery {
    public class Representation {
        public char Glyph { get; set; }
        public Color Color { get; set; }

        public Representation() { }

        public Representation(Representation other) {
            if (other is null) {
                throw new ArgumentNullException(nameof(other));
            }
            Glyph = other.Glyph;
            Color = other.Color;
        }
    }

    public class Mob {
        public Coord Location { get; set; }
        public Representation Rep { get; set; }

        public Mob() { }

        public Mob(Mob other) {
            if (other is null) {
                throw new ArgumentNullException(nameof(other));
            }
            Location = other.Location;
            Rep = new Representation(other.Rep);
        }
    }

    public class MultiTileRepresentation {
        public Coord ControlPoint { get; set; }
        public Dictionary<Coord, Representation> Tiles { get; private set; } = new Dictionary<Coord, Representation>();

        public MultiTileRepresentation() { }

        public MultiTileRepresentation(MultiTileRepresentation other) {
            if (other is null) {
                throw new ArgumentNullException(nameof(other));
            }
            ControlPoint = other.ControlPoint;
            foreach (var (coord, rep) in other.Tiles) {
                Tiles[coord] = new Representation(rep);
            }
        }
    }

    public class BigMob {
        public Color DefaultColor { get; set; }
        public Coord Location { get; set; }
        public Direction Facing { get; set; } = Direction.Up;
        public Dictionary<Direction, MultiTileRepresentation> Reps { get; private set; } = new Dictionary<Direction, MultiTileRepresentation>();

        public BigMob() { }

        public BigMob(BigMob other) {
            if (other is null) {
                throw new ArgumentNullException(nameof(other));
            }
            DefaultColor = other.DefaultColor;
            Location = other.Location;
            Facing = other.Facing;
            foreach (var (dir, mtr) in other.Reps) {
                Reps[dir] = new MultiTileRepresentation(mtr);
            }
        }

        /// <summary>
        /// Takes the input strings and converts them into a mapping for the object. Whitespaces are ignored and not included.
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
            for (int y = 0; y < input.Length; y++) {
                for (int x = 0; x < input[y].Length; x++) {
                    char working = input[y][x];
                    if (char.IsWhiteSpace(working)) {
                        continue;
                    }
                    Reps[dir].Tiles[Coord.Get(x, y)] = new Representation() { Glyph = working, Color = DefaultColor };
                }
            }
        }

        public Coord DrawingOffset() => Location - Reps[Facing].ControlPoint;
    }
}
