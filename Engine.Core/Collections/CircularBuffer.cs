using System.Collections;

namespace Engine.Core.Collections
{
    public class CircularBuffer<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        private readonly T[] buffer;
        private int start;

        public int Count { get; set; }
        public int Capacity => buffer.Length;

        public T Oldest
        {
            get
            {
                if (Count == 0)
                {
                    throw new InvalidOperationException("Cannot access Oldest on an empty CircularBuffer.");
                }

                return buffer[start];
            }
        }

        public T Newest
        {
            get
            {
                if (Count == 0)
                {
                    throw new InvalidOperationException("Cannot access Newest on an empty CircularBuffer.");
                }

                return buffer[(start + Count - 1) % Capacity];
            }
        }

        public CircularBuffer(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity, nameof(capacity));

            buffer = new T[capacity];
        }

        public void Add(T item)
        {
            int index = (start + Count) % Capacity;
            buffer[index] = item;
            if (Count < Capacity)
            {
                Count++;
                return;
            }

            start = (start + 1) % Capacity;
        }

        public T this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count, nameof(index));

                return buffer[(start + index) % Capacity];
            }
        }

        public void Clear()
        {
            Array.Clear(buffer);
            start = 0;
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
            {
                yield return buffer[(start + index) % Capacity];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
