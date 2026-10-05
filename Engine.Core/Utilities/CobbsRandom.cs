using System.Security.Cryptography;

namespace Engine.Core.Utilities
{
    public static class CobbsRandom
    {
        private static Random _random = new();
        private static int _seed;
        public static int Seed
        {
            get => _seed;
            set
            {
                _random = new(value);
                _seed = value;
            }
        }

        public static int Next(int max)
        {
            return _random.Next(max);
        }

        public static int GetRange(int min, int max)
        {
            return _random.Next(min, max);
        }

        public static long Next(long max)
        {
            return _random.NextInt64(max);
        }

        public static long GetRange(long min, long max)
        {
            return _random.NextInt64(min, max);
        }

        public static float Next(float max)
        {
            return _random.NextSingle() * max;
        }

        public static float GetRange(float min, float max)
        {
            return min + (_random.NextSingle() * (max - min));
        }

        public static double Next(double max)
        {
            return _random.NextDouble() * max;
        }

        public static double GetRange(double min, double max)
        {
            return min + (_random.NextDouble() * (max - min));
        }

        public static decimal Next(decimal max)
        {
            return (decimal)_random.NextDouble() * max;
        }

        public static decimal GetRange(decimal min, decimal max)
        {
            return min + ((decimal)_random.NextDouble() * (max - min));
        }

        public static bool Flip()
        {
            return _random.Next(2) == 0;
        }

        public static List<T> Choose<T>(this List<T> list, int count = 1, bool duplicates = true)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
            return _random.GetItems(list.ToArray(), 3)
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}
