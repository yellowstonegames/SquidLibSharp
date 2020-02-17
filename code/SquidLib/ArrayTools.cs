using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SquidLib {

    /**
     * Static methods for various frequently-used operations on 1D and 2D arrays. Has methods for copying, inserting, and
     * filling 2D arrays of primitive types (char, int, double, and bool). Has a few mehods for creating ranges of ints
     * or chars easily as 1D arrays. Also contains certain methods for working with orderings, which can be naturally used
     * with {@link squidpony.squidmath.OrderedMap}, {@link squidpony.squidmath.OrderedSet}, {@link squidpony.squidmath.K2},
     * and similar ordered collections plus ArrayList using {@link #reorder(ArrayList, int...)} in this class.
     * Created by Tommy Ettinger on 11/17/2016.
     */
    public class ArrayTools {

        static readonly char[] letters = {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a',
            'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'À', 'Á',
            'Â', 'Ã', 'Ä', 'Å', 'Æ', 'Ç', 'È', 'É', 'Ê', 'Ë', 'Ì', 'Í', 'Î', 'Ï', 'Ð', 'Ñ', 'Ò', 'Ó', 'Ô', 'Õ', 'Ö', 'Ø', 'Ù', 'Ú', 'Û', 'Ü', 'Ý',
            'Þ', 'ß', 'à', 'á', 'â', 'ã', 'ä', 'å', 'æ', 'ç', 'è', 'é', 'ê', 'ë', 'ì', 'í', 'î', 'ï', 'ð', 'ñ', 'ò', 'ó', 'ô', 'õ', 'ö', 'ø', 'ù',
            'ú', 'û', 'ü', 'ý', 'þ', 'ÿ', 'Ā', 'ā', 'Ă', 'ă', 'Ą', 'ą', 'Ć', 'ć', 'Ĉ', 'ĉ', 'Ċ', 'ċ', 'Č', 'č', 'Ď', 'ď', 'Đ', 'đ', 'Ē', 'ē', 'Ĕ',
            'ĕ', 'Ė', 'ė', 'Ę', 'ę', 'Ě', 'ě', 'Ĝ', 'ĝ', 'Ğ', 'ğ', 'Ġ', 'ġ', 'Ģ', 'ģ', 'Ĥ', 'ĥ', 'Ħ', 'ħ', 'Ĩ', 'ĩ', 'Ī', 'ī', 'Ĭ', 'ĭ', 'Į', 'į',
            'İ', 'ı', 'Ĵ', 'ĵ', 'Ķ', 'ķ', 'ĸ', 'Ĺ', 'ĺ', 'Ļ', 'ļ', 'Ľ', 'ľ', 'Ŀ', 'ŀ', 'Ł', 'ł', 'Ń', 'ń', 'Ņ', 'ņ', 'Ň', 'ň', 'ŉ', 'Ō', 'ō', 'Ŏ',
            'ŏ', 'Ő', 'ő', 'Œ', 'œ', 'Ŕ', 'ŕ', 'Ŗ', 'ŗ', 'Ř', 'ř', 'Ś', 'ś', 'Ŝ', 'ŝ', 'Ş', 'ş', 'Š', 'š', 'Ţ', 'ţ', 'Ť', 'ť', 'Ŧ', 'ŧ', 'Ũ', 'ũ',
            'Ū', 'ū', 'Ŭ', 'ŭ', 'Ů', 'ů', 'Ű', 'ű', 'Ų', 'ų', 'Ŵ', 'ŵ', 'Ŷ', 'ŷ', 'Ÿ', 'Ź', 'ź', 'Ż', 'ż', 'Ž', 'ž', 'Ǿ', 'ǿ', 'Ș', 'ș', 'Ț', 'ț',
            'Γ', 'Δ', 'Θ', 'Λ', 'Ξ', 'Π', 'Σ', 'Φ', 'Ψ', 'Ω', 'α', 'β', 'γ'};

        /**
         * Stupidly simple convenience method that produces a char range from start to end, including end, as a char array.
         *
         * @param start the inclusive lower bound on the range, such as 'a'
         * @param end   the inclusive upper bound on the range, such as 'z'
         * @return the range of chars as a char array
         */
        public static char[] charSpan(char start, char end) =>
            Enumerable.Range(start, end - start + 1).Select(c => (char)c).ToArray<char>();

        /**
         * Stupidly simple convenience method that produces a char array containing only letters that can be reasonably
         * displayed (with SquidLib's default text display assets, at least). The letters are copied from a single source
         * of 256 chars; if you need more chars or you don't need pure letters, you can use {@link #charSpan(char, char)}.
         * This set does not contain "visual duplicate" letters, such as Latin alphabet capital letter 'A' and Greek
         * alphabet capital letter alpha, 'Α'; it does contain many accented Latin letters and the visually-distinct Greek
         * letters, up to a point.
         * @param charCount the number of letters to return in an array; the maximum this will produce is 256
         * @return the range of letters as a char array
         */
        public static char[] letterSpan(int charCount) {
            char[] ret = new char[Math.Min(charCount, letters.Length)];
            Array.Copy(letters, ret, charCount);
            return ret;
        }

        /**
         * Gets the nth letter from the set that SquidLib is likely to support; from index 0 (returning 'A') to 255
         * (returning the Greek lower-case letter gamma, 'γ') and wrapping around if given negative numbers or numbers
         * larger than 255. This set does not contain "visual duplicate" letters, such as Latin alphabet capital letter 'A'
         * and Greek alphabet capital letter alpha, 'Α'; it does contain many accented Latin letters and the
         * visually-distinct Greek letters, up to a point.
         * @param index typically from 0 to 255, but all ints are allowed and will produce letters
         * @return the letter at the given index in a 256-element portion of the letters SquidLib usually supports
         */
        public static char letterAt(int index) => letters[index & 255];

        /**
         * Gets a copy of the 2D char array, source, that has the same data but shares no references with source.
         *
         * @param source a 2D array
         * @return a copy of source, or null if source is null
         */
        public static T[][] copy<T>(T[][] source) {
            if (source == null) {
                return null;
            }
            T[][] ret = new T[source.Length][];
            for (int i = 0; i < ret.Length; i++) {
                ret[i] = new T[source[i].Length];
                Array.Copy(source[i], ret[i], source[i].Length);
            }
            return ret;
        }

        /**
         * Inserts as much of source into target at the given x,y position as target can hold or source can supply.
         * Modifies target in-place and also returns target for chaining.
         * Used primarily to place a smaller array into a different position in a larger array, often freshly allocated.
         *
         * @param source a 2D char array that will be copied and inserted into target
         * @param target a 2D char array that will be modified by receiving as much of source as it can hold
         * @param x      the x position in target to receive the items from the first cell in source
         * @param y      the y position in target to receive the items from the first cell in source
         * @return target, modified, with source inserted into it at the given position
         */
        public static T[][] insert<T>(T[][] source, T[][] target, int x, int y) {
            if (source == null || target == null || source[0] == null)
                return target;
            if (source.Length < 1 || source[0].Length < 1)
                return copy(target);
            for (int i = 0; i < source.Length && x + i < target.Length; i++) {
                Array.Copy(source[i], 0, target[x + i], y, Math.Min(source[i].Length, target[x + i].Length - y));
            }
            return target;
        }

        public static T[] create<T>(T content, int size) {
            T[] ret = new T[size];
            for (int i = 0; i < size; i++) {
                ret[i] = content;
            }
            return ret;
        }

        /**
         * Creates a 2D array of the given width and height, filled with entirely with the value contents.
         * You may want to use {@link #fill(char[][], char)} to modify an existing 2D array instead.
         * @param contents the value to fill the array with
         * @param width    the desired width
         * @param height   the desired height
         * @return a freshly allocated 2D array of the requested dimensions, filled entirely with contents
         */
        public static T[][] create<T>(T content, int width, int height) {
            T[][] ret = Create<T>(width, height);
            for (int x = 0; x < width; x++) {
                ret[x] = create(content, height);
            }
            return ret;
        }

        public static void fill<T>(T[][] array2d, T value) {
            if (array2d == null || array2d[0] == null) {
                return;
            }
            fill<T>(array2d, value, 0, 0, array2d.Length, array2d[0].Length);
        }

        /**
         * Fills a sub-section of {@code array2d} with {@code value}, with the section defined by start/end x/y.
         * @param array2d a 2D array that will be modified in-place
         * @param value the value to fill all of array2D with
         * @param startX the first x position to fill (inclusive)
         * @param startY the first y position to fill (inclusive)
         * @param endX the last x position to fill (inclusive)
         * @param endY the last y position to fill (inclusive)
         */
        public static void fill<T>(T[][] array2d, T value, int startX, int startY, int endX, int endY) {
            if (array2d == null) {
                return;
            }
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            for (int x = startX; x <= endX && x < width; x++) {
                for (int y = startY; y <= endY && y < height; y++) {
                    array2d[x][y] = value;
                }
            }
        }

        /**
         * Randomly fills all of {@code array2d} with random values generated from {@code seed}; can fill an element with
         * any long, positive or negative.
         * Fairly efficient; uses a fast random number generation algorithm that can avoid some unnecessary work in this
         * context, and improves quality by seeding each column differently. Generates {@code (height + 1) * width} random
         * values to fill the {@code height * width} elements in array2d.
         * @param array2d a 2D array that will be modified in-place
         * @param seed the seed for the random values, as a long
         */
        public static void randomFill(long[][] array2d, ulong seed) {
            if (array2d == null) {
                return;
            }
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            ulong r0 = seed, z;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    z = r0 ^ (((r0 >> 23) ^ (r0 += 0xA99635D5B8597AE5L)) * 0xAD5DE9A61A9C3D95L);
                    array2d[x][y] = (long)(z ^ (z >> 29));
                }
            }
        }

        /**
         * Randomly fills all of {@code array2d} with random values generated from {@code seed}; can fill an element with
         * any int, positive or negative.
         * Fairly efficient; uses a fast random number generation algorithm that can avoid some unnecessary work in this
         * context, and improves quality by seeding each column differently. Generates {@code (height + 1) * width} random
         * values to fill the {@code height * width} elements in array2d.
         * @param array2d a 2D array that will be modified in-place
         * @param seed the seed for the random values, as a long
         */
        public static void randomFill(int[][] array2d, ulong seed) {
            if (array2d == null) {
                return;
            }
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            ulong r0 = seed, z;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    z = r0 ^ (((r0 >> 23) ^ (r0 += 0xA99635D5B8597AE5L)) * 0xAD5DE9A61A9C3D95L);
                    array2d[x][y] = (int)(z ^ (z >> 29));
                }
            }
        }

        /**
         * Randomly fills all of {@code array2d} with random values generated from {@code seed}, limiting results to between
         * 0 and {@code bound}, exclusive.
         * Fairly efficient; uses a fast random number generation algorithm that can avoid some unnecessary work in this
         * context, and improves quality by seeding each column differently. Generates {@code (height + 1) * width} random
         * values to fill the {@code height * width} elements in array2d.
         * @param array2d a 2D array that will be modified in-place
         * @param bound the upper exclusive limit for the ints this can produce
         * @param seed the seed for the random values, as a long
         */
        public static void randomFill(int[][] array2d, int bound, ulong seed) {
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            ulong r0 = seed, z;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    z = r0 ^ (((r0 >> 23) ^ (r0 += 0xA99635D5B8597AE5L)) * 0xAD5DE9A61A9C3D95L);
                    array2d[x][y] = (int)(((ulong)bound * ((z ^ (z >> 29)) & 0xFFFFFFFFL)) >> 32);
                }
            }
        }

        /**
         * Randomly fills all of {@code array2d} with random values generated from {@code seed}, choosing chars to place in
         * the given 2D array by selecting them at random from the given 1D char array {@code values}.
         * Fairly efficient; uses a fast random number generation algorithm that can avoid some unnecessary work in this
         * context, and improves quality by seeding each column differently. Generates {@code (height + 1) * width} random
         * values to fill the {@code height * width} elements in array2d.
         * @param array2d a 2D array that will be modified in-place
         * @param values a 1D char array containing the possible char values that can be chosen to fill array2d
         * @param seed the seed for the random values, as a long
         */
        public static void randomFill(char[][] array2d, char[] values, ulong seed) {
            if (array2d == null || values == null || values.Length < 1) {
                return;
            }
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            int bound = values.Length;
            ulong r0 = seed + (ulong)bound, z;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    z = r0 ^ (((r0 >> 23) ^ (r0 += 0xA99635D5B8597AE5L)) * 0xAD5DE9A61A9C3D95L);
                    array2d[x][y] = values[(int)(((ulong)bound * ((z ^ (z >> 29)) & 0xFFFFFFFFL)) >> 32)];
                }
            }
        }

        /**
         * Randomly fills all of {@code array2d} with random values generated from {@code seed}; can fill an element with
         * any float between 0.0 inclusive and 1.0 exclusive.
         * Fairly efficient; uses a fast random number generation algorithm that can avoid some unnecessary work in this
         * context, and improves quality by seeding each column differently. Generates {@code (height + 1) * width} random
         * values to fill the {@code height * width} elements in array2d.
         * @param array2d a 2D array that will be modified in-place
         * @param seed the seed for the random values, as a long
         */
        public static void randomFill(float[][] array2d, ulong seed) {
            if (array2d == null) {
                return;
            }
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            ulong r0 = seed, z;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    z = r0 ^ (((r0 >> 23) ^ (r0 += 0xA99635D5B8597AE5L)) * 0xAD5DE9A61A9C3D95L);
                    array2d[x][y] = ((z ^ (z >> 29)) & 0xFFFFFFL) * (1 / (1 << 24));// 0x1p - 24f;
                }
            }
        }
        /**
         * Randomly fills all of {@code array2d} with random values generated from {@code seed}, limiting results to between
         * 0 and {@code bound}, exclusive.
         * Fairly efficient; uses a fast random number generation algorithm that can avoid some unnecessary work in this
         * context, and improves quality by seeding each column differently. Generates {@code (height + 1) * width} random
         * values to fill the {@code height * width} elements in array2d.
         * @param array2d a 2D array that will be modified in-place
         * @param bound the upper exclusive limit for the floats this can produce
         * @param seed the seed for the random values, as a long
         */
        public static void randomFill(float[][] array2d, float bound, ulong seed) {
            if (array2d == null) {
                return;
            }
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            ulong r0 = seed, z;
            float mul = bound * (1 / (1 << 24));// 0x1p - 24f * bound;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    z = r0 ^ (((r0 >> 23) ^ (r0 += 0xA99635D5B8597AE5L)) * 0xAD5DE9A61A9C3D95L);
                    array2d[x][y] = ((z ^ (z >> 29)) & 0xFFFFFFL) * mul;
                }
            }
        }

        /**
         * Randomly fills all of {@code array2d} with random values generated from {@code seed}; can fill an element with
         * any double between 0.0 inclusive and 1.0 exclusive.
         * Fairly efficient; uses a fast random number generation algorithm that can avoid some unnecessary work in this
         * context, and improves quality by seeding each column differently. Generates {@code (height + 1) * width} random
         * values to fill the {@code height * width} elements in array2d.
         * @param array2d a 2D array that will be modified in-place
         * @param seed the seed for the random values, as a long
         */
        public static void randomFill(double[][] array2d, ulong seed) {
            if (array2d == null) {
                return;
            }
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            ulong r0 = seed, z;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    z = r0 ^ (((r0 >> 23) ^ (r0 += 0xA99635D5B8597AE5L)) * 0xAD5DE9A61A9C3D95L);
                    array2d[x][y] = ((z ^ (z >> 29)) & 0x1FFFFFFFFFFFFFL) * (1 / (1 << 53));// 0x1p - 53;
                }
            }
        }
        /**
         * Randomly fills all of {@code array2d} with random values generated from {@code seed}, limiting results to between
         * 0 and {@code bound}, exclusive.
         * Fairly efficient; uses a fast random number generation algorithm that can avoid some unnecessary work in this
         * context, and improves quality by seeding each column differently. Generates {@code (height + 1) * width} random
         * values to fill the {@code height * width} elements in array2d.
         * @param array2d a 2D array that will be modified in-place
         * @param bound the upper exclusive limit for the doubles this can produce
         * @param seed the seed for the random values, as a long
         */
        public static void randomFill(double[][] array2d, double bound, ulong seed) {
            if (array2d == null) {
                return;
            }
            int width = array2d.Length;
            int height = width == 0 ? 0 : array2d[0].Length;
            ulong r0 = seed, z;
            double mul = bound * (1 / (1 << 53));// 0x1p - 53 * bound;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    z = r0 ^ (((r0 >> 23) ^ (r0 += 0xA99635D5B8597AE5L)) * 0xAD5DE9A61A9C3D95L);
                    array2d[x][y] = ((z ^ (z >> 29)) & 0x1FFFFFFFFFFFFFL) * mul;
                }
            }
        }

        /**
         * Rearranges a List to use the given ordering, returning a copy; random orderings can be produced with
         * {@link squidpony.squidmath.RNG#randomOrdering(int)} or
         * {@link squidpony.squidmath.RNG#randomOrdering(int, int[])}. These orderings will never repeat an earlier element,
         * and the returned ArrayList may be shorter than the original if {@code ordering} isn't as long as {@code list}.
         * Using a random ordering is like shuffling, but allows you to repeat the shuffle exactly on other collections of
         * the same size. A reordering can also be inverted with {@link #invertOrdering(int[])} or
         * {@link #invertOrdering(int[], int[])}, getting the change that will undo another ordering.
         *
         * @param list     an ArrayList that you want a reordered version of; will not be modified.
         * @param ordering an ordering, typically produced by one of RNG's randomOrdering methods.
         * @param <T>      any generic type
         * @return a modified copy of {@code list} with its ordering changed to match {@code ordering}.
         */
        public static List<T> reorder<T>(List<T> list, int[] ordering) {
            int ol;
            if (list == null || ordering == null || (ol = Math.Min(list.Count, ordering.Length)) == 0)
                return list;
            List<T> alt = new List<T>(ol);
            for (int i = 0; i < ol; i++) {
                alt.Add(list[((ordering[i] % ol + ol) % ol)]);
            }
            return alt;
        }

        /**
         * Given an ordering such as one produced by {@link squidpony.squidmath.RNG#randomOrdering(int, int[])}, this finds
         * its inverse, able to reverse the reordering and vice versa.
         *
         * @param ordering the ordering to find the inverse for
         * @return the inverse of ordering
         */
        public static int[] invertOrdering(int[] ordering) {
            int ol = 0;
            if (ordering == null || (ol = ordering.Length) == 0) return ordering;
            int[] next = new int[ol];
            for (int i = 0; i < ol; i++) {
                if (ordering[i] < 0 || ordering[i] >= ol) return next;
                next[ordering[i]] = i;
            }
            return next;
        }

        /**
         * Given an ordering such as one produced by {@link squidpony.squidmath.RNG#randomOrdering(int, int[])}, this finds
         * its inverse, able to reverse the reordering and vice versa. This overload doesn't allocate a new int
         * array, and instead relies on having an int array of the same size as ordering passed to it as an
         * additional argument.
         *
         * @param ordering the ordering to find the inverse for
         * @param dest     the int array to put the inverse reordering into; should have the same length as ordering
         * @return the inverse of ordering; will have the same value as dest
         */
        public static int[] invertOrdering(int[] ordering, int[] dest) {
            int ol = 0;
            if (ordering == null || dest == null || (ol = Math.Min(ordering.Length, dest.Length)) == 0)
                return ordering;
            for (int i = 0; i < ol; i++) {
                if (ordering[i] < 0 || ordering[i] >= ol) return dest;
                dest[ordering[i]] = i;
            }
            return dest;
        }

        /**
         * Reverses the array given as a parameter, in-place, and returns the modified original.
         * @param data an array that will be reversed in-place
         * @return the array passed in, after reversal
         */
        public static bool[] reverse(bool[] data) {
            int sz;
            if (data == null || (sz = data.Length) <= 0) return data;
            bool t;
            for (int i = 0, j = sz - 1; i < j; i++, j--) {
                t = data[j];
                data[j] = data[i];
                data[i] = t;
            }
            return data;
        }

        /**
         * Reverses the array given as a parameter, in-place, and returns the modified original.
         * @param data an array that will be reversed in-place
         * @return the array passed in, after reversal
         */
        public static char[] reverse(char[] data) {
            int sz;
            if (data == null || (sz = data.Length) <= 0) return data;
            char t;
            for (int i = 0, j = sz - 1; i < j; i++, j--) {
                t = data[j];
                data[j] = data[i];
                data[i] = t;
            }
            return data;
        }

        /**
         * Reverses the array given as a parameter, in-place, and returns the modified original.
         * @param data an array that will be reversed in-place
         * @return the array passed in, after reversal
         */
        public static float[] reverse(float[] data) {
            int sz;
            if (data == null || (sz = data.Length) <= 0) return data;
            float t;
            for (int i = 0, j = sz - 1; i < j; i++, j--) {
                t = data[j];
                data[j] = data[i];
                data[i] = t;
            }
            return data;
        }

        /**
         * Reverses the array given as a parameter, in-place, and returns the modified original.
         * @param data an array that will be reversed in-place
         * @return the array passed in, after reversal
         */
        public static double[] reverse(double[] data) {
            int sz;
            if (data == null || (sz = data.Length) <= 0) return data;
            double t;
            for (int i = 0, j = sz - 1; i < j; i++, j--) {
                t = data[j];
                data[j] = data[i];
                data[i] = t;
            }
            return data;
        }

        /**
         * Reverses the array given as a parameter, in-place, and returns the modified original.
         * @param data an array that will be reversed in-place
         * @return the array passed in, after reversal
         */
        public static int[] reverse(int[] data) {
            int sz;
            if (data == null || (sz = data.Length) <= 0) return data;
            int t;
            for (int i = 0, j = sz - 1; i < j; i++, j--) {
                t = data[j];
                data[j] = data[i];
                data[i] = t;
            }
            return data;
        }

        /**
         * Reverses the array given as a parameter, in-place, and returns the modified original.
         * @param data an array that will be reversed in-place
         * @return the array passed in, after reversal
         */
        public static byte[] reverse(byte[] data) {
            int sz;
            if (data == null || (sz = data.Length) <= 0) return data;
            byte t;
            for (int i = 0, j = sz - 1; i < j; i++, j--) {
                t = data[j];
                data[j] = data[i];
                data[i] = t;
            }
            return data;
        }
        /**
         * Reverses the array given as a parameter, in-place, and returns the modified original.
         * @param data an array that will be reversed in-place
         * @return the array passed in, after reversal
         */
        public static T[] reverse<T>(T[] data) {
            int sz;
            if (data == null || (sz = data.Length) <= 0) return data;
            T t;
            for (int i = 0, j = sz - 1; i < j; i++, j--) {
                t = data[j];
                data[j] = data[i];
                data[i] = t;
            }
            return data;
        }


        /// <summary>
        /// Returns an initialized rectangular jagged array of the size specified.
        /// 
        /// Jagged arrays have better performance characteristics than multidemnsioal arrays
        /// and are so prefered. Initializing them is a pain, so here's a method to make
        /// it less of a pain.
        /// </summary>
        /// <typeparam name="T">The type of object the array shoudl hold</typeparam>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[][] Create<T>(int width, int height) {
            T[][] ret = new T[width][];
            for (int i = 0; i < ret.Length; i++) {
                ret[i] = new T[height];
            }
            return ret;
        }
    }
}
