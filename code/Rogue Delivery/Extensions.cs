using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueDelivery {
    public static class KeyValuePairExtansions {
        public static void Deconstruct<K, V>(this KeyValuePair<K, V> kvp, out K key, out V value) {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
