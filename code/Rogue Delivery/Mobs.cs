using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using SquidLib.SquidGrid;
using SquidLib.SquidMath;
using SquidLib.SquidMisc;

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

        public static implicit operator MultiTileRepresentation(Representation r) {
            var mtr = new MultiTileRepresentation() {
                ControlPoint = Coord.Get(0, 0)
            };
            mtr.Tiles[Coord.Get(0, 0)] = new Representation(r);
            return mtr;
        }
        public MultiTileRepresentation ToMultiTileRepresentation() => this;
    }

    public class Mob {
        public Coord Location { get; set; }
        public Representation Rep { get; set; }

        public bool Blocking { get; set; }

        public Mob() { }

        public Mob(Mob other) {
            if (other is null) {
                throw new ArgumentNullException(nameof(other));
            }
            Location = other.Location;
            Rep = new Representation(other.Rep);
            Blocking = other.Blocking;
        }

        public static implicit operator BigMob(Mob m) {
            if (m is null) {
                return (BigMob)null;
            }
            BigMob ret = new BigMob() {
                DefaultColor = m.Rep.Color,
                Location = m.Location
            };
            foreach (Direction dir in DirectionTools.Cardinals) {
                var mtr = new MultiTileRepresentation() {
                    ControlPoint = Coord.Get(0, 0)
                };
                mtr.Tiles[Coord.Get(0, 0)] = new Representation(m.Rep);
                ret.Reps[dir] = mtr;
            }
            return ret;
        }
        public BigMob ToBigMob() => this;
    }

    public class MultiTileRepresentation {
        public Coord ControlPoint { get; set; }
        public Dictionary<Coord, Representation> Tiles { get; private set; } = new Dictionary<Coord, Representation>();
        public Rectangle OuterBounds { get; private set; }

        public MultiTileRepresentation() { }

        public MultiTileRepresentation(MultiTileRepresentation other) {
            if (other is null) {
                throw new ArgumentNullException(nameof(other));
            }
            ControlPoint = other.ControlPoint;
            foreach (var (coord, rep) in other.Tiles) {
                Tiles[coord] = new Representation(rep);
            }
            OuterBounds = new Rectangle(other.OuterBounds.Location, other.OuterBounds.Size);
        }

        /// <summary>
        /// Adds the provided location and representation to this tile representation.
        /// 
        /// In order for the OuterBounds to be tracked properly, this should only be used after ControlPoint has been set.
        /// </summary>
        /// <param name="x">The x position relative to the ControlPoint</param>
        /// <param name="y">The y position relative to the ControlPoint</param>
        /// <param name="glyph"></param>
        /// <param name="color"></param>
        public void Put(int x, int y, char glyph, Color color) {
            Tiles[Coord.Get(x, y)] = new Representation { Glyph = glyph, Color = color };

            // Check for if the bounds needs to be adjusted
            if (!OuterBounds.Contains(x, y)) {
                OuterBounds = new Rectangle(Math.Min(x, OuterBounds.X), Math.Min(y, OuterBounds.Y), Math.Abs(x - OuterBounds.X), Math.Abs(y - OuterBounds.Y));
            }
        }
    }

    public class BigMob {
        public Color DefaultColor { get; set; }
        public Coord Location { get; set; }
        public Direction Facing { get; set; } = Direction.Up;
        public Dictionary<Direction, MultiTileRepresentation> Reps { get; private set; } = new Dictionary<Direction, MultiTileRepresentation>();
        public Dictionary<Direction, Rectangle> Bounds { get; private set; } = new Dictionary<Direction, Rectangle>();

        public bool Blocking { get; set; }

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
            Blocking = other.Blocking;
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
                    Reps[dir].Put(x, y, working, DefaultColor);
                }
            }
        }

        public Coord DrawingOffset() => Location - Reps[Facing].ControlPoint;
    }
}
