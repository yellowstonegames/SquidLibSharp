using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SquidLib.SquidMath {
    public class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue> {

        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the default initial capacity, and uses the default equality
        //     comparer for the key type.
        public IndexedDictionary() {
            Dict = new Dictionary<TKey, TValue>();
            Items = new List<TKey>();
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        //     and uses the default equality comparer for the key type.
        //
        // Parameters:
        //   dictionary:
        //     The System.Collections.Generic.IDictionary`2 whose elements are copied to the
        //     new System.Collections.Generic.Dictionary`2.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     dictionary is null.
        //
        //   T:System.ArgumentException:
        //     dictionary contains one or more duplicate keys.
        public IndexedDictionary(IDictionary<TKey, TValue> dictionary) {
            Dict = new Dictionary<TKey, TValue>(dictionary);
            Items = new List<TKey>(Dict.Keys);
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the default initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        //
        // Parameters:
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        //     comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        //     for the type of the key.
        public IndexedDictionary(IEqualityComparer<TKey> comparer) {
            Dict = new Dictionary<TKey, TValue>(comparer);
            Items = new List<TKey>();
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the specified initial capacity, and uses the default equality
        //     comparer for the key type.
        //
        // Parameters:
        //   capacity:
        //     The initial number of elements that the System.Collections.Generic.Dictionary`2
        //     can contain.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     capacity is less than 0.
        public IndexedDictionary(int capacity) {
            Dict = new Dictionary<TKey, TValue>(capacity);
            Items = new List<TKey>(capacity);
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        //     and uses the specified System.Collections.Generic.IEqualityComparer`1.
        //
        // Parameters:
        //   dictionary:
        //     The System.Collections.Generic.IDictionary`2 whose elements are copied to the
        //     new System.Collections.Generic.Dictionary`2.
        //
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        //     comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        //     for the type of the key.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     dictionary is null.
        //
        //   T:System.ArgumentException:
        //     dictionary contains one or more duplicate keys.
        public IndexedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {
            Dict = new Dictionary<TKey, TValue>(dictionary, comparer);
            Items = new List<TKey>(Dict.Keys);
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        //     that is empty, has the specified initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        //
        // Parameters:
        //   capacity:
        //     The initial number of elements that the System.Collections.Generic.Dictionary`2
        //     can contain.
        //
        //   comparer:
        //     The System.Collections.Generic.IEqualityComparer`1 implementation to use when
        //     comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1
        //     for the type of the key.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     capacity is less than 0.
        public IndexedDictionary(int capacity, IEqualityComparer<TKey> comparer) {
            Dict = new Dictionary<TKey, TValue>(capacity, comparer);
            Items = new List<TKey>(capacity);
        }


        public TValue this[TKey key] { get => Dict[key]; set => Dict[key] = value; }

        public Dictionary<TKey, TValue> Dict { get; private set; }
        public List<TKey> Items { get; private set; }

        public ICollection<TKey> Keys => Items;

        public ICollection<TValue> Values => Dict.Values;

        public int Count => Items.Count;

        public bool IsReadOnly => ((IDictionary<TKey, TValue>)Dict).IsReadOnly;

        public void Add(TKey key, TValue value) {
            Dict.Add(key, value);
            Items.Add(key);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            ((IDictionary<TKey, TValue>)Dict).Add(item);
            Items.Add(item.Key);
        }

        public void Clear() {
            Dict.Clear();
            Items.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return ((IDictionary<TKey, TValue>)Dict).Contains(item);
        }

        public bool ContainsKey(TKey key) {
            return Dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            ((IDictionary<TKey, TValue>)Dict).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return ((IDictionary<TKey, TValue>)Dict).GetEnumerator();
        }

        public bool Remove(TKey key) {
            if (Dict.Remove(key)) {
                Items.Remove(key);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (((IDictionary<TKey, TValue>)Dict).Remove(item)) {
                Items.Remove(item.Key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return Dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IDictionary<TKey, TValue>)Dict).GetEnumerator();
        }
    }
}
