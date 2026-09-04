namespace Engine.Core.Collections
{
    public class ObjectPool<T> where T : class, IPoolable
    {
        private readonly Stack<T> available;
        private readonly Func<T> factory;

        public int Count => available.Count;

        public ObjectPool(Func<T> factory, int initialObjects = 0)
        {
            ArgumentNullException.ThrowIfNull(factory, nameof(factory));
            ArgumentOutOfRangeException.ThrowIfNegative(initialObjects, nameof(initialObjects));

            this.factory = factory;
            available = new(initialObjects);

            for (int index = 0; index < initialObjects; index++)
            {
                available.Push(factory());
            }
        }

        public T Rent()
        {
            T item;

            if (available.TryPop(out T? pooled))
            {
                item = pooled;
            }
            else
            {
                item = factory();
            }

            item.OnRent();
            return item;
        }

        public void Return(T item)
        {
            ArgumentNullException.ThrowIfNull(item);

            item.OnReturn();
            available.Push(item);
        }

        public void Stock(int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
            for (int index = 0; index < count; index++)
            {
                available.Push(factory());
            }
        }

        public async Task StockAsync(int count, int batchSize = 10)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
            ArgumentOutOfRangeException.ThrowIfNegative(batchSize, nameof(batchSize));

            for (int index = 0; index < count; index++)
            {
                available.Push(factory());
                if ((index + 1) % batchSize == 0)
                {
                    await Task.Yield();
                }
            }
        }
    }

    public interface IPoolable
    {
        void OnRent();
        void OnReturn();
    }
}
