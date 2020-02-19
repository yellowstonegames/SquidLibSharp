using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SquidLib.SquidMath {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Current name indicates correct level of specificity.")]
    public class IndexedSet<T> : ISet<T> {
        public HashSet<T> Set { get; private set; }
        public List<T> Items { get; private set; }

        public IndexedSet() {
            Set = new HashSet<T>();
            Items = new List<T>(16);
        }

        public IndexedSet(IEnumerable<T> collection) : this() {
            if (collection is IEnumerable<T>) {// not null check
                foreach (var t in collection)
                    Add(t);
            }
        }

        public IndexedSet(IEqualityComparer<T> comparer) {
            Set = new HashSet<T>(comparer);
            Items = new List<T>(16);
        }

        public IndexedSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : this(comparer) {
            if (collection is IEnumerable<T>) {// not null check
                foreach (var t in collection)
                    Add(t);
            }
        }

        public T this[int index] {
            get => Items[index];
            set {
                if (index < 0 || index >= Items.Count || Set.Contains(value)) return;
                Set.Remove(Items[index]);
                Set.Add(value);
                Items[index] = value;
            }
        }

        public int Count => Items.Count;

        public bool IsReadOnly => false;

        public bool Add(T item) {
            if (!Set.Add(item)) return false;
            Items.Add(item);
            return true;
        }

        public void Clear() {
            Set.Clear();
            Items.Clear();
        }

        public bool Contains(T item) => Set.Contains(item);

        public bool Remove(T item) {
            if (!Set.Remove(item)) return false;
            Items.Remove(item);
            return true;
        }

        public void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

        public void ExceptWith(IEnumerable<T> other) {
            Set.ExceptWith(other);
            Items.RemoveAll(t => !Set.Contains(t));
        }

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        public void IntersectWith(IEnumerable<T> other) {
            Set.IntersectWith(other);
            Items.RemoveAll(t => !Set.Contains(t));
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) => Set.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => Set.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => Set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => Set.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => Set.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) => Set.SetEquals(other);

        public void SymmetricExceptWith(IEnumerable<T> other) {
            Set.SymmetricExceptWith(other);
            Items.RemoveAll(t => !Set.Contains(t));
            Items.AddRange(other.Where(t => Set.Contains(t)));
        }

        public void UnionWith(IEnumerable<T> other) {
            Items.AddRange(other.Where(t => !Set.Contains(t)));
            Set.UnionWith(other);
        }

        void ICollection<T>.Add(T item) => Add(item);

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
}
