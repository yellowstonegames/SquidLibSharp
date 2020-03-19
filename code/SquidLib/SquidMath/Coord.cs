using System;

namespace SquidLib.SquidMath {
    //TODO: more methods
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Can't use C# 8.0, so no readonly properties.")]
    public readonly struct Coord : IEquatable<Coord> {
        public readonly int X;
        public readonly int Y;

        public Coord(int x, int y) {
            X = x;
            Y = y;
        }

        public Coord ChangeX(int x) => Get(x, Y);

        public Coord ChangeY(int y) => Get(X, y);

        public Coord Add(int x, int y) => Get(X + x, Y + y);

        public static Coord Get(int x, int y) => new Coord(x, y);

        public override bool Equals(object obj) => obj is Coord coord && Equals(coord);

        public bool Equals(Coord other) => X == other.X && Y == other.Y;

        public override int GetHashCode() {
            unchecked {
                // this is a medium-high-quality hashCode built around the Rosenberg-Strong pairing function.
                int r = X << 1 ^ X >> 31;
                int s = Y << 1 ^ Y >> 31;
                uint t = (uint)(((r >= s ? r * r + r + r - s : s * s + r) ^ -776648139) * 0x9E375 + r + s);
                return (int)(t ^ t >> 11 ^ t << 15);
            }
        }

        public static Coord operator +(Coord left, Coord right) => Get(left.X + right.X, left.Y + right.Y);
        public static Coord Add(Coord left, Coord right) => left + right;
        public static Coord operator -(Coord left, Coord right) => Get(left.X - right.X, left.Y - right.Y);
        public static Coord Subtract(Coord left, Coord right) => left - right;

        public static bool operator ==(Coord left, Coord right) => left.Equals(right);

        public static bool operator !=(Coord left, Coord right) => !(left == right);

        override
        public string ToString() => $"[{X},{Y}]";
    }

    public struct Coord3D : IEquatable<Coord3D> {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Coord3D(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }


        public Coord3D ChangeX(int x) => Get(x, Y, Z);

        public Coord3D ChangeY(int y) => Get(X, y, Z);

        public Coord3D ChangeZ(int z) => Get(X, Y, z);

        public static Coord3D Get(int x, int y, int z) => new Coord3D(x, y, z);

        override
        public string ToString() => $"[{X},{Y},{Z}]";

        public override bool Equals(object obj) => obj is Coord3D d && Equals(d);

        public bool Equals(Coord3D other) => X == other.X && Y == other.Y && Z == other.Z;

        public override int GetHashCode() {
            // uses something like the R2 sequence, but structured like a normal generated GetHashCode().
            unchecked {
                return -1934021721 * (-1412856951 * (-776648141 * X + Y) + Z);
            }
        }

        public static Coord3D operator +(Coord3D left, Coord3D right) => Get(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        public static Coord3D Add(Coord3D left, Coord3D right) => left + right;
        public static Coord3D operator -(Coord3D left, Coord3D right) => Get(left.X - right.X, left.Y - right.Y, left.Z + right.Z);
        public static Coord3D Subtract(Coord3D left, Coord3D right) => left - right;


        public static bool operator ==(Coord3D left, Coord3D right) => left.Equals(right);

        public static bool operator !=(Coord3D left, Coord3D right) => !(left == right);
    }
}
