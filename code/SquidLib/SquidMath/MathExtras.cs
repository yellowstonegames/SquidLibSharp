using System;
using System.Runtime.CompilerServices;

namespace SquidLib.SquidMath {
    class MathExtras {
        /// <summary>
        /// Multiplies two 64-bit ulongs and returns the 128-bit result's high and low halves in a tuple.
        /// Credit goes to https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2 ,
        /// who ported this StackOverflow answer by catid https://stackoverflow.com/a/51587262 .
        /// </summary>
        /// <param name="x">any ulong</param>
        /// <param name="y">any ulong</param>
        /// <returns>A tuple of the high 64 bits of the product, then the low 64 bits</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (ulong Hi, ulong Lo) Multiply64(ulong x, ulong y) {
            unchecked {
                ulong lo = x * y;

                ulong x0 = (uint)x;
                ulong x1 = x >> 32;

                ulong y0 = (uint)y;
                ulong y1 = y >> 32;

                ulong p11 = x1 * y1;
                ulong p01 = x0 * y1;
                ulong p10 = x1 * y0;
                ulong p00 = x0 * y0;

                // 64-bit product + two 32-bit values
                ulong middle = p10 + (p00 >> 32) + (uint)p01;

                // 64-bit product + two 32-bit values
                ulong hi = p11 + (middle >> 32) + (p01 >> 32);

                return (hi, lo);
            }
        }
        /// <summary>
        /// Multiplies two 64-bit ulongs and returns the XOR of the high and low halves.
        /// Credit goes to https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2 ,
        /// who ported this StackOverflow answer by catid https://stackoverflow.com/a/51587262 .
        /// </summary>
        /// <param name="x">any ulong</param>
        /// <param name="y">any ulong</param>
        /// <returns>The high 64 bits of the product XORed with the low 64 bits</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MultiplyXor(ulong x, ulong y) {
            unchecked {
                ulong lo = x * y;

                ulong x0 = (uint)x;
                ulong x1 = x >> 32;

                ulong y0 = (uint)y;
                ulong y1 = y >> 32;

                ulong p11 = x1 * y1;
                ulong p01 = x0 * y1;
                ulong p10 = x1 * y0;
                ulong p00 = x0 * y0;

                // 64-bit product + two 32-bit values
                ulong middle = p10 + (p00 >> 32) + (uint)p01;

                // 64-bit product + two 32-bit values
                ulong hi = p11 + (middle >> 32) + (p01 >> 32);

                return hi ^ lo;
            }
        }
        /// <summary>
        /// Multiplies two 64-bit ulongs and returns the high 64 bits of the 128-bit product.
        /// Credit goes to https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2 ,
        /// who ported this StackOverflow answer by catid https://stackoverflow.com/a/51587262 .
        /// </summary>
        /// <param name="x">any ulong</param>
        /// <param name="y">any ulong</param>
        /// <returns>The high 64 bits of the product</returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MultiplyHigh(ulong x, ulong y) {
            unchecked {
                ulong x0 = (uint)x;
                ulong x1 = x >> 32;

                ulong y0 = (uint)y;
                ulong y1 = y >> 32;

                ulong p11 = x1 * y1;
                ulong p01 = x0 * y1;
                ulong p10 = x1 * y0;
                ulong p00 = x0 * y0;

                // 64-bit product + two 32-bit values
                ulong middle = p10 + (p00 >> 32) + (uint)p01;

                // 64-bit product + two 32-bit values
                return p11 + (middle >> 32) + (p01 >> 32);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDegrees(double radians) => radians * (180 / Math.PI);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDegrees(double x, double y) => ToDegrees(Math.Atan2(y, x));
    }
}
