using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SquidLib.SquidMath {
    class IndexedSet<T> : ISet<T> {
        public HashSet<T> set;
        public List<T> items;
        public IndexedSet() {
            set = new HashSet<T>();
            items = new List<T>(16);
        }
        public IndexedSet(IEnumerable<T> collection) : this() {
            foreach(var t in collection)
                Add(t);
        }
        public IndexedSet(IEqualityComparer<T> comparer) : this() {
            set = new HashSet<T>(comparer);
            items = new List<T>(16);
        }

        public int Count => set.Count;

        public bool IsReadOnly => false;

        public bool Add(T item) {
            if (!set.Add(item)) return false;
            items.Add(item);
            return true;
        }

        public void Clear() {
            set.Clear();
            items.Clear();
        }

        public bool Contains(T item) {
            return set.Contains(item);
        }

        public bool Remove(T item) {
            if (!set.Remove(item)) return false;
            items.Remove(item);
            return true;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            items.CopyTo(array, arrayIndex);
        }

        public void ExceptWith(IEnumerable<T> other) {
            set.ExceptWith(other);
            items.RemoveAll(t => !set.Contains(t));
        }

        public IEnumerator<T> GetEnumerator() {
            return items.GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other) {
            set.IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            return set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            return set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            return set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            return set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other) {
            return set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other) {
            return set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            set.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other) {
            set.UnionWith(other);
        }

        void ICollection<T>.Add(T item) => Add(item);

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }
}
