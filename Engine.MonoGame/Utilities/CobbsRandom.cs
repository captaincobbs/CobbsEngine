using System;
using System.Collections.Generic;

using Engine.Core.Exceptions;

using MonoGameRandom = Microsoft.Xna.Framework.MathHelper.Random;

namespace Engine.MonoGame.Utilities
{
    public sealed class CobbsRandom
    {
        private readonly object _lock = new();
        private readonly MonoGameRandom _random;

        public CobbsRandom(ulong seed)
        {
            _random = new(seed);
        }

        /// <summary>Returns a random unsigned integer</summary>
        public uint NextUInt()
        {
            return (uint)NextULong(1UL << 32);
        }

        /// <summary>Returns a random unsigned integer between [0, max)</summary>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If max is zero</exception>
        public uint NextUInt(uint max)
        {
            ArgumentOutOfRangeException.ThrowIfZero(max, nameof(max));

            return (uint)NextULong(max);
        }

        /// <summary>Returns a random unsigned integer between [min, max)</summary>
        /// <param name="min">Inclusive minimum</param>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If min is greater than max</exception>
        public uint GetRange(uint min, uint max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

            if (min == max)
            {
                return min;
            }

            return min + NextUInt(max - min);
        }

        /// <summary>Returns a random positive integer</summary>
        public int NextInt()
        {
            lock (_lock)
            {
                return _random.Next();
            }
        }

        /// <summary>Returns a random positive integer between [0, max)</summary>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If max is zero or negative</exception>
        public int NextInt(int max)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(max, nameof(max));

            lock (_lock)
            {
                return _random.Next(max);
            }
        }

        /// <summary>Returns a random integer between [min, max)</summary>
        /// <param name="min">Inclusive minimum</param>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If min is greater than max</exception>
        public int GetRange(int min, int max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

            if (min == max)
            {
                return min;
            }

            lock (_lock)
            {
                return _random.Next(min, max);
            }
        }

        /// <summary>Returns a random positive long</summary>
        public long NextLong()
        {
            return (long)NextULong(1UL << 63);
        }

        /// <summary>Returns a random positive long between [0, max)</summary>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If max is negative or zero</exception>
        public long NextLong(long max)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(max, nameof(max));

            return (long)NextULong((ulong)max);
        }

        /// <summary>Returns a random long between [min, max)</summary>
        /// <param name="min">Inclusive minimum</param>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If min is greater than max</exception>
        public long GetRange(long min, long max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

            if (min == max)
            {
                return min;
            }

            // Flip the sign bit to max signed ordering to unsigned ordering
            const ulong signBit = 1UL << 63;

            ulong lower = unchecked((ulong)min) ^ signBit;
            ulong upper = unchecked((ulong)max) ^ signBit;

            ulong result = lower + NextULong(upper - lower);

            return unchecked((long)(result ^ signBit));
        }

        /// <summary>Returns a random unsigned long</summary>
        public ulong NextULong()
        {
            lock (_lock)
            {
                ulong a = (uint)_random.Next();
                ulong b = (uint)_random.Next();
                ulong c = (uint)_random.Next();

                return (a << 33) | (b << 2) | (c & 0b11UL);
            }
        }

        /// <summary>Returns a random positive long between [0, max)</summary>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If max is zero</exception>
        public ulong NextULong(ulong max)
        {
            ArgumentOutOfRangeException.ThrowIfZero(max, nameof(max));
            ulong threshold = unchecked(0UL - max) % max;

            while (true)
            {
                ulong value = NextULong();

                if (value >= threshold)
                {
                    return value % max;
                }
            }
        }

        /// <summary>Returns a random unsigned long between [min, max)</summary>
        /// <param name="min">Inclusive minimum</param>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If min is greater than max</exception>
        public ulong GetRange(ulong min, ulong max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

            if (min == max)
            {
                return min;
            }

            return min + NextULong(max - min);
        }

        /// <summary>Returns a random float between 0 and max, max is 1 by default</summary>
        /// <param name="max">Exclusive maximum</param>
        public float NextFloat(float max = 1.0f)
        {
            lock (_lock)
            {
                return _random.NextFloat() * max;
            }
        }

        /// <summary>Returns the same float, but randomly positive or negative</summary>
        public float RandomSign(float number)
        {
            lock (_lock)
            {
                return _random.NextBool() ? -number : number;
            }
        }

        /// <summary>Returns a random float between [min, max)</summary>
        /// <param name="min">Inclusive minimum</param>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If min is greater than max</exception>
        public float GetRange(float min, float max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

            if (float.IsNaN(min) || float.IsNaN(max))
            {
                throw new ArgumentException("Bounds cannot be NaN.");
            }

            if (min == max)
            {
                return min;
            }

            lock (_lock)
            {
                return min + (_random.NextFloat() * (max - min));
            }
        }

        /// <summary>Returns a random double between 0 and max, max is 1 by default</summary>
        /// <param name="max">Exclusive maximum</param>
        public double NextDouble(double max = 1.0d)
        {
            lock (_lock)
            {
                ulong bits = ((ulong)(uint)_random.Next() << 22) | ((uint)_random.Next() & 0x3FFFFFu);
                return bits / 9007199254740992.0d * max;
            }
        }

        /// <summary>Returns the same double, but randomly positive or negative</summary>
        public double RandomSign(double number)
        {
            lock (_lock)
            {
                return _random.NextBool() ? -number : number;
            }
        }

        /// <summary>Returns a random double within the specified range</summary>
        /// <param name="min">Inclusive minimum</param>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If min is greater than max</exception>
        public double GetRange(double min, double max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

            if (double.IsNaN(min) || double.IsNaN(max))
            {
                throw new ArgumentException("Bounds cannot be NaN.");
            }

            if (min == max)
            {
                return min;
            }

            return min + (NextDouble() * (max - min));
        }


        /// <summary>Returns a random decimal between 0 and max, max is 1 by default</summary>
        /// <param name="max">Exclusive maximum</param>
        public decimal NextDecimal(decimal max = 1.0m)
        {
            lock (_lock)
            {
                ulong bits = ((ulong)(uint)_random.Next() << 22) | ((uint)_random.Next() & 0x3FFFFFu);
                return (decimal)bits / 9007199254740992.0m * max;
            }
        }

        /// <summary>Returns the same decimal, but randomly positive or negative</summary>
        public decimal RandomSign(decimal number)
        {
            lock (_lock)
            {
                return _random.NextBool() ? -number : number;
            }
        }

        /// <summary>Returns a random decimal within the specified range</summary>
        /// <param name="min">Inclusive minimum</param>
        /// <param name="max">Exclusive maximum</param>
        /// <exception cref="ArgumentOutOfRangeException">If min is greater than max</exception>
        public decimal GetRange(decimal min, decimal max)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);

            if (min == max)
            {
                return min;
            }

            return min + (NextDecimal() * (max - min));
        }

        /// <summary>Generate a random bool</summary>
        public bool NextBool()
        {
            lock (_lock)
            {
                return _random.NextBool();
            }
        }

        /// <summary>Chooses a random element from the list</summary>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="list">The list to choose from</param>
        /// <exception cref="ArgumentEmptyException">If the list is empty</exception>
        /// <returns>The chosen element</returns>
        public T Pick<T>(IList<T> list)
        {
            ArgumentEmptyException.ThrowIfNullOrEmpty(list, nameof(list));

            lock (_lock)
            {
                return list[_random.Next(list.Count)];
            }
        }

        /// <summary>Chooses a specified number of random elements from the list</summary>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="list">The list to choose from</param>
        /// <param name="count">The number of elements to choose</param>
        /// <param name="duplicates">Whether to allow choosing duplicate elements</param>
        /// <returns>A list of the chosen random elements</returns>
        /// <exception cref="ArgumentOutOfRangeException">If count is negative</exception>
        /// <exception cref="ArgumentNullException"/>If the list is null</exception>
        /// <exception cref="ArgumentEmptyException"/>If the list is empty and count is greater than 0</exception>
        public List<T> Choose<T>(IReadOnlyList<T> list, int count = 1, bool duplicates = true)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
            ArgumentNullException.ThrowIfNull(list, nameof(list));

            if (count == 0)
            {
                return [];
            }
             
            ArgumentEmptyException.ThrowIfEmpty(list, nameof(list));

            if (!duplicates && count > list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Cannot choose more unique elements than are available in the list.");
            }

            lock (_lock)
            {
                List<T> result = new(count);
                if (duplicates)
                {
                    for (int i = 0; i < count; i++)
                    {
                        result.Add(list[_random.Next(list.Count)]);
                    }

                    return result;
                }

                T[] pool = new T[list.Count];

                for (int i = 0; i < list.Count; i++)
                {
                    pool[i] = list[i];
                }

                for (int i = 0; i < count; i++)
                {
                    int j = _random.Next(i, pool.Length);
                    (pool[i], pool[j]) = (pool[j], pool[i]);
                    result.Add(pool[i]);
                }

                return result;
            }
        }

        /// <summary>Performs an in-place shuffle of elements in the list.</summary>
        /// <exception cref="ArgumentNullException">If the list is null</exception>
        public void Shuffle<T>(IList<T> list)
        {
            ArgumentNullException.ThrowIfNull(list);

            lock (_lock)
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

        /// <summary>Returns a random value of an <see cref="Enum"/> type</summary>
        public T RandomEnum<T>() where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(NextInt(values.Length));
        }
    }
}
