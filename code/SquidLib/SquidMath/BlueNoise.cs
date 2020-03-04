using System;
using System.Collections.Generic;
using System.Text;

namespace SquidLib.SquidMath {
    public static class BlueNoise {
        public static readonly byte[] RawNoise = Properties.Resources.BlueNoise;

        public static byte Get(int x, int y) => RawNoise[(y << 6 & 0xFC0U) | (x & 0x3FU)];
        public static byte Get(int x, int y, int layer) => RawNoise[(y << 6 & 0xFC0U) | (x & 0x3FU) | ((layer & 63U) << 12)];
        public static byte GetSeeded(int x, int y, uint seed) {
            uint s = seed & 63U;
            seed ^= ((uint)x >> 5) * 0x1827F5U + 0xC13FA9A9U ^ ((uint)y >> 5 ^ 0x91E10DA5U) * 0x123C23U;
            return (byte)(RawNoise[((y << 6) & 0xFC0U) | (x & 0x3FU) | (s << 12)] ^
                    ((seed ^ (seed << 19 | seed >> 13) ^ (seed << 5 | seed >> 27) ^ 0xD1B54A35U) * 0x125493U >> 15 & 63)
                    ^ (x + x + y >> 2 & 0x3FU) ^ (x - y - y >> 2 & 0x3FU));
        }
        public static byte GetSeeded(int x, int y, int layer, uint seed) {
            seed ^= ((uint)x >> 5) * 0x1827F5U + 0xC13FA9A9U ^ ((uint)y >> 5 ^ 0x91E10DA5U) * 0x123C23U;
            return (byte)(RawNoise[((y << 6) & 0xFC0U) | (x & 0x3FU) | ((layer & 63U) << 12)] ^
                    ((seed ^ (seed << 19 | seed >> 13) ^ (seed << 5 | seed >> 27) ^ 0xD1B54A35U) * 0x125493U >> 15 & 63)
                    ^ (x + x + y >> 2 & 0x3FU) ^ (x - y - y >> 2 & 0x3FU));
        }
    }
}
