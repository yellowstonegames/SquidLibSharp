using System;

namespace SquidLib.SquidMath {
    //TODO: more methods
    public struct Coord : IEquatable<Coord> {
        public readonly int x, y;

        public Coord(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Coord ChangeX(int x) => Get(x, y);

        public Coord ChangeY(int y) => Get(x, y);

        public static Coord Get(int x, int y) => new Coord(x, y);

        public override bool Equals(object obj) {
            return obj is Coord coord && Equals(coord);
        }

        public bool Equals(Coord other) {
            return x == other.x &&
                   y == other.y;
        }

        public override int GetHashCode() {
            unchecked {
                // this is a medium-high-quality hashCode built around the Rosenberg-Strong pairing function.
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

    public struct Coord3D : IEquatable<Coord3D> {
        public readonly int x, y, z;

        public Coord3D(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }


        public Coord3D ChangeX(int x) => Get(x, y, z);

        public Coord3D ChangeY(int y) => Get(x, y, z);

        public Coord3D ChangeZ(int z) => Get(x, y, z);

        public static Coord3D Get(int x, int y, int z) => new Coord3D(x, y, z);

        override
        public String ToString() => $"[{x},{y},{z}]";

        public override bool Equals(object obj) {
            return obj is Coord3D d && Equals(d);
        }

        public bool Equals(Coord3D other) {
            return x == other.x &&
                   y == other.y &&
                   z == other.z;
        }

        public override int GetHashCode() {
            // uses something like the R2 sequence, but structured like a normal generated GetHashCode().
            unchecked {
                return -1934021721 * (-1412856951 * (-776648141 * x + y) + z);
            }
        }

        public static bool operator ==(Coord3D left, Coord3D right) {
            return left.Equals(right);
        }

        public static bool operator !=(Coord3D left, Coord3D right) {
            return !(left == right);
        }
    }
}
