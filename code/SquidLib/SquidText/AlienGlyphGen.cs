using System;
using System.Collections.Generic;

using SquidLib.SquidGrid;
using SquidLib.SquidMath;
using SquidLib.SquidMisc;

namespace SquidLib.SquidText {

    /// <summary>
    /// This class provides some utility to generate glyphs that are randomly generated and have strong angles to them.
    /// The glyphs generated tend to look like alien writing.
    /// </summary>
    public class AlienGlyphGenerator {
        private const int defaultPoints = 3;
        private readonly Direction[] strokeDirections = new Direction[]{
            Direction.Down,
            Direction.DownRight,
            Direction.Right
        };

        private RNG rng;
        private Elias los;

        public AlienGlyphGenerator(RNG rng) {
            this.rng = rng;
            los = new Elias();
        }

        /// <summary>
        /// Creates a series of lines that don't conform to a grid.
        /// 
        /// If the <paramref name="lengthThreshold"/> is set then the returned list may contain less than the minimum number of strokes
        /// since some strokes will be removed for not reaching that threshold.
        /// 
        /// Because these strokes' anchor points are distributed randomly within this space, the coverage in the center is
        /// more likely than the edges.
        /// </summary>
        /// <param name="width">The width within which the line will be created.</param>
        /// <param name="height">The height within which the line will be created.</param>
        /// <param name="minStrokes">The minimum number of strokes to create (inclusive).</param>
        /// <param name="maxStrokes">The maximum number of strokes to create (inclusive).</param>
        /// <param name="lengthThreshold">The minimum length (inclusive) that a stroke must be to be returned.</param>
        /// <returns></returns>
        public List<List<Coord>> IrregularStrokes(int width, int height, int minStrokes, int maxStrokes, int lengthThreshold = 1) {
            if (height < 1) {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width of stroke area must be at least 1.");
            }
            if (height < 1) {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height of stroke area must be at least 1.");
            }
            if (minStrokes < 0) {
                throw new ArgumentOutOfRangeException(nameof(minStrokes), minStrokes, "Minimum number of strokes must be at least 0.");
            }
            if (maxStrokes < minStrokes) {
                throw new ArgumentException($"Max strokes {maxStrokes} must be greater than or equal to min strokes {minStrokes}.", nameof(maxStrokes));
            }

            List<List<Coord>> strokes = new List<List<Coord>>();
            int n = rng.NextInt(minStrokes, maxStrokes + 1);
            for (int stroke = 0; stroke < n; stroke++) {
                var (origin, target) = IrregularStroke(width, height);
                List<Coord> line = new List<Coord>(los.Line(origin, target));
                if (line.Count >= lengthThreshold) {
                    strokes.Add(line);
                }
            }
            return strokes;
        }

        /// <summary>
        /// Returns a pair of Coord objects chosen randomly from within the width and height provided, ensuring that the
        /// first Coord is up and to the left (or in line) with the second Coord. Additionally the second Coord will never
        /// be at the same point as the first.
        /// </summary>
        /// <param name="width">The amount of width within which to place the stroke.</param>
        /// <param name="height">The amount of height within which to place the stroke.</param>
        /// <returns></returns>
        public (Coord origin, Coord target) IrregularStroke(int width, int height) {
            if (height < 1) {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width of stroke area must be at least 1.");
            }
            if (height < 1) {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height of stroke area must be at least 1.");
            }

            Coord origin, target;
            do {
                origin = Coord.Get(rng.NextInt(width - 1), rng.NextInt(height - 1));
                target = Coord.Get(rng.NextInt(origin.X, width), rng.NextInt(origin.Y, height));
            } while (origin == target);
            return (origin, target);
        }

        public List<List<Coord>> OrthogonalStrokes(int strokePoints = defaultPoints, double pointDistance = 1.0) {
            Dictionary<Coord, HashSet<Direction>> strokeDirections = OrthogonalStrokesGrid(strokePoints, strokePoints, 1, 3, 1.0 / 9.0);
            List<List<Coord>> result = new List<List<Coord>>();

            foreach (var (origin, directions) in strokeDirections) {
                foreach (var direction in directions) {
                    if (direction == Direction.None) {
                        continue;
                    }
                    Coord adjustedOrigin = Coord.Get((int)(origin.X * pointDistance), (int)(origin.Y * pointDistance));
                    Coord target = Coord.Get(
                        (int)(adjustedOrigin.X + pointDistance * direction.DeltaX()),
                        (int)(adjustedOrigin.Y + pointDistance * direction.DeltaY()));
                    List<Coord> line = new List<Coord>(los.Line(adjustedOrigin, target));
                    result.Add(line);
                }
            }
            return result;
        }

        public Dictionary<Coord, HashSet<Direction>> OrthogonalStrokesGrid(int width, int height, int minStrokesPerCell, int maxStrokesPerCell, double emptyChance) {
            Dictionary<Coord, HashSet<Direction>> result = new Dictionary<Coord, HashSet<Direction>>();
            for (int x = 0; x < width - 1; x++) {
                for (int y = 0; y < height - 1; y++) {
                    Coord coordinate = Coord.Get(x, y);
                    HashSet<Direction> selected = new HashSet<Direction>();

                    // determine number of strokes from current cell's origin
                    int strokes = rng.NextInt(minStrokesPerCell, maxStrokesPerCell + 1);
                    for (int i = 0; i < strokes; i++) {
                        // check if this stroke should be "empty"
                        if (rng.NextDouble() < emptyChance) {
                            continue;
                        }
                        selected.Add(rng.RandomElement(strokeDirections));
                    }
                    result[coordinate] = selected;
                }
            }
            return result;
        }
    }
}
