using System.Collections;
using System.Numerics;

namespace Engine.Core.Collections
{
    /// <summary>Represents a list of items with associated weights, useful for weighted random selection.</summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    /// <typeparam name="TWeight">The numeric type of the weights for the items.</typeparam>
    public class WeightedList<T, TWeight> : IEnumerable<WeightedListItem<T, TWeight>>, IReadOnlyCollection<WeightedListItem<T, TWeight>>
        where TWeight : INumber<TWeight> where T: notnull
    {
        private readonly Dictionary<T, TWeight> weights;
        private readonly Random random;

        public TWeight TotalWeight { get; private set; } = TWeight.Zero;
        public TWeight MinWeight { get; private set; } = TWeight.Zero;
        public TWeight MaxWeight { get; private set; } = TWeight.Zero;

        public IReadOnlyCollection<T> Items => weights.Keys;
        public int Count => weights.Count;

        public WeightedList(Random? random = null)
        {
            weights = [];
            this.random = random ?? new();
        }

        public WeightedList(IDictionary<T, TWeight> items, Random? random = null) : this(random)
        {
            foreach (KeyValuePair<T, TWeight> item in items)
            {
                Add(item.Key, item.Value);
            }
        }

        public WeightedList(ICollection<WeightedListItem<T, TWeight>> items, Random? random = null) : this(random)
        {
            Add(items);
        }

        /// <summary>Adds an item with a specified weight to the weighted list.</summary>
        /// <param name="item">The item to add.</param>
        /// <param name="weight">The weight of the item.</param>
        public void Add(T item, TWeight weight)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(weight, nameof(weight));

            weights.Add(item, weight);
            TotalWeight += weight;

            if (weights.Count == 1)
            {
                MinWeight = weight;
                MaxWeight = weight;
                return;
            }

            if (weight < MinWeight)
            {
                MinWeight = weight;
            }

            if (weight > MaxWeight)
            {
                MaxWeight = weight;
            }
        }

        /// <summary>Adds a collection of weighted items to the weighted list.</summary>
        /// <param name="items">The items to add.</param>
        public void Add(ICollection<WeightedListItem<T, TWeight>> items)
        {
            foreach (WeightedListItem<T, TWeight> item in items)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(item.Weight, nameof(item.Weight));
                weights.Add(item.Item, item.Weight);
            }

            Recalculate();
        }

        /// <summary>Removes all items from the weighted list.</summary>
        public void Clear()
        {
            weights.Clear();
            TotalWeight = TWeight.Zero;
            MinWeight = TWeight.Zero;
            MaxWeight = TWeight.Zero;
        }

        /// <summary>Determines whether the weighted list contains a specific item.</summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>true if the item is found; otherwise, false.</returns>
        public bool Contains(T item)
        {
            return weights.ContainsKey(item);
        }

        /// <summary>Removes a specific item from the weighted list.</summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>true if the item was removed; otherwise, false.</returns>
        public bool Remove(T item)
        {
            if (!weights.Remove(item, out TWeight? weight))
            {
                return false;
            }

            TotalWeight -= weight;
            if (weights.Count == 0)
            {
                MinWeight = TWeight.Zero;
                MaxWeight = TWeight.Zero;
            }
            else if (weight == MinWeight || weight == MaxWeight)
            {
                Recalculate();
            }

            return true;
        }

        /// <summary>Sets the weight of a specific item in the weighted list.</summary>
        /// <param name="item">The item for which to set the weight.</param>
        /// <param name="weight">The new weight for the item.</param>
        /// <exception cref="KeyNotFoundException">Thrown when the item does not exist in the WeightedList.</exception>
        public void SetWeight(T item, TWeight weight)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(weight, nameof(weight));

            if (!weights.TryGetValue(item, out TWeight? oldWeight))
            {
                throw new KeyNotFoundException($"Item '{item}' does not exist in the WeightedList.");
            }

            weights[item] = weight;
            if (oldWeight == MinWeight || oldWeight == MaxWeight)
            {
                Recalculate();
                return;
            }

            TotalWeight += weight - oldWeight;
            if (weight < MinWeight)
            {
                MinWeight = weight;
            }

            if (weight > MaxWeight)
            {
                MaxWeight = weight;
            }
        }

        /// <summary>Gets the weight of a specific item in the weighted list.</summary>
        /// <param name="item">The item for which to get the weight.</param>
        /// <returns>The weight of the item.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the item does not exist in the WeightedList.</exception>
        public TWeight GetWeightOf(T item)
        {
            if (!weights.TryGetValue(item, out TWeight? weight))
            {
                throw new KeyNotFoundException($"Item '{item}' does not exist in the WeightedList.");
            }

            return weight;
        }

        /// <summary>Picks a random item from the weighted list.</summary>
        /// <returns>The picked item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
        public T Pick()
        {
            if (!TryPick(out T item))
            {
                throw new InvalidOperationException("Cannot select a random item from an empty WeightedList.");
            }

            return item;
        }

        /// <summary>Attempts to pick a random item from the weighted list.</summary>
        /// <param name="item">The picked item, if successful.</param>
        /// <returns>true if an item was picked; otherwise, false.</returns>
        public bool TryPick(out T item)
        {
            if (weights.Count == 0 || TotalWeight <= TWeight.Zero)
            {
                item = default!;
                return false;
            }

            double randomValue = random.NextDouble();
            double target = randomValue * double.CreateChecked(TotalWeight);
            double accumulatedWeight = 0.0d;

            foreach (KeyValuePair<T, TWeight> weightedItem in weights)
            {
                accumulatedWeight += double.CreateChecked(weightedItem.Value);
                if (target < accumulatedWeight)
                {
                    item = weightedItem.Key;
                    return true;
                }
            }

            item = weights.Keys.Last();
            return true;
        }

        /// <summary>Picks multiple items randomly from the weighted list.</summary>
        /// <param name="count">The number of items to pick.</param>
        /// <param name="allowDuplicates">Indicates whether duplicates are allowed.</param>
        /// <returns>The list of picked items.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the count is negative or greater than the number of unique items.</exception>
        public List<T> PickMany(int count, bool allowDuplicates = false)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));

            if (!allowDuplicates && count > weights.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be greater than the number of unique items when duplicates are not allowed.");
            }

            List<T> selectedItems = new(count);
            if (allowDuplicates)
            {
                for (int index = 0; index < count; index++)
                {
                    if (TryPick(out T item))
                    {
                        selectedItems.Add(item);
                    }
                }

                return selectedItems;
            }

            WeightedList<T, TWeight> buffer = new(random);

            foreach (WeightedListItem<T, TWeight> item in this)
            {
                buffer.Add(item.Item, item.Weight);
            }

            for (int index = 0; index < count; index++)
            {
                if (!buffer.TryPick(out T item))
                {
                    break;
                }

                selectedItems.Add(item);
                buffer.Remove(item);
            }

            return selectedItems;
        }

        private void Recalculate()
        {
            TotalWeight = TWeight.Zero;

            if (weights.Count == 0)
            {
                MinWeight = TWeight.Zero;
                MaxWeight = TWeight.Zero;
                return;
            }

            bool firstWeight = true;
            foreach (TWeight weight in weights.Values)
            {
                TotalWeight += weight;

                // Prevent MinWeight from always being zero by setting it to the weight of the first item
                if (firstWeight)
                {
                    MinWeight = weight;
                    MaxWeight = weight;
                    firstWeight = false;
                    continue;
                }

                if (weight < MinWeight)
                {
                    MinWeight = weight;
                }

                if (weight > MaxWeight)
                {
                    MaxWeight = weight;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerator<WeightedListItem<T, TWeight>> GetEnumerator()
        {
            foreach (KeyValuePair<T, TWeight> item in weights)
            {
                yield return new WeightedListItem<T, TWeight>(item.Key, item.Value);
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Converts the weighted list to a dictionary.</summary>
        /// <returns>The dictionary containing the items and their weights.</returns>
        public Dictionary<T, TWeight> ToDictionary()
        {
            return new Dictionary<T, TWeight>(weights);
        }

        /// <summary>Creates a new weighted list from a dictionary.</summary>
        /// <param name="dictionary">The dictionary containing the items and their weights.</param>
        /// <param name="random">The optional random number generator to use.</param>
        /// <returns>The new weighted list.</returns>
        public static WeightedList<T, TWeight> FromDictionary(IDictionary<T, TWeight> dictionary, Random? random = null)
        {
            return new WeightedList<T, TWeight>(dictionary, random);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{ " + string.Join(", ", weights.Select(item => $"({item.Key}, {item.Value})")) + " }";
        }
    }

    public class WeightedListItem<T, TWeight> where TWeight : INumber<TWeight> where T : notnull
    {
        public T Item { get; }
        public TWeight Weight { get; }

        public WeightedListItem(T item, TWeight weight)
        {
            Item = item;
            Weight = weight;
        }

        public override string ToString()
        {
            return $"({Item}, {Weight})";
        }
    }
}
