using System;
using System.Collections.Generic;
using System.Linq;

namespace SquidLib {
    public static class EnumTools {
        /// <summary>
        /// Returns the typed IEnumerable for the given Enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();
    }
}
