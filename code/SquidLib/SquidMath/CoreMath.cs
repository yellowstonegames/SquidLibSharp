
using System;
using System.Runtime.CompilerServices;

namespace SquidLib.SquidMath {
    /// <summary>
    /// Utility class; has various small math functions that other classes need to work.
    /// </summary>
    public class CoreMath {
        private const short FN_INLINE = 256;

        /**
         * Like {@link Math#floor}, but returns a long.
         * Doesn't consider weird doubles like INFINITY and NaN.
         *
         * @param t the double to find the floor for
         * @return the floor of t, as a long
         */
        [MethodImpl(FN_INLINE)]
        public static long LongFloor(double t) {
            return t >= 0.0 ? (long)t : (long)t - 1L;
        }
        /**
         * Like {@link Math#floor(double)}, but takes a float and returns a long.
         * Doesn't consider weird floats like INFINITY and NaN.
         *
         * @param t the double to find the floor for
         * @return the floor of t, as a long
         */
        [MethodImpl(FN_INLINE)]
        public static long LongFloor(float t) {
            return t >= 0f ? (long)t : (long)t - 1L;
        }
        /**
         * Like {@link Math#floor(double)} , but returns an int.
         * Doesn't consider weird doubles like INFINITY and NaN.
         * @param t the float to find the floor for
         * @return the floor of t, as an int
         */
        [MethodImpl(FN_INLINE)]
        public static int FastFloor(double t) {
            return t >= 0.0 ? (int)t : (int)t - 1;
        }
        /**
         * Like {@link Math#floor(double)}, but takes a float and returns an int.
         * Doesn't consider weird floats like INFINITY and NaN.
         * @param t the float to find the floor for
         * @return the floor of t, as an int
         */
        [MethodImpl(FN_INLINE)]
        public static int FastFloor(float t) {
            return t >= 0f ? (int)t : (int)t - 1;
        }

                /**
         * Gets a 64-bit point hash of a 2D point (x and y are both longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param s the state/seed; any long
         * @return 64-bit hash of the x,y point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static ulong HashAll(long X, long Y, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong s = (ulong)S;
            y += s * 0xD1B54A32D192ED03UL;
            x += y * 0xABC98388FB8FAC03UL;
            s += x * 0x8CB92BA72F3D8DD7UL;
            return ((s = (s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) ^ s >> 25);
        }

        /**
         * Gets a 64-bit point hash of a 3D point (x, y, and z are all longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param s the state/seed; any long
         * @return 64-bit hash of the x,y,z point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static ulong HashAll(long X, long Y, long Z, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong s = (ulong)S;
            z += s * 0xDB4F0B9175AE2165UL;
            y += z * 0xBBE0563303A4615FUL;
            x += y * 0xA0F2EC75A1FE1575UL;
            s += x * 0x89E182857D9ED689UL;
            return ((s = (s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) ^ s >> 25);
        }

        /**
         * Gets a 64-bit point hash of a 4D point (x, y, z, and w are all longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param w w position; any long
         * @param s the state; any long
         * @return 64-bit hash of the x,y,z,w point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static ulong HashAll(long X, long Y, long Z, long W, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong w = (ulong)W;
            ulong s = (ulong)S;
            w += s * 0xE19B01AA9D42C633UL;
            z += w * 0xC6D1D6C8ED0C9631UL;
            y += z * 0xAF36D01EF7518DBBUL;
            x += y * 0x9A69443F36F710E7UL;
            s += x * 0x881403B9339BD42DUL;
            return ((s = (s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) ^ s >> 25);
        }

        /**
         * Gets a 64-bit point hash of a 6D point (x, y, z, w, u, and v are all longs) and a state/seed as a long. This
         * point hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param w w position; any long
         * @param u u position; any long
         * @param v v position; any long
         * @param s the state; any long
         * @return 64-bit hash of the x,y,z,w,u,v point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static ulong HashAll(long X, long Y, long Z, long W, long U, long V, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong w = (ulong)W;
            ulong u = (ulong)U;
            ulong v = (ulong)V;
            ulong s = (ulong)S;
            v += s * 0xE95E1DD17D35800DUL;
            u += v * 0xD4BC74E13F3C782FUL;
            w += u * 0xC1EDBC5B5C68AC25UL;
            z += w * 0xB0C8AC50F0EDEF5DUL;
            y += z * 0xA127A31C56D1CDB5UL;
            x += y * 0x92E852C80D153DB3UL;
            s += x * 0x85EB75C3024385C3UL;
            return ((s = (s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) ^ s >> 25);
        }
        /**
         * Gets an 8-bit point hash of a 2D point (x and y are both longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param s the state/seed; any long
         * @return 8-bit hash of the x,y point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash256(long X, long Y, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong s = (ulong)S;
            y += s * 0xD1B54A32D192ED03UL;
            x += y * 0xABC98388FB8FAC03UL;
            s += x * 0x8CB92BA72F3D8DD7UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 56);
        }

        /**
         * Gets an 8-bit point hash of a 3D point (x, y, and z are all longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param s the state/seed; any long
         * @return 8-bit hash of the x,y,z point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash256(long X, long Y, long Z, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong s = (ulong)S;
            z += s * 0xDB4F0B9175AE2165UL;
            y += z * 0xBBE0563303A4615FUL;
            x += y * 0xA0F2EC75A1FE1575UL;
            s += x * 0x89E182857D9ED689UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 56);
        }

        /**
         * Gets an 8-bit point hash of a 4D point (x, y, z, and w are all longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param w w position; any long
         * @param s the state; any long
         * @return 8-bit hash of the x,y,z,w point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash256(long X, long Y, long Z, long W, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong w = (ulong)W;
            ulong s = (ulong)S;
            w += s * 0xE19B01AA9D42C633UL;
            z += w * 0xC6D1D6C8ED0C9631UL;
            y += z * 0xAF36D01EF7518DBBUL;
            x += y * 0x9A69443F36F710E7UL;
            s += x * 0x881403B9339BD42DUL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 56);
        }

        /**
         * Gets an 8-bit point hash of a 6D point (x, y, z, w, u, and v are all longs) and a state/seed as a long. This
         * point hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param w w position; any long
         * @param u u position; any long
         * @param v v position; any long
         * @param s the state; any long
         * @return 8-bit hash of the x,y,z,w,u,v point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash256(long X, long Y, long Z, long W, long U, long V, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong w = (ulong)W;
            ulong u = (ulong)U;
            ulong v = (ulong)V;
            ulong s = (ulong)S;
            v += s * 0xE95E1DD17D35800DUL;
            u += v * 0xD4BC74E13F3C782FUL;
            w += u * 0xC1EDBC5B5C68AC25UL;
            z += w * 0xB0C8AC50F0EDEF5DUL;
            y += z * 0xA127A31C56D1CDB5UL;
            x += y * 0x92E852C80D153DB3UL;
            s += x * 0x85EB75C3024385C3UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 56);
        }
        /**
         * Gets a 5-bit point hash of a 2D point (x and y are both longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param s the state/seed; any long
         * @return 5-bit hash of the x,y point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash32(long X, long Y, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong s = (ulong)S;
            y += s * 0xD1B54A32D192ED03UL;
            x += y * 0xABC98388FB8FAC03UL;
            s += x * 0x8CB92BA72F3D8DD7UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 59);
        }

        /**
         * Gets a 5-bit point hash of a 3D point (x, y, and z are all longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param s the state/seed; any long
         * @return 5-bit hash of the x,y,z point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash32(long X, long Y, long Z, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong s = (ulong)S;
            z += s * 0xDB4F0B9175AE2165UL;
            y += z * 0xBBE0563303A4615FUL;
            x += y * 0xA0F2EC75A1FE1575UL;
            s += x * 0x89E182857D9ED689UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 59);
        }

        /**
         * Gets a 5-bit point hash of a 4D point (x, y, z, and w are all longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param w w position; any long
         * @param s the state; any long
         * @return 5-bit hash of the x,y,z,w point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash32(long X, long Y, long Z, long W, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong w = (ulong)W;
            ulong s = (ulong)S;
            w += s * 0xE19B01AA9D42C633UL;
            z += w * 0xC6D1D6C8ED0C9631UL;
            y += z * 0xAF36D01EF7518DBBUL;
            x += y * 0x9A69443F36F710E7UL;
            s += x * 0x881403B9339BD42DUL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 59);
        }

        /**
         * Gets a 5-bit point hash of a 6D point (x, y, z, w, u, and v are all longs) and a state/seed as a long. This
         * point hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param w w position; any long
         * @param u u position; any long
         * @param v v position; any long
         * @param s the state; any long
         * @return 5-bit hash of the x,y,z,w,u,v point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash32(long X, long Y, long Z, long W, long U, long V, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong w = (ulong)W;
            ulong u = (ulong)U;
            ulong v = (ulong)V;
            ulong s = (ulong)S;
            v += s * 0xE95E1DD17D35800DUL;
            u += v * 0xD4BC74E13F3C782FUL;
            w += u * 0xC1EDBC5B5C68AC25UL;
            z += w * 0xB0C8AC50F0EDEF5DUL;
            y += z * 0xA127A31C56D1CDB5UL;
            x += y * 0x92E852C80D153DB3UL;
            s += x * 0x85EB75C3024385C3UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 59);
        }
        
        /**
         * Gets a 6-bit point hash of a 2D point (x and y are both longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param s the state/seed; any long
         * @return 6-bit hash of the x,y point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash64(long X, long Y, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong s = (ulong)S;
            y += s * 0xD1B54A32D192ED03UL;
            x += y * 0xABC98388FB8FAC03UL;
            s += x * 0x8CB92BA72F3D8DD7UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 58);
        }

        /**
         * Gets a 6-bit point hash of a 3D point (x, y, and z are all longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param s the state/seed; any long
         * @return 6-bit hash of the x,y,z point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash64(long X, long Y, long Z, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong s = (ulong)S;
            z += s * 0xDB4F0B9175AE2165UL;
            y += z * 0xBBE0563303A4615FUL;
            x += y * 0xA0F2EC75A1FE1575UL;
            s += x * 0x89E182857D9ED689UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 58);
        }

        /**
         * Gets a 6-bit point hash of a 4D point (x, y, z, and w are all longs) and a state/seed as a long. This point
         * hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param w w position; any long
         * @param s the state; any long
         * @return 6-bit hash of the x,y,z,w point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash64(long X, long Y, long Z, long W, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong w = (ulong)W;
            ulong s = (ulong)S;
            w += s * 0xE19B01AA9D42C633UL;
            z += w * 0xC6D1D6C8ED0C9631UL;
            y += z * 0xAF36D01EF7518DBBUL;
            x += y * 0x9A69443F36F710E7UL;
            s += x * 0x881403B9339BD42DUL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 58);
        }

        /**
         * Gets a 6-bit point hash of a 6D point (x, y, z, w, u, and v are all longs) and a state/seed as a long. This
         * point hash has close to the best speed of any algorithms tested, and though its quality is mediocre for
         * traditional uses of hashing (such as hash tables), it's sufficiently random to act as a positional RNG.
         * <br>
         * This uses a technique related to the one used by Martin Roberts for his golden-ratio-based sub-random
         * sequences, where each axis is multiplied by a different constant, and the choice of constants depends on the
         * number of axes but is always related to a generalized form of golden ratios, repeatedly dividing 1.0 by the
         * generalized ratio. See
         * <a href="http://extremelearning.com.au/unreasonable-effectiveness-of-quasirandom-sequences/">Roberts' article</a>
         * for some more information on how he uses this, but we do things differently because we want random-seeming
         * results instead of separated sub-random results.
         * @param x x position; any long
         * @param y y position; any long
         * @param z z position; any long
         * @param w w position; any long
         * @param u u position; any long
         * @param v v position; any long
         * @param s the state; any long
         * @return 6-bit hash of the x,y,z,w,u,v point with the given state
         */
        [MethodImpl(FN_INLINE)]
        public static uint Hash64(long X, long Y, long Z, long W, long U, long V, long S) {
            ulong x = (ulong)X;
            ulong y = (ulong)Y;
            ulong z = (ulong)Z;
            ulong w = (ulong)W;
            ulong u = (ulong)U;
            ulong v = (ulong)V;
            ulong s = (ulong)S;
            v += s * 0xE95E1DD17D35800DUL;
            u += v * 0xD4BC74E13F3C782FUL;
            w += u * 0xC1EDBC5B5C68AC25UL;
            z += w * 0xB0C8AC50F0EDEF5DUL;
            y += z * 0xA127A31C56D1CDB5UL;
            x += y * 0x92E852C80D153DB3UL;
            s += x * 0x85EB75C3024385C3UL;
            return (uint)(((s ^ s >> 27 ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL) >> 58);
        }

    }
}
