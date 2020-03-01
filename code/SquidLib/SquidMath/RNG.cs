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
    public class RNG : IRNG, IReversibleRNG {
        private const double doubleDivisor = 1.0 / (1 << 53);
        private const float floatDivisor = 1.0f / (1 << 24);
        static private Random localRNG = new Random();

        private (ulong a, ulong b) state;

        public string StateCode {
            get => $"{state.a:X16}{state.b:X16}";
            set {
                if (value is null) {
                    value = ""; //the following ulong.Parse() will throw a sensible exception
                }
                state.a = ulong.Parse(value.Substring(0, 16), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                state.b = ulong.Parse(value.Substring(16, 32), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        /// <summary>
        /// Creates a new RNG using a static System.Random to generate two seed values.
        /// </summary>
        public RNG() :
            this((ulong)(localRNG.NextDouble() * 0x10000000000000UL)
                    ^ (ulong)(localRNG.NextDouble() * 2.0 * 0x8000000000000000UL),
                (ulong)(localRNG.NextDouble() * 0x10000000000000UL)
                    ^ (ulong)(localRNG.NextDouble() * 2.0 * 0x8000000000000000UL)) { }

        public RNG(long seed) => state = ((ulong)seed, Randomize((ulong)seed) | 1UL);

        public RNG(ulong seed) => state = (seed, Randomize(seed) | 1UL);

        public RNG(ulong seedA, ulong seedB) => state = (seedA, seedB | 1UL);

        /// <summary>
        /// I am pretty sure this will share the given sharedState with any other RNG that uses it.
        /// </summary>
        /// <param name="sharedState">Will not be copied; will be used by reference and generation calls will mutate this</param>
        public RNG((ulong a, ulong b) sharedState) {
            state = sharedState;
            state.b |= 1UL;
        }
        public RNG(string seed) : this(Randomize((ulong)(seed is null ? "" : seed).GetHashCode())) { }

        public int NextBits(int bits) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (int)((z ^ z >> 26) >> 64 - bits);
        }

        public ulong NextULong() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return z ^ z >> 26;
        }

        public long NextLong() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (long)(z ^ z >> 26);
        }

        /**
         * Produces a copy of this RandomnessSource that, if next() and/or NextLong() are called on this object and the
         * copy, both will generate the same sequence of random numbers from the point copy() was called. This just need to
         * copy the state so it isn't shared, usually, and produce a new value with the same exact state.
         *
         * @return a copy of this RandomnessSource
         */
        public IRNG Copy() => new RNG(state);

        /**
         * Can return any int, positive or negative, of any size permissible in a 32-bit signed integer.
         *
         * @return any int, all 32 bits are random
         */
        public int NextInt() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (int)(z ^ z >> 26);
        }
        public uint NextUInt() {
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
        public int NextInt(int bound) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (int)((Math.Max(0, bound) * (long)((z ^ z >> 26) & 0xFFFFFFFFUL)) >> 32);
        }
        public uint NextUInt(uint bound) {
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
        public int NextSignedInt(int bound) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return (int)((bound * (long)((z ^ z >> 26) & 0xFFFFFFFFUL)) >> 32);
        }

        /// <summary>
        /// Gets what the previous call to NextSignedInt(int) would have produced, given the same
        /// state, and rolls back the state further so the next call to this will go earlier.
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        public int PreviousSignedInt(int bound) {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return (int)((bound * (long)((z ^ z >> 26) & 0xFFFFFFFFUL)) >> 32);
        }

        public ulong NextULong(ulong bound) {
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
         * as {@link #NextLong()}.
         *
         * @param bound the outer exclusive bound; can be positive or negative
         * @return a random long between 0 (inclusive) and bound (exclusive)
         */
        public long NextLong(long bound) => (long)NextULong((ulong)Math.Max(0L, bound));

        /**
         * Exclusive on bound (which may be positive or negative), with an inner bound of 0.
         * If bound is negative this returns a negative long; if bound is positive this returns a positive long. The bound
         * can even be 0, which will cause this to return 0L every time.
         * <br>
         * Credit goes to https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2 ,
         * who ported this StackOverflow answer by catid https://stackoverflow.com/a/51587262 .
         * It also always gets exactly one random long, so by default it advances the state as much
         * as {@link #NextLong()}.
         *
         * @param bound the outer exclusive bound; can be positive or negative
         * @return a random long between 0 (inclusive) and bound (exclusive)
         */
        public long NextSignedLong(long bound) {
            long sign = bound >> 63;
            return (long)(NextULong((ulong)(sign == -1L ? -bound : bound))) + sign ^ sign; // cheaper "times the sign" when you already have the sign.
        }

        /**
         * Gets a uniform random double in the range [0.0,1.0)
         *
         * @return a random double at least equal to 0.0 and less than 1.0
         */
        public double NextDouble() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26) & 0x1FFFFFFFFFFFFFUL) * doubleDivisor;

        }

        /**
         * Gets a uniform random double in the range [0.0,outer) given a positive parameter outer. If outer
         * is negative, it will be the (exclusive) lower bound and 0.0 will be the (inclusive) upper bound.
         *
         * @param outer the exclusive outer bound, can be negative
         * @return a random double between 0.0 (inclusive) and outer (exclusive)
         */
        public double NextDouble(double outer) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26) & 0x1FFFFFFFFFFFFFUL) * doubleDivisor * outer;
        }

        /**
         * Gets a uniform random float in the range [0.0,1.0)
         *
         * @return a random float at least equal to 0.0 and less than 1.0
         */
        public float NextFloat() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            return ((s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL) >> 40) * floatDivisor;
        }

        public float NextFloat(float outer) {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            return ((s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL) >> 40) * floatDivisor * outer;
        }

        /**
         * Gets a random value, true or false.
         * Calls NextLong() once.
         *
         * @return a random true or false value.
         */
        public bool NextBoolean() {
            ulong s = (state.a += 0xC6BC279692B5C323UL);
            return (s ^ s >> 31) * (state.b += 0x9E3779B97F4A7C16UL) < 0x8000000000000000UL;
        }

        /**
         * Given a byte array as a parameter, this will fill the array with random bytes (modifying it
         * in-place). Calls NextLong() {@code Math.ceil(bytes.length / 8.0)} times.
         *
         * @param bytes a byte array that will have its contents overwritten with random bytes.
         */
        public void NextBytes(byte[] bytes) {
            if (bytes is null) {
                return;
            }
            int i = bytes.Length, n;
            while (i != 0) {
                n = Math.Min(i, 8);
                for (ulong bits = NextULong(); n-- != 0; bits >>= 8) bytes[--i] = (byte)bits;
            }
        }

        public void SetState(string seed) {
            if (seed is null) {
                seed = ""; // rely on following Parse to throw relevant exceptions
            }
            state.a = ulong.Parse(seed.Substring(0, 16), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            state.b = ulong.Parse(seed.Substring(16, 32), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public string GetState() => $"{state.a:X16}{state.b:X16}";

        public override string ToString() => $"RNG with state (0x{state.a:X16}UL,0x{state.b:X16}UL)";

        /**
         * Fast static randomizing method that takes its state as a parameter; state is expected to change between calls to
         * this. It is recommended that you use {@code RNG.determine(++state)} or {@code RNG.determine(--state)}
         * to produce a sequence of different numbers, and you may have slightly worse quality with increments or decrements
         * other than 1. All longs are accepted by this method, and all longs can be produced; passing 0 to determine will
         * produce 0, but any other such fixed points are not known.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} will behave well when
         * the inputs are sequential, while {@code randomize()} is meant to have excellent quality regardless of patterns in
         * input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
         * unary hashes; determine() is a slightly stronger/slower version of 
         * <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
         * randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * 
         * @param state any long; subsequent calls should change by an odd number, such as with {@code ++state}
         * @return any long
         */
        public static ulong Determine(ulong state) {
            state ^= state >> 27;
            state *= 0x3C79AC492BA7B653UL;
            state ^= state >> 33;
            state ^= state >> 11;
            state *= 0x1C69B3F74AC4AE35UL;
            return state ^ state >> 27;
        }

        /**
         * High-quality static randomizing method that takes its state as a parameter; state is expected to change between
         * calls to this. It is suggested that you use {@code RNG.randomize(++state)} or
         * {@code RNG.randomize(--state)} to produce a sequence of different numbers, but any increments are allowed
         * (even-number increments won't be able to produce all outputs, but their quality will be fine for the numbers they
         * can produce). All longs are accepted by this method, and all longs can be produced; unlike determine(), passing 0
         * here does not return 0.
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
        public static ulong Randomize(ulong state) =>
            (state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28;

        /**
         * Fast static randomizing method that takes its state as a parameter and limits output to an int between 0
         * (inclusive) and bound (exclusive); state is expected to change between calls to this. It is recommended that you
         * use {@code RNG.determineBounded(++state, bound)} or {@code RNG.determineBounded(--state, bound)} to
         * produce a sequence of different numbers. All longs are accepted
         * by this method, but not all ints between 0 and bound are guaranteed to be produced with equal likelihood (for any
         * odd-number values for bound, this isn't possible for most generators). The bound can be negative.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} will behave well when
         * the inputs are sequential, while {@code randomize()} is meant to have excellent quality regardless of patterns in
         * input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
         * unary hashes; determine() is a slightly stronger/slower version of 
         * <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
         * randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * 
         * @param state any long; subsequent calls should change by an odd number, such as with {@code ++state}
         * @param bound the outer exclusive bound, as an int
         * @return an int between 0 (inclusive) and bound (exclusive)
         */
        public static int DetermineBounded(ulong state, int bound) {
            state ^= state >> 27;
            state *= 0x3C79AC492BA7B653UL;
            state ^= state >> 33;
            state ^= state >> 11;
            state *= 0x1C69B3F74AC4AE35UL;
            return (int)(((ulong)bound * ((state ^ state >> 27) & 0xFFFFFFFFUL)) >> 32);
        }

        /**
         * High-quality static randomizing method that takes its state as a parameter and limits output to an int between 0
         * (inclusive) and bound (exclusive); state is expected to change between calls to this. It is suggested that you
         * use {@code RNG.randomizeBounded(++state)} or {@code RNG.randomize(--state)} to produce a sequence of
         * different numbers, but any increments are allowed (even-number increments won't be able to produce all outputs,
         * but their quality will be fine for the numbers they can produce). All longs are accepted by this method, but not
         * all ints between 0 and bound are guaranteed to be produced with equal likelihood (for any odd-number values for
         * bound, this isn't possible for most generators). The bound can be negative.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} will behave well when
         * the inputs are sequential, while {@code randomize()} is meant to have excellent quality regardless of patterns in
         * input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
         * unary hashes; determine() is a slightly stronger/slower version of 
         * <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
         * randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * 
         * @param state any long; subsequent calls should change by an odd number, such as with {@code ++state}
         * @param bound the outer exclusive bound, as an int
         * @return an int between 0 (inclusive) and bound (exclusive)
         */

        public static int RandomizeBounded(ulong state, int bound) =>
            (int)(((ulong)bound * (((state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28) & 0xFFFFFFFFL)) >> 32);

        /**
         * Returns a random float that is deterministic based on state; if state is the same on two calls to this, this will
         * return the same float. This is expected to be called with a changing variable, e.g.
         * {@code determineFloat(++state)}, where the increment for state should generally be 1. The period is 2 to the 64
         * if you increment or decrement by 1, but there are only 2 to the 30 possible floats between 0 and 1.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} will behave well when
         * the inputs are sequential, while {@code randomize()} is meant to have excellent quality regardless of patterns in
         * input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
         * unary hashes; determine() is a slightly stronger/slower version of 
         * <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
         * randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * 
         * @param state a variable that should be different every time you want a different random result;
         *              using {@code determineFloat(++state)} is recommended to go forwards or
         *              {@code determineFloat(--state)} to generate numbers in reverse order
         * @return a pseudo-random float between 0f (inclusive) and 1f (exclusive), determined by {@code state}
         */
        public static float DetermineFloat(ulong state) {
            state ^= state >> 27;
            state *= 0x3C79AC492BA7B653UL;
            state ^= state >> 33;
            state ^= state >> 11;
            state *= 0x1C69B3F74AC4AE35UL;
            return (state >> 40) * floatDivisor;
        }

        /**
         * Returns a random float that is deterministic based on state; if state is the same on two calls to this, this will
         * return the same float. This is expected to be called with a changing variable, e.g.
         * {@code randomizeFloat(++state)}, where the increment for state can be any value and should usually be odd
         * (even-number increments reduce the period). The period is 2 to the 64 if you increment or decrement by any odd
         * number, but there are only 2 to the 30 possible floats between 0 and 1.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} will behave well when
         * the inputs are sequential, while {@code randomize()} is meant to have excellent quality regardless of patterns in
         * input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
         * unary hashes; determine() is a slightly stronger/slower version of 
         * <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
         * randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * 
         * @param state a variable that should be different every time you want a different random result;
         *              using {@code randomizeFloat(++state)} is recommended to go forwards or
         *              {@code randomizeFloat(--state)} to generate numbers in reverse order
         * @return a pseudo-random float between 0f (inclusive) and 1f (exclusive), determined by {@code state}
         */
        public static float RandomizeFloat(ulong state) =>
            (((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L >> 40) * floatDivisor;

        /**
         * Returns a random double that is deterministic based on state; if state is the same on two calls to this, this
         * will return the same float. This is expected to be called with a changing variable, e.g.
         * {@code determineDouble(++state)}, where the increment for state should generally be 1. The period is 2 to the 64
         * if you increment or decrement by 1, but there are only 2 to the 62 possible doubles between 0 and 1.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} will behave well when
         * the inputs are sequential, while {@code randomize()} is meant to have excellent quality regardless of patterns in
         * input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
         * unary hashes; determine() is a slightly stronger/slower version of 
         * <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
         * randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * 
         * @param state a variable that should be different every time you want a different random result;
         *              using {@code determineDouble(++state)} is recommended to go forwards or
         *              {@code determineDouble(--state)} to generate numbers in reverse order
         * @return a pseudo-random double between 0.0 (inclusive) and 1.0 (exclusive), determined by {@code state}
         */
        public static double DetermineDouble(ulong state) {
            state ^= state >> 27;
            state *= 0x3C79AC492BA7B653UL;
            state ^= state >> 33;
            state ^= state >> 11;
            state *= 0x1C69B3F74AC4AE35UL;
            return ((state ^ state >> 27) & 0x1FFFFFFFFFFFFFL) * doubleDivisor;
        }

        /**
         * Returns a random double that is deterministic based on state; if state is the same on two calls to this, this
         * will return the same float. This is expected to be called with a changing variable, e.g.
         * {@code randomizeDouble(++state)}, where the increment for state can be any number but should usually be odd
         * (even-number increments reduce the period). The period is 2 to the 64 if you increment or decrement by 1, but 
         * there are only 2 to the 62 possible doubles between 0 and 1.
         * <br>
         * You have a choice between determine() and randomize() in this class. {@code determine()} will behave well when
         * the inputs are sequential, while {@code randomize()} is meant to have excellent quality regardless of patterns in
         * input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
         * unary hashes; determine() is a slightly stronger/slower version of 
         * <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
         * randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
         * <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
         * testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
         * input but will be about 30% slower than {@code determine()}. Each method will produce all long outputs if given
         * all possible longs as input.
         * 
         * @param state a variable that should be different every time you want a different random result;
         *              using {@code randomizeDouble(++state)} is recommended to go forwards or
         *              {@code randomizeDouble(--state)} to generate numbers in reverse order
         * @return a pseudo-random double between 0.0 (inclusive) and 1.0 (exclusive), determined by {@code state}
         */
        public static double RandomizeDouble(ulong state) =>
           (((state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28) & 0x1FFFFFFFFFFFFFL) * doubleDivisor;


        public int NextInt(int min, int max) => min + NextSignedInt(max - min);
        public long NextLong(long min, long max) => min + NextSignedLong(max - min);
        public double NextDouble(double min, double max) => min + NextDouble(max - min);

        public T RandomElement<T>(IEnumerable<T> enumerable) {
            switch (enumerable) {
                case T[] array when array.Length > 0:
                    return array[NextSignedInt(array.Length)];
                //TODO: IndexedSet and other shuffle-able sets/maps should implement IOrdered or something like it
                case IList<T> list when list.Count > 0:
                    return list[NextSignedInt(list.Count)];
                case ICollection<T> coll when coll.Count > 0:
                    var e = coll.GetEnumerator();
                    for (int target = NextSignedInt(coll.Count); target > 0; target--) {
                        e.MoveNext();
                    }
                    return e.Current;
                case null:
                default:
                    return default;
            }
        }

        private static void Swap<T>(ref T a, ref T b) {
            T temp = a;
            a = b;
            b = temp;
        }

        private static void Swap<T>(ref List<T> list, int a, int b) {
            T temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        public T[] Shuffle<T>(T[] elements) {
            if (elements is null) {
                return null;
            }
            int size = elements.Length;
            T[] array = new T[size];
            elements.CopyTo(array, 0);
            ShuffleInPlace(array);
            return array;
        }
        public T[] ShuffleInPlace<T>(T[] elements) {
            if (elements is null) {
                return null;
            }
            int size = elements.Length;
            for (int i = size; i > 1; i--) {
                Swap(ref elements[i - 1], ref elements[NextSignedInt(i)]);
            }
            return elements;
        }
        public T[] ReverseShuffleInPlace<T>(T[] elements) {
            if (elements is null) {
                return null;
            }
            int size = elements.Length;
            for (int i = 2; i <= size; i++) {
                Swap(ref elements[i - 1], ref elements[PreviousSignedInt(i)]);
            }
            return elements;
        }
        public T[] Shuffle<T>(T[] elements, T[] dest) {
            if (elements is null || dest is null) {
                return null;
            }
            int size = elements.Length, target = dest.Length;
            if (size != target) return RandomPortion(elements, dest);
            elements.CopyTo(dest, 0);
            ShuffleInPlace(dest);
            return dest;
        }
        public T[] RandomPortion<T>(T[] elements, T[] dest) {
            if (elements is null || dest is null) {
                return null;
            }
            int size = elements.Length, target = dest.Length, runs = (target + size - 1) / size;
            for (int i = 0; i < runs; i++) {
                ShuffleInPlace(elements);
                Array.Copy(elements, 0, dest, i * size, Math.Min(size, target - i * size));
            }
            for (int i = 0; i < runs; i++) {
                ReverseShuffleInPlace(elements);
            }
            return dest;
        }
        public List<T> Shuffle<T>(IEnumerable<T> elements) => ShuffleInPlace(new List<T>(elements));
        public List<T> Shuffle<T>(IEnumerable<T> elements, List<T> dest) {
            if (dest == null)
                dest = new List<T>(elements);
            else
                dest.AddRange(elements);
            return ShuffleInPlace(dest);
        }
        public List<T> ShuffleInPlace<T>(List<T> elements) {
            if (elements is null) {
                return null;
            }
            int size = elements.Count;
            for (int i = size; i > 1; i--) {
                Swap(ref elements, i - 1, NextSignedInt(i));
            }
            return elements;
        }
        public List<T> ReverseShuffleInPlace<T>(List<T> elements) {
            if (elements is null) {
                return null;
            }
            int size = elements.Count;
            for (int i = 2; i <= size; i++) {
                Swap(ref elements, i - 1, PreviousSignedInt(i));
            }
            return elements;
        }
        public int[] RandomOrdering(int length) => RandomOrdering(length, new int[length]);
        public int[] RandomOrdering(int length, int[] dest) {
            if (dest is null) {
                return null;
            }
            int n = Math.Min(length, dest.Length);
            for (int i = 0; i < n; i++) {
                dest[i] = i;
            }
            for (int i = n - 1; i > 0; i--) {
                int r = NextSignedInt(i + 1),
                        t = dest[r];
                dest[r] = dest[i];
                dest[i] = t;
            }
            return dest;

        }

        public int PreviousBits(int bits) {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return (int)((z ^ z >> 26) >> 64 - bits);

        }

        public long PreviousLong() {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return (long)(z ^ z >> 26);
        }

        public ulong PreviousULong() {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return z ^ z >> 26;
        }

        public int PreviousInt() {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return (int)(z ^ z >> 26);
        }

        public uint PreviousUInt() {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return (uint)(z ^ z >> 26);
        }

        public int PreviousInt(int bound) {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return (int)((Math.Max(0, bound) * (long)((z ^ z >> 26) & 0xFFFFFFFFUL)) >> 32);
        }

        public uint PreviousUInt(uint bound) {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return (uint)(bound * ((z ^ z >> 26) & 0xFFFFFFFFUL) >> 32);
        }

        public long PreviousLong(long bound) => (long)PreviousULong((ulong)Math.Max(0L, bound));

        public ulong PreviousULong(ulong bound) {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return MathExtras.MultiplyHigh(z ^ z >> 26, bound);
        }

        public bool PreviousBoolean() {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return z < 0x8000000000000000UL;
        }

        public double PreviousDouble() {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return ((z ^ z >> 26) & 0x1FFFFFFFFFFFFFUL) * doubleDivisor;
        }

        public double PreviousDouble(double outer) => PreviousDouble() * outer;

        public float PreviousFloat() {
            ulong s = state.a;
            state.a -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * state.b;
            state.b -= 0x9E3779B97F4A7C16UL;
            return (z >> 40) * floatDivisor;
        }

        public float PreviousFloat(float outer) => PreviousFloat() * outer;

        public long PreviousSignedLong(long bound) {
            long sign = bound >> 63;
            return (long)(PreviousULong((ulong)(sign == -1L ? -bound : bound))) + sign ^ sign; // cheaper "times the sign" when you already have the sign.
        }

        public int PreviousInt(int min, int max) => min + PreviousInt(max - min);

        public long PreviousLong(long min, long max) => min + PreviousLong(max - min);

        public double PreviousDouble(double min, double max) => min + PreviousDouble(max - min);

        public T PreviousRandomElement<T>(IEnumerable<T> enumerable) {
            switch (enumerable) {
                case T[] array when array.Length > 0:
                    return array[PreviousSignedInt(array.Length)];
                //TODO: IndexedSet and other shuffle-able sets/maps should implement IOrdered or something like it
                case IList<T> list when list.Count > 0:
                    return list[PreviousSignedInt(list.Count)];
                case ICollection<T> coll when coll.Count > 0:
                    var e = coll.GetEnumerator();
                    for (int target = PreviousSignedInt(coll.Count); target > 0; target--) {
                        e.MoveNext();
                    }
                    return e.Current;
                case null:
                default:
                    return default;
            }
        }

        public T[] ReverseShuffle<T>(T[] elements) {
            if (elements is null) {
                return null;
            }
            int size = elements.Length;
            T[] array = new T[size];
            elements.CopyTo(array, 0);
            ReverseShuffleInPlace(array);
            return array;
        }

        public T[] ReverseShuffle<T>(T[] elements, T[] dest) {
            if (elements is null || dest is null) {
                return null;
            }
            int size = elements.Length, target = dest.Length;
            if (size != target) return ReverseRandomPortion(elements, dest);
            elements.CopyTo(dest, 0);
            ReverseShuffleInPlace(dest);
            return dest;
        }

        public List<T> ReverseShuffle<T>(IEnumerable<T> elements) => ReverseShuffleInPlace(new List<T>(elements));

        public List<T> ReverseShuffle<T>(IEnumerable<T> elements, List<T> dest) {
            if (dest == null)
                dest = new List<T>(elements);
            else
                dest.AddRange(elements);
            return ReverseShuffleInPlace(dest);
        }

        public T[] ReverseRandomPortion<T>(T[] elements, T[] dest) {
            if (elements is null || dest is null) {
                return null;
            }
            int size = elements.Length, target = dest.Length, runs = (target + size - 1) / size;
            for (int i = 0; i < runs; i++) {
                ReverseShuffleInPlace(elements);
                Array.Copy(elements, 0, dest, i * size, Math.Min(size, target - i * size));
            }
            for (int i = 0; i < runs; i++) {
                ShuffleInPlace(elements);
            }
            return dest;
        }

        public T RandomKey<T>(IOrdered<T> ordered) => ordered.Ordering[NextSignedInt(ordered.Ordering.Count)];

        public void ShuffleInPlace<T>(IOrdered<T> ordered) => ShuffleInPlace(ordered.Ordering);
    }

}
