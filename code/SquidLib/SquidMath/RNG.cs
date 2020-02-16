using System;
using System.Collections.Generic;

namespace SquidLib.SquidMath {
    /// <summary>
    /// Based on TangleRNG's algrithm, which is extremely fast, has a more-than-good-enough period of 2 to the 64,
    /// and has 2 to the 63 possible "streams" of random numbers it can produce. While not all streams have been tested
    /// (that's basically impossible), all of them tested so far have passed PractRand testing to 32TB of generated data.
    /// An individual stream is not equidistributed and will produce some results more than once over the period, while
    /// other numbers will never appear in that stream (roughly 1/3 of possible ulong results won't appear). If you look
    /// across all possible streams, however, the full set of generators is equidistributed, one-dimensionally. It is
    /// encouraged to make lots of these RNGs where their results need to be independent, rather than skipping around in
    /// just one generator.
    /// </summary>
    public class RNG : IStatefulRNG {
        private const double DOUBLE_DIVISOR = 1.0 / (1 << 53);
        private const float FLOAT_DIVISOR = 1.0f / (1 << 24);
        static private Random localRNG = new Random();

        private (ulong a, ulong b) state;
        /// <summary>
        /// Creates a new RNG using a static System.Random to generate two seed values.
        /// </summary>
        public RNG() :
            this((ulong)(localRNG.NextDouble() * 0x10000000000000UL)
                    ^ (ulong)(localRNG.NextDouble() * 2.0 * 0x8000000000000000UL),
                (ulong)(localRNG.NextDouble() * 0x10000000000000UL)
                    ^ (ulong)(localRNG.NextDouble() * 2.0 * 0x8000000000000000UL)) { }

        public RNG(long seed) => state = ((ulong)seed, randomize((ulong)seed) | 1UL);

        public RNG(ulong seed) => state = (seed, randomize(seed) | 1UL);

        public RNG(ulong seedA, ulong seedB) => state = (seedA, seedB | 1UL);

        /// <summary>
        /// I am pretty sure this will share the given sharedState with any other RNG that uses it.
        /// </summary>
        /// <param name="sharedState">Will not be copied; will be used by reference and generation calls will mutate this</param>
        public RNG((ulong a, ulong b) sharedState) {
            state = sharedState;
            state.b |= 1UL;
        }
        public RNG(string seed) : this(randomize((ulong)seed.GetHashCode())) { } // WAS - CrossHash.hash64(seed);

        public int next(int bits) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (int)((z ^ z >> 26) >> 64 - bits);
        }

        public ulong nextULong() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return z ^ z >> 26;
        }

        public long nextLong() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (long)(z ^ z >> 26);
        }

        /**
         * Produces a copy of this RandomnessSource that, if next() and/or nextLong() are called on this object and the
         * copy, both will generate the same sequence of random numbers from the point copy() was called. This just need to
         * copy the state so it isn't shared, usually, and produce a new value with the same exact state.
         *
         * @return a copy of this RandomnessSource
         */
        public IRNG copy() => new RNG(state);

        /**
         * Can return any int, positive or negative, of any size permissible in a 32-bit signed integer.
         *
         * @return any int, all 32 bits are random
         */
        public int nextInt() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (int)(z ^ z >> 26);
        }
        public uint nextUInt() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (uint)(z ^ z >> 26);
        }

        /**
         * Exclusive on the outer bound.  The inner bound is 0.
         * If the bound is negative, this returns 0 but still advances the state normally.
         *
         * @param bound the upper bound; should be positive
         * @return a random int between 0 (inclusive) and bound (exclusive)
         */
        public int nextInt(int bound) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (int)((Math.Max(0, bound) * (long)((z ^ z >> 26) & 0xFFFFFFFFUL)) >> 32);
        }
        public uint nextUInt(uint bound) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (uint)(bound * ((z ^ z >> 26) & 0xFFFFFFFFUL) >> 32);
        }

        /**
         * Exclusive on the outer bound.  The inner bound is 0.
         * The bound can be negative, which makes this produce either a negative int or 0.
         * This should perform like NextUInt(uint), that is, a little faster than NextInt(int).
         * Keep in mind, NextSignedLong(long) does not perform as well as NextULong(ulong).
         * 
         * @param bound the upper bound; should be positive
         * @return a random int between 0 (inclusive) and bound (exclusive)
         */
        public int nextSignedInt(int bound) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (int)((bound * (long)((z ^ z >> 26) & 0xFFFFFFFFUL)) >> 32);
        }

        /**
         * Inclusive inner, exclusive outer.
         *
         * @param inner the inner bound, inclusive, can be positive or negative
         * @param outer the outer bound, exclusive, can be positive or negative, usually greater than inner
         * @return a random int between inner (inclusive) and outer (exclusive)
         */
        public int nextInt(int inner, int outer) {
            return inner + nextInt(outer - inner);
        }

        public ulong nextULong(ulong bound) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return MathExtras.MultiplyHigh(z ^ z >> 26, bound);
        }
        /**
         * Exclusive on bound (which may be positive or negative), with an inner bound of 0.
         * If the bound is negative, this returns 0 but still advances the state normally.
         * <br>
         * Credit goes to https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2 ,
         * who ported this StackOverflow answer by catid https://stackoverflow.com/a/51587262 .
         * It also always gets exactly one random long, so by default it advances the state as much
         * as {@link #nextLong()}.
         *
         * @param bound the outer exclusive bound; can be positive or negative
         * @return a random long between 0 (inclusive) and bound (exclusive)
         */
        public long nextLong(long bound) {
            return (long)nextULong((ulong)Math.Max(0L, bound));
        }
        /**
         * Exclusive on bound (which may be positive or negative), with an inner bound of 0.
         * If bound is negative this returns a negative long; if bound is positive this returns a positive long. The bound
         * can even be 0, which will cause this to return 0L every time.
         * <br>
         * Credit goes to https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2 ,
         * who ported this StackOverflow answer by catid https://stackoverflow.com/a/51587262 .
         * It also always gets exactly one random long, so by default it advances the state as much
         * as {@link #nextLong()}.
         *
         * @param bound the outer exclusive bound; can be positive or negative
         * @return a random long between 0 (inclusive) and bound (exclusive)
         */
        public long nextSignedLong(long bound) {
            long sign = bound >> 63;
            return (long)(nextULong((ulong)(sign == -1L ? -bound : bound)))
                + sign ^ sign; // cheaper "times the sign" when you already have the sign.
        }
        /**
         * Inclusive inner, exclusive outer; lower and upper can be positive or negative and there's no requirement for one
         * to be greater than or less than the other.
         *
         * @param lower the lower bound, inclusive, can be positive or negative
         * @param upper the upper bound, exclusive, can be positive or negative
         * @return a random long that may be equal to lower and will otherwise be between lower and upper
         */
        public long nextLong(long lower, long upper) {
            return lower + nextLong(upper - lower);
        }

        /**
         * Gets a uniform random double in the range [0.0,1.0)
         *
         * @return a random double at least equal to 0.0 and less than 1.0
         */
        public double nextDouble() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26) & 0x1FFFFFFFFFFFFFUL) * DOUBLE_DIVISOR;

        }

        /**
         * Gets a uniform random double in the range [0.0,outer) given a positive parameter outer. If outer
         * is negative, it will be the (exclusive) lower bound and 0.0 will be the (inclusive) upper bound.
         *
         * @param outer the exclusive outer bound, can be negative
         * @return a random double between 0.0 (inclusive) and outer (exclusive)
         */
        public double nextDouble(double outer) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26) & 0x1FFFFFFFFFFFFFUL) * DOUBLE_DIVISOR * outer;
        }

        /**
         * Gets a uniform random float in the range [0.0,1.0)
         *
         * @return a random float at least equal to 0.0 and less than 1.0
         */
        public float nextFloat() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            return ((s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL) >> 40) * FLOAT_DIVISOR;
        }

        public float nextFloat(float outer) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            return ((s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL) >> 40) * FLOAT_DIVISOR * outer;
        }

        /**
         * Gets a random value, true or false.
         * Calls nextLong() once.
         *
         * @return a random true or false value.
         */
        public bool nextBoolean() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            return (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL) < 0x8000000000000000UL;
        }

        /**
         * Given a byte array as a parameter, this will fill the array with random bytes (modifying it
         * in-place). Calls nextLong() {@code Math.ceil(bytes.length / 8.0)} times.
         *
         * @param bytes a byte array that will have its contents overwritten with random bytes.
         */
        public void nextBytes(byte[] bytes) {
            int i = bytes.Length, n;
            while (i != 0) {
                n = Math.Min(i, 8);
                for (ulong bits = nextULong(); n-- != 0; bits >>= 8) bytes[--i] = (byte)bits;
            }
        }

        public void setState(string seed) {
            state.a = ulong.Parse(seed.Substring(0, 16), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            state.b = ulong.Parse(seed.Substring(16, 32), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public string getState() => $"{state.a:X16}{state.b:X16}";

        public string toString() => $"RNG with state (0x{state.a:X16}UL,0x{state.b:X16}UL)";

        /**
         * Fast static randomizing method that takes its state as a parameter; state is expected to change between calls to
         * this. It is recommended that you use {@code RNG.determine(++state)} or {@code RNG.determine(--state)}
         * to produce a sequence of different numbers, and you may have slightly worse quality with increments or decrements
         * other than 1. All longs are accepted by this method, and all longs can be produced; unlike several other classes'
         * determine() methods, passing 0 here does not return 0.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} is the same as
         * {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while {@code randomize()}
         * is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * @param state any long; subsequent calls should change by an odd number, such as with {@code ++state}
         * @return any long
         */
        public static ulong determine(ulong state) =>
            (state = ((state = (((state * 0x632BE59BD9B4E019UL) ^ 0x9E3779B97F4A7C15UL) * 0xC6BC279692B5CC83UL)) ^ state >> 27) * 0xAEF17502108EF2D9UL) ^ state >> 25;

        /**
         * High-quality static randomizing method that takes its state as a parameter; state is expected to change between
         * calls to this. It is suggested that you use {@code RNG.randomize(++state)} or
         * {@code RNG.randomize(--state)} to produce a sequence of different numbers, but any increments are allowed
         * (even-number increments won't be able to produce all outputs, but their quality will be fine for the numbers they
         * can produce). All longs are accepted by this method, and all longs can be produced; unlike several other classes'
         * determine() methods, passing 0 here does not return 0.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} is the same as
         * {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while {@code randomize()}
         * is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * @param state any long; subsequent calls should change by an odd number, such as with {@code ++state}
         * @return any long
         */
        public static ulong randomize(ulong state) =>
            (state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28;

        /**
         * Fast static randomizing method that takes its state as a parameter and limits output to an int between 0
         * (inclusive) and bound (exclusive); state is expected to change between calls to this. It is recommended that you
         * use {@code RNG.determineBounded(++state, bound)} or {@code RNG.determineBounded(--state, bound)} to
         * produce a sequence of different numbers. All longs are accepted
         * by this method, but not all ints between 0 and bound are guaranteed to be produced with equal likelihood (for any
         * odd-number values for bound, this isn't possible for most generators). The bound can be negative.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} is the same as
         * {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while {@code randomize()}
         * is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * @param state any long; subsequent calls should change by an odd number, such as with {@code ++state}
         * @param bound the outer exclusive bound, as an int
         * @return an int between 0 (inclusive) and bound (exclusive)
         */
        public static int determineBounded(ulong state, int bound) =>
            (int)(((ulong)bound * (((state = ((state = (((state * 0x632BE59BD9B4E019UL) ^ 0x9E3779B97F4A7C15L) * 0xC6BC279692B5CC83L)) ^ state >> 27) * 0xAEF17502108EF2D9L) ^ state >> 25) & 0xFFFFFFFFL)) >> 32);
        /**
         * High-quality static randomizing method that takes its state as a parameter and limits output to an int between 0
         * (inclusive) and bound (exclusive); state is expected to change between calls to this. It is suggested that you
         * use {@code RNG.randomizeBounded(++state)} or {@code RNG.randomize(--state)} to produce a sequence of
         * different numbers, but any increments are allowed (even-number increments won't be able to produce all outputs,
         * but their quality will be fine for the numbers they can produce). All longs are accepted by this method, but not
         * all ints between 0 and bound are guaranteed to be produced with equal likelihood (for any odd-number values for
         * bound, this isn't possible for most generators). The bound can be negative.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} is the same as
         * {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while {@code randomize()}
         * is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * @param state any long; subsequent calls should change by an odd number, such as with {@code ++state}
         * @param bound the outer exclusive bound, as an int
         * @return an int between 0 (inclusive) and bound (exclusive)
         */

        public static int randomizeBounded(ulong state, int bound) =>
            (int)(((ulong)bound * (((state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28) & 0xFFFFFFFFL)) >> 32);

        /**
         * Returns a random float that is deterministic based on state; if state is the same on two calls to this, this will
         * return the same float. This is expected to be called with a changing variable, e.g.
         * {@code determineFloat(++state)}, where the increment for state should generally be 1. The period is 2 to the 64
         * if you increment or decrement by 1, but there are only 2 to the 30 possible floats between 0 and 1.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} is the same as
         * {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while {@code randomize()}
         * is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * @param state a variable that should be different every time you want a different random result;
         *              using {@code determineFloat(++state)} is recommended to go forwards or
         *              {@code determineFloat(--state)} to generate numbers in reverse order
         * @return a pseudo-random float between 0f (inclusive) and 1f (exclusive), determined by {@code state}
         */
        public static float determineFloat(ulong state) =>
            ((((state = (((state * 0x632BE59BD9B4E019UL) ^ 0x9E3779B97F4A7C15L) * 0xC6BC279692B5CC83L)) ^ state >> 27) * 0xAEF17502108EF2D9L) >> 40) * 0x1e - 24f;

        /**
         * Returns a random float that is deterministic based on state; if state is the same on two calls to this, this will
         * return the same float. This is expected to be called with a changing variable, e.g.
         * {@code randomizeFloat(++state)}, where the increment for state can be any value and should usually be odd
         * (even-number increments reduce the period). The period is 2 to the 64 if you increment or decrement by any odd
         * number, but there are only 2 to the 30 possible floats between 0 and 1.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} is the same as
         * {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while {@code randomize()}
         * is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * @param state a variable that should be different every time you want a different random result;
         *              using {@code randomizeFloat(++state)} is recommended to go forwards or
         *              {@code randomizeFloat(--state)} to generate numbers in reverse order
         * @return a pseudo-random float between 0f (inclusive) and 1f (exclusive), determined by {@code state}
         */
        public static float randomizeFloat(ulong state) =>
            (((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L >> 40) * 0x1e - 24f;

        /**
         * Returns a random double that is deterministic based on state; if state is the same on two calls to this, this
         * will return the same float. This is expected to be called with a changing variable, e.g.
         * {@code determineDouble(++state)}, where the increment for state should generally be 1. The period is 2 to the 64
         * if you increment or decrement by 1, but there are only 2 to the 62 possible doubles between 0 and 1.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} is the same as
         * {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while {@code randomize()}
         * is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * @param state a variable that should be different every time you want a different random result;
         *              using {@code determineDouble(++state)} is recommended to go forwards or
         *              {@code determineDouble(--state)} to generate numbers in reverse order
         * @return a pseudo-random double between 0.0 (inclusive) and 1.0 (exclusive), determined by {@code state}
         */
        public static double determineDouble(ulong state) =>
            (((state = ((state = (((state * 0x632BE59BD9B4E019UL) ^ 0x9E3779B97F4A7C15L) * 0xC6BC279692B5CC83L)) ^ state >> 27) * 0xAEF17502108EF2D9L) ^ state >> 25) & 0x1FFFFFFFFFFFFFL) * DOUBLE_DIVISOR;

        /**
         * Returns a random double that is deterministic based on state; if state is the same on two calls to this, this
         * will return the same float. This is expected to be called with a changing variable, e.g.
         * {@code randomizeDouble(++state)}, where the increment for state can be any number but should usually be odd
         * (even-number increments reduce the period). The period is 2 to the 64 if you increment or decrement by 1, but 
         * there are only 2 to the 62 possible doubles between 0 and 1.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} is the same as
         * {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while {@code randomize()}
         * is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * @param state a variable that should be different every time you want a different random result;
         *              using {@code randomizeDouble(++state)} is recommended to go forwards or
         *              {@code randomizeDouble(--state)} to generate numbers in reverse order
         * @return a pseudo-random double between 0.0 (inclusive) and 1.0 (exclusive), determined by {@code state}
         */
        public static double randomizeDouble(ulong state) =>
           (((state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28) & 0x1FFFFFFFFFFFFFL) * DOUBLE_DIVISOR;


        public int between(int min, int max) => min + nextSignedInt(max - min);
        public long between(long min, long max) => min + nextSignedLong(max - min);
        public double between(double min, double max) => min + nextDouble(max - min);
        public T getRandomElement<T>(T[] array) => array[nextInt(array.Length)];
        public T getRandomElement<T>(List<T> list) => list[nextInt(list.Count)];
        
        public T getRandomElement<T>(ICollection<T> coll) {
            var e = coll.GetEnumerator();
            for (int target = nextInt(coll.Count); target > 0; target--) {
                e.MoveNext();
            }
            return e.Current;
                
        }
        public T[] shuffle<T>(T[] elements) => throw new NotImplementedException();
        public T[] shuffleInPlace<T>(T[] elements) => throw new NotImplementedException();
        public T[] shuffle<T>(T[] elements, T[] dest) => throw new NotImplementedException();
        public List<T> shuffle<T>(ICollection<T> elements) => throw new NotImplementedException();
        public List<T> shuffle<T>(ICollection<T> elements, List<T> buf) => throw new NotImplementedException();
        public List<T> shuffleInPlace<T>(List<T> elements) => throw new NotImplementedException();
        public int[] randomOrdering(int length) => throw new NotImplementedException();
        public int[] randomOrdering(int length, int[] dest) => throw new NotImplementedException();
        public T[] randomPortion<T>(T[] data, T[] output) => throw new NotImplementedException();
    }

}
