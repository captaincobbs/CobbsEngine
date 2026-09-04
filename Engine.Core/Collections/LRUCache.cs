namespace Engine.Core.Collections
{
    public class LRUCache<TKey, TValue> where TKey: notnull
    {
        private readonly Dictionary<TKey, LinkedListNode<Entry>> entries;
        private readonly LinkedList<Entry> usageOrder;

        public int Count => entries.Count;
        public int Capacity { get; set; }

        private readonly struct Entry
        {
            public TKey Key { get; }
            public TValue Value { get; }

            public Entry(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        public LRUCache(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity, nameof(capacity));

            Capacity = capacity;
            entries = new(capacity);
            usageOrder = [];
        }

        public void Add(TKey key, TValue value)
        {
            if (entries.TryGetValue(key, out LinkedListNode<Entry>? existingNode))
            {
                existingNode.Value = new(key, value);
                usageOrder.Remove(existingNode);
                usageOrder.AddFirst(existingNode);
                return;
            }

            LinkedListNode<Entry> node = usageOrder.AddFirst(new Entry(key, value));
            entries.Add(key, node);

            if (entries.Count > Capacity)
            {
                LinkedListNode<Entry> leastRecentlyUsed = usageOrder.Last!;
                usageOrder.RemoveLast();
                entries.Remove(leastRecentlyUsed.Value.Key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!entries.TryGetValue(key, out LinkedListNode<Entry>? node))
            {
                value = default!;
                return false;
            }

            usageOrder.Remove(node);
            usageOrder.AddFirst(node);

            value = node.Value.Value;
            return true;
        }

        public bool Remove(TKey key)
        {
            if (!entries.Remove(key, out LinkedListNode<Entry>? node))
            {
                return false;
            }

            usageOrder.Remove(node);
            return true;
        }

        public bool ContainsKey(TKey key)
        {
            return entries.ContainsKey(key);
        }

        public void Clear()
        {
            entries.Clear();
            usageOrder.Clear();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out TValue value))
                {
                    throw new KeyNotFoundException($"The key '{key}' was not found in the cache.");
                }

                return value;
            }
        }
    }
}
