using System.Collections.Generic;

namespace SquidLib.SquidMath {
    /**
     * Interface for random number generators that allow stepping backwards to implement.
     */
    public interface IReversibleRNG {
        /**
         * Get up to 32 bits (inclusive) of random output; the int this produces
         * will not require more than {@code bits} bits to represent.
         *
         * @param bits an int between 1 and 32, both inclusive
         * @return a random number that fits in the specified number of bits
         */
        int PreviousBits(int bits);

        /**
         *
         * Using this method, any algorithm that needs to efficiently generate more
         * than 32 bits of random data can interface with this randomness source.
         *
         * Get a random long between Long.MIN_VALUE and Long.MAX_VALUE (both inclusive).
         * @return a random long between Long.MIN_VALUE and Long.MAX_VALUE (both inclusive)
         */
        long PreviousLong();
        ulong PreviousULong();

        /**
         * Get a random integer between Integer.MIN_VALUE to Integer.MAX_VALUE (both inclusive).
         *
         * @return a 32-bit random int.
         */
        int PreviousInt();

        uint PreviousUInt();

        /**
         * Returns a random non-negative integer below the given bound, or 0 if the bound is 0 or
         * negative.
         *
         * @param bound the upper bound (exclusive)
         * @return the found number
         */
        int PreviousInt(int bound);
        uint PreviousUInt(uint bound);

        /**
         * Returns a random long below the given bound, or 0 if the bound is 0 or
         * negative.
         *
         * @param bound the upper bound (exclusive)
         * @return the found number
         */
        long PreviousLong(long bound);

        ulong PreviousULong(ulong bound);

        /**
         * Get a random bit of state, interpreted as true or false with approximately equal likelihood.
         * @return a random boolean.
         */
        bool PreviousBoolean();

        /**
         * Gets a random double between 0.0 inclusive and 1.0 exclusive.
         * This returns a maximum of 0.9999999999999999 because that is the largest double value that is less than 1.0 .
         *
         * @return a double between 0.0 (inclusive) and 0.9999999999999999 (inclusive)
         */
        double PreviousDouble();
        /**
         * This returns a random double between 0.0 (inclusive) and outer (exclusive). The value for outer can be positive
         * or negative. Because of how math on doubles works, there are at most 2 to the 53 values this can return for any
         * given outer bound, and very large values for outer will not necessarily produce all numbers you might expect.
         *
         * @param outer the outer exclusive bound as a double; can be negative or positive
         * @return a double between 0.0 (inclusive) and outer (exclusive)
         */
        double PreviousDouble(double outer);

        /**
         * Gets a random float between 0.0f inclusive and 1.0f exclusive.
         * This returns a maximum of 0.99999994 because that is the largest float value that is less than 1.0f .
         *
         * @return a float between 0f (inclusive) and 0.99999994f (inclusive)
         */
        float PreviousFloat();
        /**
         * This returns a random float between 0.0f (inclusive) and outer (exclusive). The value for outer can be positive
         * or negative. Because of how math on floats works, there are at most 2 to the 24 values this can return for any
         * given outer bound, and very large values for outer will not necessarily produce all numbers you might expect.
         *
         * @param outer the outer exclusive bound as a float; can be negative or positive
         * @return a float between 0f (inclusive) and outer (exclusive)
         */
        float PreviousFloat(float outer);
        /**
         * Exclusive on bound (which may be positive or negative), with an inner bound of 0.
         * If bound is negative this returns a negative long; if bound is positive this returns a positive long. The bound
         * can even be 0, which will cause this to return 0L every time. This uses a biased technique to get numbers from
         * large ranges, but the amount of bias is incredibly small (expected to be under 1/1000 if enough random ranged
         * numbers are requested, which is about the same as an unbiased method that was also considered). It may have
         * noticeable bias if the generator's period is exhausted by only calls to this method. Unlike all unbiased methods,
         * this advances the state by an equivalent to exactly one call to {@link #NextLong()}, where rejection sampling
         * would sometimes advance by one call, but other times by arbitrarily many more.
         * @param bound the outer exclusive bound; can be positive or negative
         * @return a random long between 0 (inclusive) and bound (exclusive)
         */
        long PreviousSignedLong(long bound);

        /**
         * Returns a random non-negative integer between 0 (inclusive) and the given bound (exclusive),
         * or 0 if the bound is 0. The bound can be negative, which will produce 0 or a negative result.
         * <br>
         * Credit goes to Daniel Lemire, http://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction/
         *
         * @param bound the outer bound (exclusive), can be negative or positive
         * @return the found number
         */
        int PreviousSignedInt(int bound);
        /**
         * Returns a value between min (inclusive) and max (exclusive) as ints.
         * <br>
         * The inclusive and exclusive behavior is to match the behavior of the similar
         * method that deals with floating point values.
         * <br>
         * If {@code min} and {@code max} happen to be the same, {@code min} is returned
         * (breaking the exclusive behavior, but it's convenient to do so).
         *
         * @param min
         *            the minimum bound on the return value (inclusive)
         * @param max
         *            the maximum bound on the return value (exclusive)
         * @return the found value
         */
        int PreviousInt(int min, int max);

        /**
         * Returns a value between min (inclusive) and max (exclusive) as longs.
         * <br>
         * The inclusive and exclusive behavior is to match the behavior of the similar
         * method that deals with floating point values.
         * <br>
         * If {@code min} and {@code max} happen to be the same, {@code min} is returned
         * (breaking the exclusive behavior, but it's convenient to do so).
         *
         * @param min
         *            the minimum bound on the return value (inclusive)
         * @param max
         *            the maximum bound on the return value (exclusive)
         * @return the found value
         */
        long PreviousLong(long min, long max);

        /**
         * Returns a value from a uniform distribution from min (inclusive) to max
         * (exclusive).
         *
         * @param min the minimum bound on the return value (inclusive)
         * @param max the maximum bound on the return value (exclusive)
         * @return the found value
         */
        double PreviousDouble(double min, double max);

        /**
         * Returns a random element from the provided IEnumerable, which should have predictable iteration order if you want
         * predictable behavior for identical RNG seeds, though it will get a random element just fine for any Collection
         * (just not predictably in all cases). If coll is empty, returns null.
         * <br>
         * @param <T>  the type of the returned object
         * @param coll the Collection to get an element from; remember, Map does not implement Collection
         * @return the randomly selected element
         */
        T PreviousRandomElement<T>(IEnumerable<T> coll);

        /**
         * Shuffle an array using the Fisher-Yates algorithm and returns a shuffled copy, freshly-allocated, without
         * modifying elements.
         * <br>
         * <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Wikipedia has more on this algorithm</a>.
         *
         * @param elements an array of T; will not be modified
         * @param <T>      can be any non-primitive type.
         * @return a shuffled copy of elements
         */
        T[] ReverseShuffle<T>(T[] elements);

        /**
         * Shuffles an array in-place using the Fisher-Yates algorithm.
         * If you don't want the array modified, use {@link #shuffle(Object[], Object[])}.
         * <br>
         * <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Wikipedia has more on this algorithm</a>.
         *
         * @param elements an array of T; <b>will</b> be modified
         * @param <T>      can be any non-primitive type.
         * @return elements after shuffling it in-place
         */
        T[] ReverseShuffleInPlace<T>(T[] elements);

        /**
         * Shuffle an array using the Fisher-Yates algorithm. DO NOT give the same array for both elements and
         * dest, since the prior contents of dest are rearranged before elements is used, and if they refer to the same
         * array, then you can end up with bizarre bugs where one previously-unique item shows up dozens of times. If
         * possible, create a new array with the same length as elements and pass it in as dest; the returned value can be
         * assigned to whatever you want and will have the same items as the newly-formed array.
         * <br>
         * <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Wikipedia has more on this algorithm</a>.
         *
         * @param elements an array of T; will not be modified
         * @param <T>      can be any non-primitive type.
         * @param dest     Where to put the shuffle. If it does not have the same length as {@code elements}, this will use the
         *                 randomPortion method of this class to fill the smaller dest. MUST NOT be the same array as elements!
         * @return {@code dest} after modifications
         */
        T[] ReverseShuffle<T>(T[] elements, T[] dest);
        /**
         * Shuffles a {@link Collection} of T using the Fisher-Yates algorithm and returns an ArrayList of T.
         * <br>
         * <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Wikipedia has more on this algorithm</a>.
         * @param elements a Collection of T; will not be modified
         * @param <T>      can be any non-primitive type.
         * @return a shuffled ArrayList containing the whole of elements in pseudo-random order.
         */
        List<T> ReverseShuffle<T>(IEnumerable<T> elements);

        /**
         * Shuffles a {@link Collection} of T using the Fisher-Yates algorithm and puts it in a buffer.
         * The result is allocated if {@code buf} is null or if {@code buf} isn't empty,
         * otherwise {@code elements} is poured into {@code buf}.
         * <br>
         * <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Wikipedia has more on this algorithm</a>.
         * @param elements a Collection of T; will not be modified
         * @param buf a buffer as an ArrayList that will be filled with the shuffled contents of elements;
         *            if null or non-empty, a new ArrayList will be allocated and returned
         * @param <T>      can be any non-primitive type.
         * @return a shuffled ArrayList containing the whole of elements in pseudo-random order, which may be {@code buf}
         */
        List<T> ReverseShuffle<T>(IEnumerable<T> elements, List<T> buf);
        /**
         * Shuffles a Collection of T items in-place using the Fisher-Yates algorithm.
         * This only shuffles List data structures.
         * If you don't want the array modified, use {@link #shuffle(Collection)}, which returns a List as well.
         * <br>
         * <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Wikipedia has more on this algorithm</a>.
         *
         * @param elements a Collection of T; <b>will</b> be modified
         * @param <T>      can be any non-primitive type.
         * @return elements after shuffling it in-place
         */
        List<T> ReverseShuffleInPlace<T>(List<T> elements);
        /**
         * Gets a random portion of data (an array), assigns that portion to output (an array) so that it fills as much as
         * it can, and then returns output. Will only use a given position in the given data at most once.
         * 
         * @param data   an array of T; will not be modified.
         * @param output an array of T that will be overwritten; should always be instantiated with the portion length
         * @param <T>    can be any non-primitive type.
         * @return output, after {@code Math.min(output.length, data.length)} unique items have been put into it from data
         */
        T[] ReverseRandomPortion<T>(T[] data, T[] output);
    }

}
