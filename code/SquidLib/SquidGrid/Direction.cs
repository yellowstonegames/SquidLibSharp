using System;
using SquidLib.SquidMath;

namespace SquidLib.SquidGrid {

    /**
     * Represents the eight grid directions and the deltaX, deltaY values associated
     * with those directions.
     *
     * The grid referenced has x positive to the right and y positive downwards on
     * screen.
     *
     * @author Eben Howard - http://squidpony.com - howard@squidpony.com
     */
    public enum Direction {

        Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight, None

    }

    public static class DirectionExtensions {
        private static IndexedDictionary<Direction, Coord> dictionary = new IndexedDictionary<Direction, Coord>() {
            {Direction.Up, SquidMath.Coord.Get(0, -1) },
            {Direction.Down, SquidMath.Coord.Get(0, 1) },
            {Direction.Left, SquidMath.Coord.Get(-1, 0) },
            {Direction.Right, SquidMath.Coord.Get(1, 0) },
            {Direction.UpLeft, SquidMath.Coord.Get(-1, -1) },
            {Direction.UpRight, SquidMath.Coord.Get(1, -1) },
            {Direction.DownLeft, SquidMath.Coord.Get(-1, 1) },
            {Direction.DownRight, SquidMath.Coord.Get(1, 1) },
            {Direction.None, SquidMath.Coord.Get(0, 0) }
        };

        public static int DeltaX(this Direction d) => dictionary[d].X;
        public static int DeltaY(this Direction d) => dictionary[d].Y;
        public static Coord Coord(this Direction d) => dictionary[d];

        /**
     * An array which holds only the four cardinal directions.
     */
        public static readonly Direction[] Cardinals = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        /**
         * An array which holds only the four cardinal directions in clockwise order.
         */
        public static readonly Direction[] CardinalsClockwise = { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
        /**
         * An array which holds only the four cardinal directions in counter-clockwise order.
         */
        public static readonly Direction[] CardinalsCounterclockwise = { Direction.Up, Direction.Left, Direction.Down, Direction.Right };
        /**
         * An array which holds only the four diagonal directions.
         */
        public static readonly Direction[] Diagonals = { Direction.UpLeft, Direction.UpRight, Direction.DownLeft, Direction.DownRight };
        /**
         * An array which holds all eight OUTWARDS directions.
         */
        public static readonly Direction[] Octals = { Direction.Up, Direction.Down, Direction.Left, Direction.Right, Direction.UpLeft, Direction.UpRight, Direction.DownLeft, Direction.DownRight };
        /**
         * An array which holds all eight OUTWARDS directions in clockwise order.
         */
        public static readonly Direction[] OctalsClockwise = { Direction.Up, Direction.UpRight, Direction.Right, Direction.DownRight, Direction.Down, Direction.DownLeft, Direction.Left, Direction.UpLeft };
        /**
         * An array which holds all eight OUTWARDS directions in counter-clockwise order.
         */
        public static readonly Direction[] OctalsCounterclockwise = { Direction.Up, Direction.UpLeft, Direction.Left, Direction.DownLeft, Direction.Down, Direction.DownRight, Direction.Right, Direction.UpRight };


        /**
         * Returns the direction that most closely matches the input.
         *
         * This can be used to get the primary magnitude intercardinal direction
         * from an origin point to an event point, such as a mouse click on a grid.
         *
         * If the point given is exactly on a boundary between directions then the
         * direction clockwise is returned.
         *
         * @param x
         * @param y
         * @return
         */
        public static Direction GetOctalDirection(int x, int y) {
            if (x == 0 && y == 0) {
                return Direction.None;
            }

            double degree = MathExtras.ToDegrees(x, y);
            degree += 450;//rotate to all positive and 0 is up
            degree %= 360;//normalize
            if (degree < 22.5) {
                return Direction.Up;
            } else if (degree < 67.5) {
                return Direction.UpRight;
            } else if (degree < 112.5) {
                return Direction.Right;
            } else if (degree < 157.5) {
                return Direction.DownRight;
            } else if (degree < 202.5) {
                return Direction.Down;
            } else if (degree < 247.5) {
                return Direction.DownLeft;
            } else if (degree < 292.5) {
                return Direction.Left;
            } else if (degree < 337.5) {
                return Direction.UpLeft;
            } else {
                return Direction.Up;
            }
        }

        /**
         * Gets an estimate at the correct direction that a position lies in given the distance towards it on the x and y
         * axes. If x and y are both between -1 and 1 inclusive, this will always be accurate, and should be faster than
         * {@link #getDirection(int, int)} by avoiding trigonometry or any other math on doubles. If at least one of x or y
         * is 0, then this will also be accurate and will produce either a cardinal direction or NONE if both are 0. If x
         * and y are both non-zero, this will always produce a diagonal, even if a cardinal direction should be more
         * accurate; this behavior may sometimes be desirable to detect when some position is even slightly off from a true
         * cardinal direction.
         * @param x the relative x position to find the direction towards
         * @param y the relative y position to find the direction towards
         * @return the Direction that x,y lies in, roughly; will always be accurate for arguments between -1 and 1 inclusive
         */
        public static Direction GetRoughOctalDirection(int x, int y) {
            x = x == 0 ? 0 : (x >> 31 | 1); // signum with less converting to/from float
            y = y == 0 ? 0 : (y >> 31 | 1); // signum with less converting to/from float
            switch (x) {
                case -1:
                    switch (y) {
                        case 1: return Direction.DownLeft;
                        case -1: return Direction.UpLeft;
                        default: return Direction.Left;
                    }
                case 1:
                    switch (y) {
                        case 1: return Direction.DownRight;
                        case -1: return Direction.UpRight;
                        default: return Direction.Right;
                    }
                default:
                    switch (y) {
                        case 1: return Direction.Down;
                        case -1: return Direction.Up;
                        default: return Direction.None;
                    }
            }
        }

        /**
         * Returns the direction that most closely matches the input.
         *
         * This can be used to get the primary magnitude cardinal direction from an
         * origin point to an event point, such as a mouse click on a grid.
         *
         * If the point given is directly diagonal then the direction clockwise is
         * returned.
         *
         * @param x
         * @param y
         * @return
         */
        public static Direction GetCardinalDirection(int x, int y) {
            if (x == 0 && y == 0) {
                return Direction.None;
            }

            int absx = Math.Abs(x);

            if (y > absx) {
                return Direction.Up;
            }

            int absy = Math.Abs(y);

            if (absy > absx) {
                return Direction.Down;
            }

            if (x > 0) {
                if (-y == x) {//on diagonal
                    return Direction.Down;
                }
                return Direction.Right;
            }

            if (y == x) {//on diagonal
                return Direction.Up;
            }
            return Direction.Left;

        }

        /**
         * @param from
         *            The starting point.
         * @param to
         *            The desired point to reach.
         * @return The direction to follow to go from {@code from} to {@code to}. It
         *         can be cardinal or diagonal.
         */
        public static Direction ToGoTo(Coord from, Coord to) => GetOctalDirection(to.X - from.X, to.Y - from.Y);

        /**
         * Returns the Direction one step clockwise including diagonals.
         *
         * If considering only Cardinal directions, calling this twice will get the
         * next clockwise cardinal direction.
         *
         * @return
         */
        public static Direction Clockwise(this Direction d) {
            switch (d) {
                case Direction.Up:
                    return Direction.UpRight;
                case Direction.Down:
                    return Direction.DownLeft;
                case Direction.Left:
                    return Direction.UpLeft;
                case Direction.Right:
                    return Direction.DownRight;
                case Direction.UpLeft:
                    return Direction.Up;
                case Direction.UpRight:
                    return Direction.Right;
                case Direction.DownLeft:
                    return Direction.Left;
                case Direction.DownRight:
                    return Direction.Down;
                case Direction.None:
                default:
                    return Direction.None;
            }
        }

        /**
         * Returns the Direction one step counterclockwise including diagonals.
         *
         * If considering only Cardinal directions, calling this twice will get the
         * next counterclockwise cardinal direction.
         *
         * @return
         */
        public static Direction CounterClockwise(this Direction d) {
            switch (d) {
                case Direction.Up:
                    return Direction.UpLeft;
                case Direction.Down:
                    return Direction.DownRight;
                case Direction.Left:
                    return Direction.DownLeft;
                case Direction.Right:
                    return Direction.UpRight;
                case Direction.UpLeft:
                    return Direction.Left;
                case Direction.UpRight:
                    return Direction.Up;
                case Direction.DownLeft:
                    return Direction.Down;
                case Direction.DownRight:
                    return Direction.Right;
                case Direction.None:
                default:
                    return Direction.None;
            }
        }

        /**
         * Returns the direction directly opposite of this one.
         *
         * @return
         */
        public static Direction Opposite(this Direction d) {
            switch (d) {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                case Direction.UpLeft:
                    return Direction.DownRight;
                case Direction.UpRight:
                    return Direction.DownLeft;
                case Direction.DownLeft:
                    return Direction.UpRight;
                case Direction.DownRight:
                    return Direction.UpLeft;
                case Direction.None:
                default:
                    return Direction.None;
            }
        }

        /**
         * @return Whether this is a diagonal move.
         */
        public static bool IsDiagonal(this Direction d) => (d.DeltaX() & d.DeltaY()) != 0;

        /**
         * @return Whether this is a cardinal-direction move.
         */
        public static bool IsCardinal(this Direction d) => (d.DeltaX() + d.DeltaY() & 1) != 0;

        /**
         * @return {@code true} if {@code this} has an upward component.
         */
        public static bool HasUp(this Direction d) {
            switch (d) {
                case Direction.Up:
                case Direction.UpLeft:
                case Direction.UpRight:
                    return true;
                case Direction.Down:
                case Direction.DownLeft:
                case Direction.DownRight:
                case Direction.Left:
                case Direction.None:
                case Direction.Right:
                default:
                    return false;
            }
        }

        /**
         * @return {@code true} if {@code this} has a downward component.
         */
        public static bool HasDown(this Direction d) {
            switch (d) {
                case Direction.Down:
                case Direction.DownLeft:
                case Direction.DownRight:
                    return true;
                case Direction.Left:
                case Direction.None:
                case Direction.Right:
                case Direction.Up:
                case Direction.UpLeft:
                case Direction.UpRight:
                default:
                    return false;
            }
        }

        /**
         * @return {@code true} if {@code this} has a left component.
         */
        public static bool HasLeft(this Direction d) {
            switch (d) {
                case Direction.DownLeft:
                case Direction.Left:
                case Direction.UpLeft:
                    return true;
                case Direction.Down:
                case Direction.DownRight:
                case Direction.None:
                case Direction.Right:
                case Direction.Up:
                case Direction.UpRight:
                default:
                    return false;
            }
        }

        /**
         * @return {@code true} if {@code this} has a right component.
         */
        public static bool HasRight(this Direction d) {
            switch (d) {
                case Direction.Right:
                case Direction.DownRight:
                case Direction.UpRight:
                    return true;
                case Direction.Down:
                case Direction.None:
                case Direction.Up:
                case Direction.DownLeft:
                case Direction.Left:
                case Direction.UpLeft:
                default:
                    return false;
            }
        }
    }
}
