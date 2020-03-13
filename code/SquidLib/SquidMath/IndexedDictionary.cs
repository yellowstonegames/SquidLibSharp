using System;
using System.Collections;
using System.Collections.Generic;

namespace SquidLib.SquidMath {
    /// <summary>
    /// Only for use with indexers in IndexedDictionary to show you want to access a key.
    /// There is only one enum constant here, At.
    /// </summary>
    public enum Key {
        At
    }
    /// <summary>
    /// Only for use with indexers in IndexedDictionary to show you want to access a value.
    /// There is only one enum constant here, At.
    /// </summary>
    public enum Value {
        At
    }
    /// <summary>
    /// An IDictionary that also allows access to its keys and values in insertion-order, allowing random access.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class IndexedDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, ICollection, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IOrdered<TKey>, IEquatable<IndexedDictionary<TKey, TValue>> {

        /// <summary>
        ///     Initializes a new instance of the SquidLib.SquidMath.IndexedDictionary class
        ///     that is empty, has the default initial capacity, and uses the default equality
        ///     comparer for the key type.
        /// </summary>
        public IndexedDictionary() {
            Dict = new Dictionary<TKey, TValue>();
            Ordering = new List<TKey>();
        }
        /// <summary>
        ///     Initializes a new instance of the SquidLib.SquidMath.IndexedDictionary class
        ///     that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        ///     and uses the default equality comparer for the key type.
        /// </summary>
        /// <param name="dictionary">The System.Collections.Generic.IDictionary`2 whose elements are copied to the new SquidLib.SquidMath.IndexedDictionary.</param>
        public IndexedDictionary(IDictionary<TKey, TValue> dictionary) {
            Dict = new Dictionary<TKey, TValue>(dictionary);
            Ordering = new List<TKey>(Dict.Keys);
        }
        /// <summary>
        ///     Initializes a new instance of the SquidLib.SquidMath.IndexedDictionary class
        ///     that is empty, has the default initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        /// </summary>
        /// <param name="comparer">The System.Collections.Generic.IEqualityComparer`1 implementation to use when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1 for the type of the key.</param>
        public IndexedDictionary(IEqualityComparer<TKey> comparer) {
            Dict = new Dictionary<TKey, TValue>(comparer);
            Ordering = new List<TKey>();
        }
        /// <summary>
        /// Initializes a new instance of the SquidLib.SquidMath.IndexedDictionary class
        /// that is empty, has the specified initial capacity, and uses the default equality
        /// comparer for the key type.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the SquidLib.SquidMath.IndexedDictionary can contain.</param>
        public IndexedDictionary(int capacity) {
            Dict = new Dictionary<TKey, TValue>(capacity);
            Ordering = new List<TKey>(capacity);
        }
        /// <summary>
        /// Initializes a new instance of the SquidLib.SquidMath.IndexedDictionary class
        /// that contains elements copied from the specified System.Collections.Generic.IDictionary`2
        /// and uses the specified System.Collections.Generic.IEqualityComparer`1.
        /// </summary>
        /// <param name="dictionary">The System.Collections.Generic.IDictionary`2 whose elements are copied to the new SquidLib.SquidMath.IndexedDictionary.</param>
        /// <param name="comparer">The System.Collections.Generic.IEqualityComparer`1 implementation to use when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1 for the type of the key.</param>
        public IndexedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {
            Dict = new Dictionary<TKey, TValue>(dictionary, comparer);
            Ordering = new List<TKey>(Dict.Keys);
        }
        /// <summary>
        /// Initializes a new instance of the SquidLib.SquidMath.IndexedDictionary class
        /// that is empty, has the specified initial capacity, and uses the specified System.Collections.Generic.IEqualityComparer`1.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the SquidLib.SquidMath.IndexedDictionary can contain.</param>
        /// <param name="comparer">The System.Collections.Generic.IEqualityComparer`1 implementation to use when comparing keys, or null to use the default System.Collections.Generic.EqualityComparer`1 for the type of the key.</param>
        public IndexedDictionary(int capacity, IEqualityComparer<TKey> comparer) {
            Dict = new Dictionary<TKey, TValue>(capacity, comparer);
            Ordering = new List<TKey>(capacity);
        }


        public TValue this[TKey key] {
            get => Dict[key];
            set {
                if (!Dict.ContainsKey(key))
                    Ordering.Add(key);
                Dict[key] = value;
            }
        }

        /// <summary>
        /// Use with <code>MyKey k = myIndexedDictionary[Key.At, 0]</code> or <code>myIndexedDictionary[Key.At, 0] = k;</code>
        /// </summary>
        /// <param name="op">Any Key enum, which means just Key.At</param>
        /// <param name="index">The index into the ordering of the IndexedSet; must be at least 0 and less than Count</param>
        /// <returns>the TKey at the given index</returns>
        public TKey this[Key op, int index] {
            get => Ordering[index];
            set {
                TValue v = Dict[Ordering[index]];
                Dict.Remove(Ordering[index]);
                Dict[value] = v;
            }
        }
        /// <summary>
        /// Use with <code>MyValue v = myIndexedDictionary[Value.At, 0]</code> or <code>myIndexedDictionary[Value.At, 0] = v;</code>
        /// </summary>
        /// <param name="op">Any Value enum, which means just Value.At</param>
        /// <param name="index">The index into the ordering of the IndexedSet; must be at least 0 and less than Count</param>
        /// <returns>the TValue at the given index</returns>
        public TValue this[Value op, int index] {
            get => Dict[Ordering[index]];
            set => Dict[Ordering[index]] = value;
        }

        public Dictionary<TKey, TValue> Dict { get; private set; }
        public List<TKey> Ordering { get; private set; }

        private Object _syncRoot;

        public ICollection<TKey> Keys => Ordering;

        private ValueCollection values;
        public ICollection<TValue> Values {
            get {
                if (values == null) values = new ValueCollection(Dict, Ordering);
                return values;
            }
        }

        public int Count => Ordering.Count;

        public bool IsReadOnly => false;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Ordering;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values {
            get {
                if (values == null) values = new ValueCollection(Dict, Ordering);
                return values;
            }
        }

        public bool IsSynchronized => false;

        public object SyncRoot {
            get {
                if (_syncRoot == null) {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        /// <summary>
        /// Adds at the end of the ordering, or throws an ArgumentException if key is already present.
        /// </summary>
        /// <param name="key">The key to add; should not be present in this already.</param>
        /// <param name="value">The value to associate with the given key.</param>
        public void Add(TKey key, TValue value) {
            Dict.Add(key, value);
            Ordering.Add(key);
        }

        /// <summary>
        /// Adds at the end of the ordering, or throws an ArgumentException if the key in item is already present.
        /// </summary>
        /// <param name="item">The key and value to add; the key should not be present in this already.</param>
        public void Add(KeyValuePair<TKey, TValue> item) {
            ((IDictionary<TKey, TValue>)Dict).Add(item);
            Ordering.Add(item.Key);
        }

        public void Clear() {
            Dict.Clear();
            Ordering.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Dict).Contains(item);

        public bool ContainsKey(TKey key) => Dict.ContainsKey(key);

        /// <summary>
        /// Copies the entries as KeyValuePair s into the given array; NOT ORDERED.
        /// </summary>
        /// <param name="array">The array to copy into.</param>
        /// <param name="arrayIndex">The first index in array to insert this into.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)Dict).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(Dict, Ordering);

        public bool Remove(TKey key) {
            if (Dict.Remove(key)) {
                Ordering.Remove(key);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (((IDictionary<TKey, TValue>)Dict).Remove(item)) {
                Ordering.Remove(item.Key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) => Dict.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(Dict, Ordering);

        public override bool Equals(object obj) => Equals(obj as IndexedDictionary<TKey, TValue>);

        public bool Equals(IndexedDictionary<TKey, TValue> other) => other != null &&
                   EqualityComparer<Dictionary<TKey, TValue>>.Default.Equals(Dict, other.Dict) &&
                   EqualityComparer<List<TKey>>.Default.Equals(Ordering, other.Ordering);

        public override int GetHashCode() {
            var hashCode = -392379326;
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<TKey, TValue>>.Default.GetHashCode(Dict);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<TKey>>.Default.GetHashCode(Ordering);
            return hashCode;
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        internal sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue> {
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

            bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException("An IndexedDictionary cannot be modified via its values collection.");

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

                internal Enumerator(Dictionary<TKey, TValue> dictionary, List<TKey> items) {
                    _dictionary = dictionary;
                    _items = items;
                    _index = 0;
                    Current = default;
                }

                public void Dispose() {
                }

                public bool MoveNext() {
                    if (_index < _items.Count) {
                        Current = _dictionary[_items[_index++]];
                        return true;
                    }
                    _index = _items.Count;
                    Current = default;
                    return false;
                }

                public TValue Current { get; private set; }

                object IEnumerator.Current {
                    get {
                        if (_index <= 0 || (_index == _items.Count)) {
                            throw new InvalidOperationException("Cannot get current item if the Enumerator hasn't started or has ended.");
                        }

                        return Current;
                    }
                }

                void IEnumerator.Reset() {
                    _index = 0;
                    Current = default;
                }
            }
        }

        public static bool operator ==(IndexedDictionary<TKey, TValue> left, IndexedDictionary<TKey, TValue> right) => EqualityComparer<IndexedDictionary<TKey, TValue>>.Default.Equals(left, right);

        public static bool operator !=(IndexedDictionary<TKey, TValue> left, IndexedDictionary<TKey, TValue> right) => !(left == right);
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator {
            private readonly Dictionary<TKey, TValue> _dictionary;
            private readonly List<TKey> _items;
            private int _index;

            internal Enumerator(Dictionary<TKey, TValue> dictionary, List<TKey> items) {
                _dictionary = dictionary;
                _items = items;
                _index = 0;
                Current = default;
            }

            public void Dispose() {
            }

            public bool MoveNext() {
                if (_index < _items.Count) {
                    Current = new KeyValuePair<TKey, TValue>(_items[_index], _dictionary[_items[_index++]]);
                    return true;
                }
                _index = _items.Count;
                Current = default;
                return false;
            }

            public KeyValuePair<TKey, TValue> Current { get; private set; }

            object IEnumerator.Current {
                get {
                    if (_index <= 0 || (_index == _items.Count)) {
                        throw new InvalidOperationException("Cannot get current item if the Enumerator hasn't started or has ended.");
                    }

                    return Current;
                }
            }

            void IEnumerator.Reset() {
                _index = 0;
                Current = default;
            }
        }
    }
}