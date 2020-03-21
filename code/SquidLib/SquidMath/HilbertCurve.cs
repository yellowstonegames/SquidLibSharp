namespace SquidLib.SquidMath {
    public static class HilbertCurve {
        public static readonly byte[] Hilbert2DX = Properties.Resources.Hilbert2D_X;
        public static readonly byte[] Hilbert2DY = Properties.Resources.Hilbert2D_Y;
        public static readonly byte[] Hilbert2DDist = Properties.Resources.Hilbert2D_Dist;

        public static int Distance(int x, int y) {
            int idx = (x | y << 8) << 1 & 0x1FFFE;
            return Hilbert2DDist[idx] | Hilbert2DDist[idx + 1] << 8;
        }
        public static Coord Get(int distance) => Coord.Get(Hilbert2DX[distance & 0xFFFF], Hilbert2DY[distance & 0xFFFF]);
    }
}
