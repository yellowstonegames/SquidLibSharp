using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SquidLib.SquidMath {
    public class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue> {

        /// <summary>
        ///     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        ///     that is empty, has the default initial capacity, and uses the default equality
        ///     comparer for the key type.
        /// </summary>
        public IndexedDictionary() {
            Dict = new Dictionary<TKey, TValue>();
            Items = new List<TKey>();
        }
        /// <summary>
        ///     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        ///     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        ///     and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The System.Collections.Generic.IDictionary`2 whose elements are copied to the new System.Collections.Generic.Dictionary`2.</param>
        public IndexedDictionary(IDictionary<TKey, TValue> dictionary) {
            Dict = new Dictionary<TKey, TValue>(dictionary);
            Items = new List<TKey>(Dict.Keys);
        }
        ///     
        /// <summary>
        ///     Initializes a new instance of the System.Collections.Generic.Dictionary`2 class
        ///     that is empty, has the default initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        /// </summary>
        /// <param name="comparer">The System.Collections.Generic.IEqualityComparer`1 implementation to use when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1 for the type of the key.</param>
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

        private ValueCollection values;
        public ICollection<TValue> Values {
            get {
                if (values == null) values = new ValueCollection(Dict, Items);
                return values;
            }
        }

        public int Count => Items.Count;

        public bool IsReadOnly => false;

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
        public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue> {
            private readonly List<TKey> _items;
            private readonly Dictionary<TKey, TValue> _dictionary;

            public ValueCollection(Dictionary<TKey, TValue> dictionary, List<TKey> items) {
                if (dictionary == null) {
                    throw new ArgumentNullException(nameof(dictionary));
                }
                if (items == null) {
                    throw new ArgumentNullException(nameof(items));
                }
                _dictionary = dictionary;
                _items = items;
            }

            public Enumerator GetEnumerator()
                => new Enumerator(_dictionary, _items);

            public void CopyTo(TValue[] array, int index) {
                if (array == null) {
                    throw new ArgumentNullException(nameof(array));
                }

                if ((uint)index > array.Length) {
                    throw new IndexOutOfRangeException("The index is not within the range for the array.");
                }

                int count = _items.Count;
                if (array.Length - index < count) {
                    throw new ArgumentException("The given array is too small for the copied IndexedDictionary.");
                }

                for (int i = 0; i < count; i++) {
                    array[index++] = _dictionary[_items[i]];
                }
            }

            public int Count => _dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;

            void ICollection<TValue>.Add(TValue item)
                => throw new NotSupportedException("An IndexedDictionary cannot be modified via its values collection.");

            bool ICollection<TValue>.Remove(TValue item) {
                throw new NotSupportedException("An IndexedDictionary cannot be modified via its values collection.");
            }

            void ICollection<TValue>.Clear()
                => throw new NotSupportedException("An IndexedDictionary cannot be modified via its values collection.");

            bool ICollection<TValue>.Contains(TValue item)
                => _dictionary.ContainsValue(item);

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
                => new Enumerator(_dictionary, _items);

            IEnumerator IEnumerable.GetEnumerator()
                => new Enumerator(_dictionary, _items);

            void ICollection.CopyTo(Array array, int index) {
                if (array == null) throw new ArgumentNullException(nameof(array));
                if (array.Rank != 1) throw new ArgumentException("Multidimensional arrays are not supported for this CopyTo() operation.");
                if (array.GetLowerBound(0) != 0)
                    throw new ArgumentException("The array must not have a lower bound other than 0.");
                if ((uint)index > (uint)array.Length)
                    throw new IndexOutOfRangeException("The index is not within the range for the array.");
                if (array.Length - index < _dictionary.Count)
                    throw new ArgumentException("The given array is too small for the copied IndexedDictionary.");

                if (array is TValue[] values) {
                    CopyTo(values, index);
                } else {
                    object[] objects = array as object[];
                    if (objects == null) {
                        throw new ArgumentException("The given array has an invalid type.");
                    }

                    int count = _items.Count;
                    try {
                        for (int i = 0; i < count; i++) {
                            objects[index++] = _dictionary[_items[i]];
                        }
                    } catch (ArrayTypeMismatchException) {
                        throw new ArgumentException("The given array has an invalid type.");
                    }
                }
            }

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

            public struct Enumerator : IEnumerator<TValue>, IEnumerator {
                private readonly Dictionary<TKey, TValue> _dictionary;
                private readonly List<TKey> _items;
                private int _index;
                private TValue _currentValue;

                internal Enumerator(Dictionary<TKey, TValue> dictionary, List<TKey> items) {
                    _dictionary = dictionary;
                    _items = items;
                    _index = 0;
                    _currentValue = default;
                }

                public void Dispose() {
                }

                public bool MoveNext() {
                    if(_index < _items.Count) {
                        _currentValue = _dictionary[_items[_index++]];
                        return true;
                    }
                    _index = _items.Count;
                    _currentValue = default;
                    return false;
                }

                public TValue Current => _currentValue;

                object IEnumerator.Current {
                    get {
                        if (_index == 0 || (_index == _items.Count)) {
                            throw new InvalidOperationException("Cannot get current item if the Enumerator hasn't started or has ended.");
                        }

                        return _currentValue;
                    }
                }

                void IEnumerator.Reset() {
                    _index = 0;
                    _currentValue = default;
                }
            }
        }
    }
}
