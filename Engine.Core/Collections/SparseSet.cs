using System.Collections;

namespace Engine.Core.Collections
{
    public class SparseSet<T> : IEnumerable<T>, IReadOnlyCollection<T> where T : class
    {
        private readonly Func<T, int> getID;
        private readonly List<T> dense;
        private readonly List<int> sparse;

        public int Count => dense.Count;

        public SparseSet(Func<T, int> getID, int initialCapacity = 16)
        {
            ArgumentNullException.ThrowIfNull(getID, nameof(getID));
            ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity, nameof(initialCapacity));

            this.getID = getID;
            dense = new(initialCapacity);
            sparse = new(initialCapacity);
        }

        public void Add(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            int id = getID(item);
            EnsureSparseCapacity(id);

            if (Contains(id))
            {
                throw new ArgumentException($"An item with ID '{id}' already exists.", nameof(item));
            }

            int denseIndex = dense.Count;
            dense.Add(item);
            sparse[id] = denseIndex;
        }

        public bool Remove(int id)
        {
            if (!Contains(id))
            {
                return false;
            }

            int denseIndex = sparse[id];
            int lastIndex = dense.Count - 1;

            if (denseIndex != lastIndex)
            {
                T movedItem = dense[lastIndex];
                int movedID = getID(movedItem);

                dense[denseIndex] = movedItem;
                sparse[movedID] = denseIndex;
            }

            dense.RemoveAt(lastIndex);
            sparse[id] = -1;

            return true;
        }

        public bool Remove(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return Remove(getID(item));
        }

        public bool Contains(int id)
        {
            return id >= 0 && id < sparse.Count && sparse[id] >= 0;
        }

        public bool Contains(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return Contains(getID(item));
        }

        public bool TryGet(int id, out T item)
        {
            if (!Contains(id))
            {
                item = default!;
                return false;
            }

            item = dense[sparse[id]];
            return true;
        }

        public T Get(int id)
        {
            if (!TryGet(id, out T item))
            {
                throw new KeyNotFoundException($"An item with ID '{id}' does not exist in the SparseSet.");
            }

            return item;
        }

        public void Clear()
        {
            dense.Clear();
            sparse.Clear();
        }

        private void EnsureSparseCapacity(int id)
        {
            while (sparse.Count <= id)
            {
                sparse.Add(-1);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return dense.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
