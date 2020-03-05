using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Dictionary<Coord, Representation> Tiles { get; set; } = new Dictionary<Coord, Representation>();
    }

    public class BigMob {
        public Color DefaultColor { get; set; }
        public Coord Location { get; set; }
        public Direction Facing { get; set; } = Direction.Up;
        public Dictionary<Direction, MultiTileRepresentation> Reps { get; set; } = new Dictionary<Direction, MultiTileRepresentation>();

        /// <summary>
        /// Takes the input strings and converts them into a mapping for the object. Whitespaces are ignored.
        /// 
        /// The input will be internally value rotated so that the output looks the same orientation as normal input reads.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public void SetGlyphs(Direction dir, int controlX, int controlY, string[] input) {
            Reps[dir] = new MultiTileRepresentation {
                ControlPoint = Coord.Get(controlX, controlY)
            };
            for (int y = 0; y < input.Length; y++) {
                for (int x = 0; x < input[y].Length; x++) {
                    Reps[dir].Tiles[Coord.Get(x, y)] = new Representation() { Glyph = input[y][x], Color = DefaultColor };
                }
            }
        }
    }
}
