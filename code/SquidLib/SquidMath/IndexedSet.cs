using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SquidLib.SquidMath {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Current name indicates correct level of specificity.")]
    public class IndexedSet<T> : ISet<T>, IEquatable<IndexedSet<T>>, IOrdered<T> {
        public HashSet<T> Set { get; private set; }
        public List<T> Ordering { get; private set; }

        public IndexedSet() {
            Set = new HashSet<T>();
            Ordering = new List<T>(16);
        }

        public IndexedSet(IEnumerable<T> collection) : this() {
            if (collection is IEnumerable<T>) {// not null check
                foreach (var t in collection)
                    Add(t);
            }
        }

        public IndexedSet(IEqualityComparer<T> comparer) {
            Set = new HashSet<T>(comparer);
            Ordering = new List<T>(16);
        }

        public IndexedSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : this(comparer) {
            if (collection is IEnumerable<T>) {// not null check
                foreach (var t in collection)
                    Add(t);
            }
        }

        public T this[int index] {
            get => Ordering[index];
            set {
                if (index < 0 || index >= Ordering.Count || Set.Contains(value)) return;
                Set.Remove(Ordering[index]);
                Set.Add(value);
                Ordering[index] = value;
            }
        }

        public int Count => Ordering.Count;

        public bool IsReadOnly => false;

        public bool Add(T item) {
            if (!Set.Add(item)) return false;
            Ordering.Add(item);
            return true;
        }

        public void Clear() {
            Set.Clear();
            Ordering.Clear();
        }

        public bool Contains(T item) => Set.Contains(item);

        public bool Remove(T item) {
            if (!Set.Remove(item)) return false;
            Ordering.Remove(item);
            return true;
        }

        public void CopyTo(T[] array, int arrayIndex) => Ordering.CopyTo(array, arrayIndex);

        public void ExceptWith(IEnumerable<T> other) {
            Set.ExceptWith(other);
            Ordering.RemoveAll(t => !Set.Contains(t));
        }

        public IEnumerator<T> GetEnumerator() => Ordering.GetEnumerator();

        public void IntersectWith(IEnumerable<T> other) {
            Set.IntersectWith(other);
            Ordering.RemoveAll(t => !Set.Contains(t));
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) => Set.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => Set.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => Set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => Set.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => Set.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) => Set.SetEquals(other);

        public void SymmetricExceptWith(IEnumerable<T> other) {
            Set.SymmetricExceptWith(other);
            Ordering.RemoveAll(t => !Set.Contains(t));
            Ordering.AddRange(other.Where(t => Set.Contains(t)));
        }

        public void UnionWith(IEnumerable<T> other) {
            Ordering.AddRange(other.Where(t => !Set.Contains(t)));
            Set.UnionWith(other);
        }

        void ICollection<T>.Add(T item) => Add(item);

        IEnumerator IEnumerable.GetEnumerator() => Ordering.GetEnumerator();

        public override bool Equals(object obj) => Equals(obj as IndexedSet<T>);

        public bool Equals(IndexedSet<T> other) => other != null &&
                   EqualityComparer<List<T>>.Default.Equals(Ordering, other.Ordering);

        public override int GetHashCode() => -604923257 + EqualityComparer<List<T>>.Default.GetHashCode(Ordering);

        public static bool operator ==(IndexedSet<T> left, IndexedSet<T> right) => EqualityComparer<IndexedSet<T>>.Default.Equals(left, right);

        public static bool operator !=(IndexedSet<T> left, IndexedSet<T> right) => !(left == right);
    }
}
