using System.Runtime.CompilerServices;

namespace SquidLib.SquidMath {
    class ArrayTools {

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
