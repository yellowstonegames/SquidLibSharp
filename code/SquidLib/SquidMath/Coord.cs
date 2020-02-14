using System;

namespace SquidLib.SquidMath {
    // TODO - make this not a stub class
    public struct Coord : IEquatable<Coord> {
        public readonly int x, y;

        public Coord(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Coord setX(int x) => get(x, y);

        public Coord setY(int y) => get(x, y);

        public static Coord get(int x, int y) => new Coord(x, y);

        public override bool Equals(object obj) {
            return obj is Coord coord && Equals(coord);
        }

        public bool Equals(Coord other) {
            return x == other.x &&
                   y == other.y;
        }

        public override int GetHashCode() {
            unchecked {
                int r = x << 1 ^ x >> 31;
                int s = y << 1 ^ y >> 31;
                uint t = (uint)(((r >= s ? r * r + r + r - s : s * s + r) ^ -776648139) * 0x9E375 + r + s);
                return (int)(t ^ t >> 11 ^ t << 15);
            }
        }

        public static bool operator ==(Coord left, Coord right) {
            return left.Equals(right);
        }

        public static bool operator !=(Coord left, Coord right) {
            return !(left == right);
        }

        override
        public String ToString() => $"[{x},{y}]";
    }

    public struct Coord3D {
        public readonly int x, y, z;

        public Coord3D(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public Coord3D setX(int x) => get(x, y, z);

        public Coord3D setY(int y) => get(x, y, z);

        public Coord3D setZ(int z) => get(x, y, z);

        public static Coord3D get(int x, int y, int z) => new Coord3D(x, y, z);

        override
        public String ToString() => $"[{x},{y},{z}]";
    }
}
