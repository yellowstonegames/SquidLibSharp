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
    /// <remarks>
    /// Some codebases may just want this class and not the rest of SquidLibSharp; if you want to copy this class out,
    /// you can remove the interfaces this implements (IRNG and IReversibleRNG), remove the constructor that takes a
    /// string, and remove any code that references IOrdered or IndexedDictionary (like RandomKey(), and a section of
    /// RandomElement()). This is a pretty good type of random number generator, in part because it can step forwards or
    /// backwards (making some shuffle-related code much better), but also because it can repeat or skip varying sets of
    /// output numbers depending on its stream. If a generator always produced a sequence of numbers that excluded about
    /// a third of the possible range, that would be bad, but since each generator produces a different such sequence, it
    /// is good, because it makes predicting the output much harder.
    /// </remarks>
    [Serializable]
    public class RNG : Random, IRNG, IReversibleRNG {
        private const double doubleDivisor = 1.0 / (1L << 53);
        private const float floatDivisor = 1.0f / (1 << 24);
        private static Random localRNG = new Random();

        public (ulong a, ulong b) State { get => (StateA, StateB); set { StateA = value.a; StateB = value.b; } }
        private ulong stateB;
        public ulong StateA { get; set; }
        public ulong StateB { get => stateB; set => stateB = (value | 1UL); }

        public string StateCode {
            get => $"{StateA:X16}{StateB:X16}";
            set {
                if (value is null) {
                    value = ""; //the following ulong.Parse() will throw a sensible exception
                }
                StateA = ulong.Parse(value.Substring(0, 16), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                StateB = ulong.Parse(value.Substring(16, 32), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
        }

        /// <summary>
        /// Creates a new RNG using a static System.Random to generate two seed values.
        /// </summary>
        public RNG() :
            this(MakeSeed(), MakeSeed()) { }

        public RNG(int seed) : this((ulong)seed, Randomize((ulong)seed)) { }
        
        public RNG(long seed) : this((ulong)seed, Randomize((ulong)seed)) { }

        public RNG(ulong seed) : this(seed, Randomize(seed)) { }

        public RNG(ulong seedA, ulong seedB) {
            StateA = seedA;
            StateB = seedB;
        }

        /// <summary>
        /// Uses SeededHash to get two different 64-bit hashes that this will use as its initial state. This can be useful
        /// if you don't know whether the .NET environment this will run on uses randomized hashing; if it does, just
        /// initializing an RNG with string.GetHashCode() would be non-deterministic. It also helps that we get 128 bits of
        /// hash in total here (we only use 127 bits), so quite a lot of generators should be potentially creatable from
        /// string seeds.
        /// </summary>
        /// <param name="seed">any string; it is allowed to be null or empty</param>
        public RNG(string seed) : this(SeededHash.Alpha.Hash64(seed), SeededHash.Omega.Hash64(seed)) { }

        /// <summary>
        /// Produces a copy of other such that, if Next() or other generator methods are called on this object and the
        /// copy, both will generate the same sequence of random numbers from the point the copy constructor was called.
        /// </summary>
        /// <param name="other">Another RNG that must not be null, and will be copied exactly.</param>
        public RNG(RNG other) : this(other is null ? MakeSeed() : other.StateA, other is null ? MakeSeed() : other.StateB) { }

        private static ulong MakeSeed() =>(ulong)(localRNG.NextDouble() * 0x10000000000000UL) ^ (ulong)(localRNG.NextDouble() * 2.0 * 0x8000000000000000UL);

        public IRNG Copy() => new RNG(StateA, StateB);

        public int NextBits(int bits) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (int)((z ^ z >> 26 ^ z >> 6) >> 64 - bits);
        }

        public ulong NextULong() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return z ^ z >> 26 ^ z >> 6;
        }

        public long NextLong() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (long)(z ^ z >> 26 ^ z >> 6);
        }

        /// <summary>
        /// Can return any int, positive or negative, of any size permissible in a 32-bit signed integer.
        /// </summary>
        /// <returns>any int, all 32 bits are random</returns>
        public int NextInt() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (int)(z ^ z >> 26 ^ z >> 6);
        }
        public uint NextUInt() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (uint)(z ^ z >> 26 ^ z >> 6);
        }

        /// <summary>
        /// Exclusive on the outer bound.  The inner bound is 0.
        /// If the bound is negative, this returns 0 but still advances the state normally.
        /// </summary>
        /// <param name="maxValue">bound the upper bound; should be positive</param>
        /// <returns>a random int between 0 (inclusive) and bound (exclusive)</returns>
        public int NextInt(int maxValue) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (int)((Math.Max(0, maxValue) * (long)((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFFFUL)) >> 32);
        }
        public uint NextUInt(uint bound) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (uint)(bound * ((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFFFUL) >> 32);
        }

        /// <summary>
        /// Exclusive on the outer bound.  The inner bound is 0.
        /// The bound can be negative, which makes this produce either a negative int or 0.
        /// This should perform like NextUInt(uint), that is, a little faster than NextInt(int).
        /// Keep in mind, NextSignedLong(long) does not perform as well as NextULong(ulong).
        /// </summary>
        /// <param name="bound">bound the upper bound; should be positive</param>
        /// <returns>a random int between 0 (inclusive) and bound (exclusive)</returns>
        public int NextSignedInt(int bound) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (int)((bound * (long)((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFFFUL)) >> 32);
        }

        /// <summary>
        /// Gets what the previous call to NextSignedInt(int) would have produced, given the same
        /// state, and rolls back the state further so the next call to this will go earlier.
        /// </summary>
        /// <param name="bound"></param>
        /// <returns></returns>
        public int PreviousSignedInt(int bound) {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return (int)((bound * (long)((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFFFUL)) >> 32);
        }

        public ulong NextULong(ulong bound) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);

            ulong x = z ^ z >> 26 ^ z >> 6;
            ulong x0 = (uint)x;
            ulong x1 = x >> 32;

            ulong y0 = (uint)bound;
            ulong y1 = bound >> 32;

            ulong p11 = x1 * y1;
            ulong p01 = x0 * y1;
            ulong p10 = x1 * y0;
            ulong p00 = x0 * y0;

            // 64-bit product + two 32-bit values
            ulong middle = p10 + (p00 >> 32) + (uint)p01;

            // 64-bit product + two 32-bit values
            return p11 + (middle >> 32) + (p01 >> 32);
        }

        /// <summary>
        /// Exclusive on bound (which may be positive or negative), with an inner bound of 0.
        /// If the bound is negative, this returns 0 but still advances the state normally.
        /// <br/>
        /// Credit goes to https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2 ,
        /// who ported this StackOverflow answer by catid https://stackoverflow.com/a/51587262 .
        /// It also always gets exactly one random long, so by default it advances the state as much
        /// as <see cref="NextULong"/>.
        /// </summary>
        /// <param name="bound">the outer exclusive bound; can be positive or negative</param>
        /// <returns>a random long between 0 (inclusive) and bound (exclusive)</returns>
        public long NextLong(long bound) => (long)NextULong((ulong)Math.Max(0L, bound));

        /// <summary>
        /// Exclusive on bound (which may be positive or negative), with an inner bound of 0.
        /// If bound is negative this returns a negative long; if bound is positive this returns a positive long. The bound
        /// can even be 0, which will cause this to return 0L every time.
        /// <br/>
        /// Credit goes to https://gist.github.com/cocowalla/6070a53445e872f2bb24304712a3e1d2 ,
        /// who ported this StackOverflow answer by catid https://stackoverflow.com/a/51587262 .
        /// It also always gets exactly one random long, so by default it advances the state as much
        /// as <see cref="NextULong"/>.
        /// </summary>
        /// <param name="bound">the outer exclusive bound; can be positive or negative</param>
        /// <returns>a random long between 0 (inclusive) and bound (exclusive)</returns>
        public long NextSignedLong(long bound) {
            long sign = bound >> 63;
            return (long)(NextULong((ulong)(sign == -1L ? -bound : bound))) + sign ^ sign; // cheaper "times the sign" when you already have the sign.
        }

        /// <summary>
        /// Gets a uniform random double in the range [0.0,1.0)
        /// </summary>
        /// <returns>a random double at least equal to 0.0 and less than 1.0</returns>
        public override double NextDouble() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26 ^ z >> 6) & 0x1FFFFFFFFFFFFFUL) * doubleDivisor;
        }

        /// <summary>
        /// Gets a uniform random double in the range [0.0,outer) given a positive parameter outer. If outer
        /// is negative, it will be the (exclusive) lower bound and 0.0 will be the (inclusive) upper bound.
        /// </summary>
        /// <param name="outer">the exclusive outer bound, can be negative</param>
        /// <returns>a random double between 0.0 (inclusive) and outer (exclusive)</returns>
        public double NextDouble(double outer) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26 ^ z >> 6) & 0x1FFFFFFFFFFFFFUL) * doubleDivisor * outer;
        }

        /// <summary>
        /// Gets a uniform random float in the range [0.0,1.0)
        /// </summary>
        /// <returns>a random float at least equal to 0.0 and less than 1.0</returns>
        public float NextFloat() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFUL) * floatDivisor;
        }

        public float NextFloat(float outer) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFUL) * floatDivisor * outer;
        }

        /// <summary>
        /// Gets a random value, true or false.
        /// Calls NextLong() once.
        /// </summary>
        /// <returns>a random true or false value.</returns>
        public bool NextBoolean() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            return (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL) < 0x8000000000000000UL;
        }

        /// <summary>
        /// Given a byte array as a parameter, this will fill the array with random bytes (modifying it
        /// in-place). Calls NextULong() {@code Math.ceil(bytes.length / 8.0)} times.
        /// </summary>
        /// <param name="bytes">a byte array that will have its contents overwritten with random bytes.</param>
        public override void NextBytes(byte[] bytes) {
            if (bytes is null) {
                return;
            }
            int i = bytes.Length, n;
            while (i != 0) {
                n = Math.Min(i, 8);
                for (ulong bits = NextULong(); n-- != 0; bits >>= 8) bytes[--i] = (byte)bits;
            }
        }
        public override string ToString() => $"RNG with state (0x{StateA:X16}UL,0x{StateB:X16}UL)";


        /// <summary>
        /// Fast static randomizing method that takes its state as a parameter; state is expected to change between calls to
        /// this. It is recommended that you use <code>RNG.determine(++state)</code> or <code>RNG.determine(--state)</code>
        /// to produce a sequence of different numbers, and you may have slightly worse quality with increments or decrements
        /// other than 1. All longs are accepted by this method, and all longs can be produced; passing 0 to determine will
        /// produce 0, but any other such fixed points are not known.
        /// <br/>
        /// You have a choice between determine() and randomize() in this class. <code>determine()</code> will behave well when
        /// the inputs are sequential, while <code>randomize()</code> is meant to have excellent quality regardless of patterns in
        /// input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
        /// unary hashes; determine() is a slightly stronger/slower version of 
        /// <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
        /// randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
        /// <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
        /// testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
        /// input but will be about 30% slower than <code>determine()</code>. Each method will produce all long outputs if given
        /// all possible longs as input.
        /// </summary>
        /// <param name="state">any long; subsequent calls should change by an odd number, such as with <code>++state</code></param>
        /// <returns>any long</returns>
        public static ulong Determine(ulong state) {
            state ^= state >> 27;
            state *= 0x3C79AC492BA7B653UL;
            state ^= state >> 33;
            state ^= state >> 11;
            state *= 0x1C69B3F74AC4AE35UL;
            return state ^ state >> 27;
        }

        /// <summary>
        /// High-quality static randomizing method that takes its state as a parameter; state is expected to change between
        /// calls to this. It is suggested that you use <code>RNG.randomize(++state)</code> or
        /// <code>RNG.randomize(--state)</code> to produce a sequence of different numbers, but any increments are allowed
        /// (even-number increments won't be able to produce all outputs, but their quality will be fine for the numbers they
        /// can produce). All longs are accepted by this method, and all longs can be produced; unlike determine(), passing 0
        /// here does not return 0.
        /// <br>
        /// You have a choice between determine() and randomize() in this class. <code>determine()</code> is the same as
        /// {@link LinnormRNG#determine(long)} and will behave well when the inputs are sequential, while <code>randomize()</code>
        /// is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
        /// <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
        /// testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
        /// input but will be about 30% slower than <code>determine()</code>. Each method will produce all long outputs if given
        /// all possible longs as input.
        /// </summary>
        /// <param name="state">any long; subsequent calls should change by an odd number, such as with <code>++state</code></param>
        /// <returns>any long</returns>
        public static ulong Randomize(ulong state) =>
            (state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28;

        /// <summary>
        /// Fast static randomizing method that takes its state as a parameter and limits output to an int between 0
        /// (inclusive) and bound (exclusive); state is expected to change between calls to this. It is recommended that you
        /// use <code>RNG.determineBounded(++state, bound)</code> or <code>RNG.determineBounded(--state, bound)</code> to
        /// produce a sequence of different numbers. All longs are accepted
        /// by this method, but not all ints between 0 and bound are guaranteed to be produced with equal likelihood (for any
        /// odd-number values for bound, this isn't possible for most generators). The bound can be negative.
        /// <br>
        /// You have a choice between determine() and randomize() in this class. <code>determine()</code> will behave well when
        /// the inputs are sequential, while <code>randomize()</code> is meant to have excellent quality regardless of patterns in
        /// input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
        /// unary hashes; determine() is a slightly stronger/slower version of 
        /// <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
        /// randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
        /// <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
        /// testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
        /// input but will be about 30% slower than <code>determine()</code>. Each method will produce all long outputs if given
        /// all possible longs as input.
        /// </summary>
        /// <param name="state">any long; subsequent calls should change by an odd number, such as with <code>++state</code></param>
        /// <param name="bound">the outer exclusive bound, as an int</param>
        /// <returns>an int between 0 (inclusive) and bound (exclusive)</returns>
        public static int DetermineBounded(ulong state, int bound) {
            state ^= state >> 27;
            state *= 0x3C79AC492BA7B653UL;
            state ^= state >> 33;
            state ^= state >> 11;
            state *= 0x1C69B3F74AC4AE35UL;
            return (int)(((ulong)bound * ((state ^ state >> 27) & 0xFFFFFFFFUL)) >> 32);
        }

        /// <summary>
        /// High-quality static randomizing method that takes its state as a parameter and limits output to an int between 0
        /// (inclusive) and bound (exclusive); state is expected to change between calls to this. It is suggested that you
        /// use <code>RNG.randomizeBounded(++state)</code> or <code>RNG.randomize(--state)</code> to produce a sequence of
        /// different numbers, but any increments are allowed (even-number increments won't be able to produce all outputs,
        /// but their quality will be fine for the numbers they can produce). All longs are accepted by this method, but not
        /// all ints between 0 and bound are guaranteed to be produced with equal likelihood (for any odd-number values for
        /// bound, this isn't possible for most generators). The bound can be negative.
        /// <br>
        /// You have a choice between determine() and randomize() in this class. <code>determine()</code> will behave well when
        /// the inputs are sequential, while <code>randomize()</code> is meant to have excellent quality regardless of patterns in
        /// input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
        /// unary hashes; determine() is a slightly stronger/slower version of 
        /// <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
        /// randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
        /// <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
        /// testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
        /// input but will be about 30% slower than <code>determine()</code>. Each method will produce all long outputs if given
        /// all possible longs as input.
        /// </summary>
        /// <param name="state">any long; subsequent calls should change by an odd number, such as with <code>++state</code></param>
        /// <param name="bound">the outer exclusive bound, as an int</param>
        /// <returns>an int between 0 (inclusive) and bound (exclusive)</returns>
        public static int RandomizeBounded(ulong state, int bound) =>
            (int)(((ulong)bound * (((state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28) & 0xFFFFFFFFL)) >> 32);

        /// <summary>
        /// Returns a random float that is deterministic based on state; if state is the same on two calls to this, this will
        /// return the same float. This is expected to be called with a changing variable, e.g.
        /// <code>determineFloat(++state)</code>, where the increment for state should generally be 1. The period is 2 to the 64
        /// if you increment or decrement by 1, but there are only 2 to the 30 possible floats between 0 and 1.
        /// <br>
        /// You have a choice between determine() and randomize() in this class. <code>determine()</code> will behave well when
        /// the inputs are sequential, while <code>randomize()</code> is meant to have excellent quality regardless of patterns in
        /// input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
        /// unary hashes; determine() is a slightly stronger/slower version of 
        /// <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
        /// randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
        /// <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
        /// testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
        /// input but will be about 30% slower than <code>determine()</code>. Each method will produce all long outputs if given
        /// all possible longs as input.
        /// </summary>
        /// <param name="state">
        /// a variable that should be different every time you want a different random result;
        /// using <code>determineFloat(++state)</code> is recommended to go forwards or
        /// <code>determineFloat(--state)</code> to generate numbers in reverse order
        /// </param>
        /// <returns>a pseudo-random float between 0f (inclusive) and 1f (exclusive), determined by <code>state</code></returns>
        public static float DetermineFloat(ulong state) {
            state ^= state >> 27;
            state *= 0x3C79AC492BA7B653UL;
            state ^= state >> 33;
            state ^= state >> 11;
            state *= 0x1C69B3F74AC4AE35UL;
            return (state >> 40) * floatDivisor;
        }

        /// <summary>
        /// Returns a random float that is deterministic based on state; if state is the same on two calls to this, this will
        /// return the same float. This is expected to be called with a changing variable, e.g.
        /// <code>randomizeFloat(++state)</code>, where the increment for state can be any value and should usually be odd
        /// (even-number increments reduce the period). The period is 2 to the 64 if you increment or decrement by any odd
        /// number, but there are only 2 to the 30 possible floats between 0 and 1.
        /// <br>
        /// You have a choice between determine() and randomize() in this class. <code>determine()</code> will behave well when
        /// the inputs are sequential, while <code>randomize()</code> is meant to have excellent quality regardless of patterns in
        /// input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
        /// unary hashes; determine() is a slightly stronger/slower version of 
        /// <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
        /// randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
        /// <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
        /// testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
        /// input but will be about 30% slower than <code>determine()</code>. Each method will produce all long outputs if given
        /// all possible longs as input.
        /// </summary>
        /// <param name="state">a variable that should be different every time you want a different random result;
        /// using <code>randomizeFloat(++state)</code> is recommended to go forwards or
        /// <code>randomizeFloat(--state)</code> to generate numbers in reverse order
        /// </param>
        /// <returns>a pseudo-random float between 0f (inclusive) and 1f (exclusive), determined by <code>state</code></returns>
        public static float RandomizeFloat(ulong state) =>
            (((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L >> 40) * floatDivisor;

        /// <summary>
        /// Returns a random double that is deterministic based on state; if state is the same on two calls to this, this
        /// will return the same float. This is expected to be called with a changing variable, e.g.
        /// <code>determineDouble(++state)</code>, where the increment for state should generally be 1. The period is 2 to the 64
        /// if you increment or decrement by 1, but there are only 2 to the 62 possible doubles between 0 and 1.
        /// <br>
        /// You have a choice between determine() and randomize() in this class. <code>determine()</code> will behave well when
        /// the inputs are sequential, while <code>randomize()</code> is meant to have excellent quality regardless of patterns in
        /// input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
        /// unary hashes; determine() is a slightly stronger/slower version of 
        /// <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
        /// randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
        /// <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
        /// testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
        /// input but will be about 30% slower than <code>determine()</code>. Each method will produce all long outputs if given
        /// all possible longs as input.
        /// </summary>
        /// <param name="state">
        /// a variable that should be different every time you want a different random result;
        /// using <code>determineDouble(++state)</code> is recommended to go forwards or
        /// <code>determineDouble(--state)</code> to generate numbers in reverse order
        /// </param>
        /// <returns>a pseudo-random double between 0.0 (inclusive) and 1.0 (exclusive), determined by <code>state</code></returns>
        public static double DetermineDouble(ulong state) {
            state ^= state >> 27;
            state *= 0x3C79AC492BA7B653UL;
            state ^= state >> 33;
            state ^= state >> 11;
            state *= 0x1C69B3F74AC4AE35UL;
            return ((state ^ state >> 27) & 0x1FFFFFFFFFFFFFL) * doubleDivisor;
        }

        /// <summary>
        /// Returns a random double that is deterministic based on state; if state is the same on two calls to this, this
        /// will return the same float. This is expected to be called with a changing variable, e.g.
        /// <code>randomizeDouble(++state)</code>, where the increment for state can be any number but should usually be odd
        /// (even-number increments reduce the period). The period is 2 to the 64 if you increment or decrement by 1, but 
        /// there are only 2 to the 62 possible doubles between 0 and 1.
        /// <br>
        /// You have a choice between determine() and randomize() in this class. <code>determine()</code> will behave well when
        /// the inputs are sequential, while <code>randomize()</code> is meant to have excellent quality regardless of patterns in
        /// input, though randomize() will be about 30% slower than determine(). Both algorithms use Pelle Evensen's work on
        /// unary hashes; determine() is a slightly stronger/slower version of 
        /// <a href="http://mostlymangling.blogspot.com/2019/12/stronger-better-morer-moremur-better.html">Moremur64</a, while
        /// randomize is a completely different algorithm based on Pelle Evensen's rrxmrrxmsx_0 and evaluated with
        /// <a href="http://mostlymangling.blogspot.com/2019/01/better-stronger-mixer-and-test-procedure.html">the same
        /// testing requirements Evensen used for rrxmrrxmsx_0</a>; it will have excellent quality regardless of patterns in
        /// input but will be about 30% slower than <code>determine()</code>. Each method will produce all long outputs if given
        /// all possible longs as input.
        /// </summary>
        /// <param name="state">
        /// a variable that should be different every time you want a different random result;
        /// using <code>randomizeDouble(++state)</code> is recommended to go forwards or
        /// <code>randomizeDouble(--state)</code> to generate numbers in reverse order
        /// </param>
        /// <returns>a pseudo-random double between 0.0 (inclusive) and 1.0 (exclusive), determined by <code>state</code></returns>
        public static double RandomizeDouble(ulong state) =>
           (((state = ((state = (state ^ (state << 41 | state >> 23) ^ (state << 17 | state >> 47) ^ 0xD1B54A32D192ED03L) * 0xAEF17502108EF2D9L) ^ state >> 43 ^ state >> 31 ^ state >> 23) * 0xDB4F0B9175AE2165L) ^ state >> 28) & 0x1FFFFFFFFFFFFFL) * doubleDivisor;

        public int NextInt(int min, int max) => min + NextSignedInt(max - min);
        public long NextLong(long min, long max) => min + NextSignedLong(max - min);
        public double NextDouble(double min, double max) => min + NextDouble(max - min);

        /// <summary>
        /// Retrieves a random key from the given non-empty IndexedDictionary.
        /// </summary>
        /// <typeparam name="TKey">Key type of the IndexedDictionary</typeparam>
        /// <typeparam name="TValue">Value type of the IndexedDictionary</typeparam>
        /// <param name="dictionary">a non-empty, non-null IndexedDictionary</param>
        /// <returns>a randomly-selected key from dict</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="dictionary"/> is null</exception>
        /// <exception cref="InvalidOperationException"> thrown when <paramref name="dictionary"/> is empty</exception>
        public TKey RandomKey<TKey, TValue>(IndexedDictionary<TKey, TValue> dictionary) {
            if (dictionary is null) {
                throw new ArgumentNullException($"{nameof(dictionary)}");
            }
            int size = dictionary.Count;
            if (size <= 0) {
                throw new InvalidOperationException($"IndexedDictionary '{dictionary}' is empty.");
            }
            return dictionary[Key.At, NextSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random key from the given IndexedDictionary. If the IndexedDictionary is empty
        /// then the default value of <typeparamref name="TKey"/> is returned.
        /// </summary>
        /// <typeparam name="TKey">Key type of the IndexedDictionary</typeparam>
        /// <typeparam name="TValue">Value type of the IndexedDictionary</typeparam>
        /// <param name="dictionary">a non-empty, non-null IndexedDictionary</param>
        /// <returns>a randomly-selected key from dict</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="dictionary"/> is null</exception>
        public TKey RandomKeyOrDefault<TKey, TValue>(IndexedDictionary<TKey, TValue> dictionary) {
            if (dictionary is null) {
                throw new ArgumentNullException($"{nameof(dictionary)}");
            }
            int size = dictionary.Count;
            if (size <= 0) {
                return default;
            }
            return dictionary[Key.At, NextSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random value from the given non-empty IndexedDictionary.
        /// </summary>
        /// <typeparam name="TKey">Key type of the IndexedDictionary</typeparam>
        /// <typeparam name="TValue">Value type of the IndexedDictionary</typeparam>
        /// <param name="dictionary">a non-empty, non-null IndexedDictionary</param>
        /// <returns>a randomly-selected value from dict</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="dictionary"/> is null</exception>
        /// <exception cref="InvalidOperationException"> thrown when <paramref name="dictionary"/> is empty</exception>
        public TValue RandomValue<TKey, TValue>(IndexedDictionary<TKey, TValue> dictionary) {
            if (dictionary is null) {
                throw new ArgumentNullException($"{nameof(dictionary)}");
            }
            int size = dictionary.Count;
            if (size <= 0) {
                throw new InvalidOperationException($"IndexedDictionary '{dictionary}' is empty.");
            }
            return dictionary[Value.At, NextSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random value from the given IndexedDictionary. If the IndexedDictionary is empty
        /// then the default value for <typeparamref name="TValue"/> will be returned.
        /// </summary>
        /// <typeparam name="TKey">Key type of the IndexedDictionary</typeparam>
        /// <typeparam name="TValue">Value type of the IndexedDictionary</typeparam>
        /// <param name="dictionary">a non-empty, non-null IndexedDictionary</param>
        /// <returns>a randomly-selected value from dict</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="dictionary"/> is null</exception>
        public TValue RandomValueOrDefault<TKey, TValue>(IndexedDictionary<TKey, TValue> dictionary) {
            if (dictionary is null) {
                throw new ArgumentNullException($"{nameof(dictionary)}");
            }
            int size = dictionary.Count;
            if (size <= 0) {
                return default;
            }
            return dictionary[Value.At, NextSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random element from the given non-empty IList of T (or array of T).
        /// This runs in constant time if the element accessor in <see cref="IList{T}"/> runs in constant time,
        /// which is the case for <see cref="List{T}"/> and <see cref="Array"/> but not <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The generic type of IList</typeparam>
        /// <param name="list">An IList that must be non-null and non-empty.</param>
        /// <returns>A random T element from list.</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="list"/> is null</exception>
        /// <exception cref="InvalidOperationException"> thrown when <paramref name="list"/> is empty</exception>
        public T RandomElement<T>(IList<T> list) {
            if (list is null) {
                throw new ArgumentNullException($"{nameof(list)}");
            }
            int size = list.Count;
            if (size <= 0) {
                throw new InvalidOperationException($"IList '{list}' is empty.");
            }
            return list[NextSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random element from the given IList of T (or array of T).
        /// This runs in constant time if the element accessor in <see cref="IList{T}"/> runs in constant time,
        /// which is the case for <see cref="List{T}"/> and <see cref="Array"/> but not <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The generic type of IList</typeparam>
        /// <param name="list">An IList that must be non-null</param>
        /// <returns>A random T element from list or default if the list is empty</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="list"/> is null</exception>
        public T RandomElementOrDefault<T>(IList<T> list) {
            if (list is null) {
                throw new ArgumentNullException($"{nameof(list)}");
            }
            int size = list.Count;
            if (size <= 0) {
                return default;
            }
            return list[NextSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random element from the given non-empty IOrdered of T (or array of T).
        /// This typically should only be run on <see cref="IndexedSet{T}"/>; if you have an <see cref="IndexedDictionary{TKey, TValue}"/>,
        /// then use <see cref="RandomKey{TKey, TValue}(IndexedDictionary{TKey, TValue})"/> and/or <see cref="RandomValue{TKey, TValue}(IndexedDictionary{TKey, TValue})"/>.
        /// This should generally run in constant time.
        /// </summary>
        /// <typeparam name="T">The generic type of IOrdered</typeparam>
        /// <param name="ordered">An IOrdered that must be non-null and non-empty</param>
        /// <returns>A random T element from ordered.</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="ordered"/> is null</exception>
        /// <exception cref="InvalidOperationException"> thrown when <paramref name="ordered"/> is empty</exception>
        public T RandomElement<T>(IOrdered<T> ordered) {
            if (ordered is null) {
                throw new ArgumentNullException($"{nameof(ordered)}");
            }
            int size = ordered.Ordering.Count;
            if (size <= 0) {
                throw new InvalidOperationException($"IList '{ordered}' is empty.");
            }
            return ordered.Ordering[NextSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random element from the given IOrdered of T (or array of T).
        /// This typically should only be run on <see cref="IndexedSet{T}"/>; if you have an <see cref="IndexedDictionary{TKey, TValue}"/>,
        /// then use <see cref="RandomKey{TKey, TValue}(IndexedDictionary{TKey, TValue})"/> and/or <see cref="RandomValue{TKey, TValue}(IndexedDictionary{TKey, TValue})"/>.
        /// This should generally run in constant time.
        /// </summary>
        /// <typeparam name="T">The generic type of IOrdered</typeparam>
        /// <param name="ordered">An IOrdered that must be non-null</param>
        /// <returns>A random T element from ordered.</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="ordered"/> is null</exception>
        public T RandomElementOrDefault<T>(IOrdered<T> ordered) {
            if (ordered is null) {
                throw new ArgumentNullException($"{nameof(ordered)}");
            }
            int size = ordered.Ordering.Count;
            if (size <= 0) {
                return default;
            }
            return ordered.Ordering[NextSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random element from the given non-empty ICollection of T (an IList or array uses a different overload).
        /// This runs at best in linear time, since it has to iterate through a random amount of coll before it has a result.
        /// </summary>
        /// <typeparam name="T">The generic type of ICollection</typeparam>
        /// <param name="collection">An ICollection that must be non-null and non-empty.</param>
        /// <returns>A random T element from coll.</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="collection"/> is null</exception>
        /// <exception cref="InvalidOperationException"> thrown when <paramref name="collection"/> is empty</exception>
        public T RandomElement<T>(ICollection<T> collection) {
            if (collection is null) {
                throw new ArgumentNullException($"{nameof(collection)}");
            }
            int size = collection.Count;
            if (size <= 0) {
                throw new InvalidOperationException($"IList '{collection}' is empty.");
            }
            var e = collection.GetEnumerator();
            for (int target = NextSignedInt(collection.Count); target > 0; target--) {
                e.MoveNext();
            }
            return e.Current;
        }

        /// <summary>
        /// Retrieves a random element from the given ICollection of T (an IList or array uses a different overload).
        /// This runs at best in linear time, since it has to iterate through a random amount of coll before it has a result.
        /// </summary>
        /// <typeparam name="T">The generic type of ICollection</typeparam>
        /// <param name="collection">An ICollection that must be non-null and non-empty.</param>
        /// <returns>A random T element from coll.</returns>
        /// <exception cref="ArgumentNullException"> thrown when <paramref name="collection"/> is null</exception>
        public T RandomElementOrDefault<T>(ICollection<T> collection) {
            if (collection is null) {
                throw new ArgumentNullException($"{nameof(collection)}");
            }
            int size = collection.Count;
            if (size <= 0) {
                return default;
            }
            var e = collection.GetEnumerator();
            for (int target = NextSignedInt(collection.Count); target > 0; target--) {
                e.MoveNext();
            }
            return e.Current;
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
            if (elements is null) {
                return null;
            }
            int size = elements.Length;
            if (dest is null)
                dest = new T[size];
            else {
                int target = dest.Length;
                if (size != target) return RandomPortion(elements, dest);
            }
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
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return (int)((z ^ z >> 26 ^ z >> 6) >> 64 - bits);

        }

        public long PreviousLong() {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return (long)(z ^ z >> 26 ^ z >> 6);
        }

        public ulong PreviousULong() {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return z ^ z >> 26 ^ z >> 6;
        }

        public int PreviousInt() {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return (int)(z ^ z >> 26 ^ z >> 6);
        }

        public uint PreviousUInt() {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return (uint)(z ^ z >> 26 ^ z >> 6);
        }

        public int PreviousInt(int bound) {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return (int)((Math.Max(0, bound) * (long)((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFFFUL)) >> 32);
        }

        public uint PreviousUInt(uint bound) {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return (uint)(bound * ((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFFFUL) >> 32);
        }

        public long PreviousLong(long bound) => (long)PreviousULong((ulong)Math.Max(0L, bound));

        public ulong PreviousULong(ulong bound) {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            ulong x = z ^ z >> 26 ^ z >> 6;
            ulong x0 = (uint)x;
            ulong x1 = x >> 32;

            ulong y0 = (uint)bound;
            ulong y1 = bound >> 32;

            ulong p11 = x1 * y1;
            ulong p01 = x0 * y1;
            ulong p10 = x1 * y0;
            ulong p00 = x0 * y0;

            // 64-bit product + two 32-bit values
            ulong middle = p10 + (p00 >> 32) + (uint)p01;

            // 64-bit product + two 32-bit values
            return p11 + (middle >> 32) + (p01 >> 32);
        }

        public bool PreviousBoolean() {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return z < 0x8000000000000000UL;
        }

        public double PreviousDouble() {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return ((z ^ z >> 26 ^ z >> 6) & 0x1FFFFFFFFFFFFFUL) * doubleDivisor;
        }

        public double PreviousDouble(double outer) => PreviousDouble() * outer;

        public float PreviousFloat() {
            ulong s = StateA;
            StateA -= 0xC6BC279692B5C323UL;
            ulong z = (s ^ s >> 31) * StateB;
            StateB -= 0x9E3779B97F4A7C16UL;
            return ((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFUL) * floatDivisor;
        }

        public float PreviousFloat(float outer) => PreviousFloat() * outer;

        public long PreviousSignedLong(long bound) {
            long sign = bound >> 63;
            return (long)(PreviousULong((ulong)(sign == -1L ? -bound : bound))) + sign ^ sign; // cheaper "times the sign" when you already have the sign.
        }

        public int PreviousInt(int min, int max) => min + PreviousInt(max - min);

        public long PreviousLong(long min, long max) => min + PreviousLong(max - min);

        public double PreviousDouble(double min, double max) => min + PreviousDouble(max - min);
        /// <summary>
        /// Retrieves a random key from the given non-empty IndexedDictionary, changing the state to one earlier step after getting a key.
        /// </summary>
        /// <typeparam name="TKey">Key type of the IndexedDictionary</typeparam>
        /// <typeparam name="TValue">Value type of the IndexedDictionary</typeparam>
        /// <param name="dict">a non-empty, non-null IndexedDictionary</param>
        /// <returns>a randomly-selected key from dict</returns>
        public TKey PreviousRandomKey<TKey, TValue>(IndexedDictionary<TKey, TValue> dict) {
            int size;
            if (dict is null || (size = dict.Count) <= 0)
                throw new InvalidOperationException($"IndexedDictionary '{dict}' is not valid or is empty.");
            return dict[Key.At, PreviousSignedInt(size)];
        }

        /// <summary>
        /// Retrieves a random value from the given non-empty IndexedDictionary, changing the state to one earlier step after getting a value.
        /// </summary>
        /// <typeparam name="TKey">Key type of the IndexedDictionary</typeparam>
        /// <typeparam name="TValue">Value type of the IndexedDictionary</typeparam>
        /// <param name="dict">a non-empty, non-null IndexedDictionary</param>
        /// <returns>a randomly-selected value from dict</returns>
        public TValue PreviousRandomValue<TKey, TValue>(IndexedDictionary<TKey, TValue> dict) {
            int size;
            if (dict is null || (size = dict.Count) <= 0)
                throw new InvalidOperationException($"IndexedDictionary '{dict}' is not valid or is empty.");
            return dict[Value.At, PreviousSignedInt(size)];
        }

        /// <summary>
        /// Retrieves the previously-chosen random element from the given non-empty IList of T (or array of T).
        /// This runs in constant time if the element accessor in <see cref="IList{T}"/> runs in constant time,
        /// which is the case for <see cref="List{T}"/> and <see cref="Array"/> but not <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The generic type of IList</typeparam>
        /// <param name="list">An IList that must be non-null and non-empty; otherwise this returns default(T).</param>
        /// <returns>The previously-chosen random T element from list.</returns>
        public T PreviousRandomElement<T>(IList<T> list) {
            if (list != null && list.Count > 0)
                return list[PreviousSignedInt(list.Count)];
            throw new InvalidOperationException($"IList '{list}' is not valid or is empty.");
        }

        /// <summary>
        /// Retrieves the previously-chosen random element from the given non-empty IOrdered of T (or array of T).
        /// This typically should only be run on <see cref="IndexedSet{T}"/>; if you have an <see cref="IndexedDictionary{TKey, TValue}"/>,
        /// then use <see cref="RandomKey{TKey, TValue}(IndexedDictionary{TKey, TValue})"/> and/or <see cref="RandomValue{TKey, TValue}(IndexedDictionary{TKey, TValue})"/>.
        /// This should generally run in constant time.
        /// </summary>
        /// <typeparam name="T">The generic type of IOrdered</typeparam>
        /// <param name="ordered">An IOrdered that must be non-null and non-empty; otherwise this returns default(T).</param>
        /// <returns>The previously-chosen random T element from ordered.</returns>
        public T PreviousRandomElement<T>(IOrdered<T> ordered) {
            if (ordered != null && ordered.Ordering != null && ordered.Ordering.Count > 0)
                return ordered.Ordering[PreviousSignedInt(ordered.Ordering.Count)];
            throw new InvalidOperationException($"IOrdered '{ordered}' is not valid or is empty.");
        }

        /// <summary>
        /// Retrieves the previously-chosen random element from the given non-empty ICollection of T (an IList or array uses a different overload).
        /// This runs at best in linear time, since it has to iterate through a random amount of coll before it has a result.
        /// </summary>
        /// <typeparam name="T">The generic type of ICollection</typeparam>
        /// <param name="coll">An ICollection that must be non-null and non-empty; otherwise this returns default(T).</param>
        /// <returns>The previously-chosen random T element from coll.</returns>
        public T PreviousRandomElement<T>(ICollection<T> coll) {
            if (coll != null && coll.Count > 0) {
                var e = coll.GetEnumerator();
                for (int target = PreviousSignedInt(coll.Count); target > 0; target--) {
                    e.MoveNext();
                }
                return e.Current;
            }
            throw new InvalidOperationException($"ICollection '{coll}' is not valid or is empty.");
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
            if (elements is null) {
                return null;
            }
            int size = elements.Length;
            if (dest is null)
                dest = new T[size];
            else {
                int target = dest.Length;
                if (size != target) return ReverseRandomPortion(elements, dest);
            }
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

        public void ShuffleInPlace<T>(IOrdered<T> ordered) => ShuffleInPlace(ordered?.Ordering);
        public void ReverseShuffleInPlace<T>(IOrdered<T> ordered) => ReverseShuffleInPlace(ordered?.Ordering);

        /// <summary>
        /// Gets a ulong that identifies which stream of numbers this generator is producing; this stream identifier is always
        /// an odd ulong and won't change by generating numbers. It is determined at construction and will usually (not
        /// always) change if setStateA(ulong) or setStateB(ulong) are called. Each stream is a
        /// probably-unique sequence of 2 to the 64 longs, where approximately 1 / 3 of all possible longs will not ever occur
        /// (while others occur twice or more), but this set of results is different for every stream. There are 2 to the 64
        /// possible streams, one for every odd long.
        /// </summary>
        /// <returns>An odd long that identifies which stream this TangleRNG is generating from.</returns>
        /// <remarks>The implementation here is neat; 0x1743CE5C6E1B848BUL is the multiplicative inverse of state.a's increment mod 2 to the 64,
        /// so subtracting state.a times that gives us how many steps have been taken since state.a was 0. The relationship between state.a
        /// and state.b is the stream, so stepping state.b back the above number of steps gives us its offset from state.a at all positions.
        /// I don't have a particular reason why you would want to use CurrentStream(), but one could easily come up; maybe it's important that
        /// two generators use independent streams?</remarks>
        public ulong CurrentStream() => StateB - (StateA * 0x1743CE5C6E1B848BUL) * 0x9E3779B97F4A7C16UL;
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            return StateA == ((RNG)obj).StateA && StateB == ((RNG)obj).StateB;
        }
        public override int GetHashCode() => (int)(StateA + stateB >> 32);
        public override int Next() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (int)(z ^ z >> 26 ^ z >> 6) & 0x7FFFFFFF;
        }

        public override int Next(int maxValue) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return (int)((Math.Max(0, maxValue) * (long)((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFFFUL)) >> 32);
        }

        public override int Next(int minValue, int maxValue) {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return minValue + (int)((Math.Max(0, maxValue - minValue) * (long)((z ^ z >> 26 ^ z >> 6) & 0xFFFFFFFFUL)) >> 32);
        }

        protected override double Sample() {
            ulong s = (StateA += 0xC6BC279692B5C323UL);
            ulong z = (s ^ s >> 31) * (StateB += 0x9E3779B97F4A7C16UL);
            return ((z ^ z >> 26 ^ z >> 6) & 0x1FFFFFFFFFFFFFUL) * doubleDivisor;
        }
    }

}
