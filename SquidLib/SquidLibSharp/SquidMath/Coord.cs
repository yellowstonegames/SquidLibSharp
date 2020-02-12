using System;
using System.Collections.Generic;
using System.Transactions;

namespace SquidLib.SquidMath
{
    // TODO - make this not a stub class
    public class Coord
    {
        public int x, y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Coord setX(int x) => get(x, y);

        public Coord setY(int y) => get(x, y);

        public static Coord get(int x, int y) => new Coord(x, y);
    }

    public class Coord3D : Coord
    {
        public int z;

        public Coord3D(int x, int y, int z) : base(x, y)
        {
            this.z = z;
        }


        public Coord3D setX(int x) => get(x, y, z);

        public Coord3D setY(int y) => get(x, y, z);

        public Coord3D setZ(int z) => get(x, y, z);

        public static Coord3D get(int x, int y, int z) => new Coord3D(x, y, z);
    }
}
