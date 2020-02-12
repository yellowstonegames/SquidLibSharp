using System;
using System.Collections;
using System.Collections.Generic;

namespace SquidLib.SquidMath {
    public class OrderedSet<T> : ISet<T> {
        private SortedDictionary<int, T> set;
        private Dictionary<T, int> contents;
        private int indexCounter = 0;

        public OrderedSet(int capacity) {
            set = new SortedDictionary<int, T>();
            for (int i = 0; i < capacity; i++) // No capacity parameter, have to manually work it up
            {
                set.Add(i, default(T));
            }
            set.Clear();
            contents = new Dictionary<T, int>(capacity);
        }

        public OrderedSet(int capacity, float loadFactor) : this(capacity) { } // TODO - use loadFactor

        public T this[int index] { get => set[index]; set => set[index] = value; }

        public int Count => set.Count;

        public bool IsReadOnly => false;

        public bool Add(T item) {
            if (Contains(item)) {
                return false;
            }

            set[indexCounter] = item;
            contents[item] = indexCounter;
            indexCounter++;
            return true;
        }

        public void Clear() {
            contents.Clear();
            set.Clear();
        }

        public bool Contains(T item) => contents.ContainsKey(item);

        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();

        public void ExceptWith(IEnumerable<T> other) => throw new NotImplementedException();

        public IEnumerator<T> GetEnumerator() {
            foreach (KeyValuePair<int, T> kvp in set) {
                yield return kvp.Value;
            }
        }

        public void IntersectWith(IEnumerable<T> other) => throw new NotImplementedException();

        public bool IsProperSubsetOf(IEnumerable<T> other) => throw new NotImplementedException();

        public bool IsProperSupersetOf(IEnumerable<T> other) => throw new NotImplementedException();

        public bool IsSubsetOf(IEnumerable<T> other) => throw new NotImplementedException();

        public bool IsSupersetOf(IEnumerable<T> other) => throw new NotImplementedException();

        public bool Overlaps(IEnumerable<T> other) => throw new NotImplementedException();

        public bool Remove(T item) {
            if (contents.TryGetValue(item, out int i)) {
                contents.Remove(item);
                set.Remove(i);
                return true;
            }
            return false;
        }

        public bool SetEquals(IEnumerable<T> other) => throw new NotImplementedException();

        public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotImplementedException();

        public void UnionWith(IEnumerable<T> other) => throw new NotImplementedException();

        void ICollection<T>.Add(T item) => Add(item);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
