using System;
using System.Collections;
using System.Collections.Generic;

namespace SquidLib.SquidMath
{
    public class OrderedSet<T> : IList<T>
    {
        private int v1;
        private float v2;

        public OrderedSet(int size)
        {
        }

        public OrderedSet(int v1, float v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => throw new NotImplementedException();
        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        public int IndexOf(T item) => throw new NotImplementedException();
        public void Insert(int index, T item) => throw new NotImplementedException();
        public bool Remove(T item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
