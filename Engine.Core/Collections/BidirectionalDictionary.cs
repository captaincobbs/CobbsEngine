using System.Collections;

namespace Engine.Core.Collections
{
    public class BidirectionalDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey: notnull where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> forward;
        private readonly Dictionary<TValue, TKey> reverse;

        public int Count => forward.Count;
        public IReadOnlyCollection<TKey> Keys => forward.Keys;
        public IReadOnlyCollection<TValue> Values => reverse.Keys;

        public BidirectionalDictionary()
        {
            forward = new();
            reverse = new();
        }

        public BidirectionalDictionary(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));

            forward = new(capacity);
            reverse = new(capacity);
        }

        public void Add(TKey key, TValue value)
        {
            if (forward.ContainsKey(key))
            {
                throw new ArgumentException($"The key '{key}' already exists.", nameof(key));
            }

            if (reverse.ContainsKey(value))
            {
                throw new ArgumentException($"The value '{value}' already exists.", nameof(value));
            }

            forward.Add(key, value);
            reverse.Add(value, key);
        }

        public bool RemoveByKey(TKey key)
        {
            if (!forward.Remove(key, out TValue? value))
            {
                return false;
            }

            reverse.Remove(value);
            return true;
        }

        public bool RemoveByValue(TValue value)
        {
            if (!reverse.Remove(value, out TKey? key))
            {
                return false;
            }

            forward.Remove(key);
            return true;
        }

        public bool ContainsKey(TKey key) => forward.ContainsKey(key);

        public bool ContainsValue(TValue value) => reverse.ContainsKey(value);

        public bool TryGetValue(TKey key, out TValue value) => forward.TryGetValue(key, out value!);

        public bool TryGetKey(TValue value, out TKey key) => reverse.TryGetValue(value, out key!);

        public TValue GetKey(TKey key)
        {
            if (!forward.TryGetValue(key, out TValue? value))
            {
                throw new KeyNotFoundException($"The key '{key}' was not found.");
            }
            return value;
        }

        public TKey GetValue(TValue value)
        {
            if (!reverse.TryGetValue(value, out TKey? key))
            {
                throw new KeyNotFoundException($"The value '{value}' was not found.");
            }
            return key;
        }

        public void SetByKey(TKey key, TValue value)
        {
            if (forward.TryGetValue(key, out TValue? existingValue))
            {
                reverse.Remove(existingValue);
            }
            if (reverse.TryGetValue(value, out TKey? existingKey))
            {
                forward.Remove(existingKey);
            }
            forward[key] = value;
            reverse[value] = key;
        }

        public void SetByValue(TValue value, TKey key)
        {
            if (reverse.TryGetValue(value, out TKey? existingKey))
            {
                forward.Remove(existingKey);
            }
            if (forward.TryGetValue(key, out TValue? existingValue))
            {
                reverse.Remove(existingValue);
            }
            reverse[value] = key;
            forward[key] = value;
        }

        public void Clear()
        {
            forward.Clear();
            reverse.Clear();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return forward.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public TValue this[TKey key]
        {
            get
            {
                return GetKey(key);
            }
            set
            {
                SetByKey(key, value);
            }    
        }
    }
}
