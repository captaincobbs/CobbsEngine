using System.Numerics;

namespace Engine.Core.Collections
{
    public class SpatialHash<T> where T : notnull
    {
        private readonly Dictionary<(int X, int Y), HashSet<T>> cells;
        private readonly Dictionary<T, (int X, int Y)> itemCells;

        public int Count => itemCells.Count;
        public float CellSize { get; init; }

        public SpatialHash(int cellSize)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cellSize, nameof(cellSize));

            CellSize = cellSize;
            cells = [];
            itemCells = [];
        }

        public void Add(T item, Vector2 position)
        {
            ArgumentException.ThrowIfNullOrEmpty(item.ToString());

            if (itemCells.ContainsKey(item))
            {
                throw new ArgumentException("An item with the same value already exists in the SpatialHash.", nameof(item));
            }

            (int X, int Y) cell = GetCell(position);
            if (!cells.TryGetValue(cell, out HashSet<T>? items))
            {
                items = [];
                cells.Add(cell, items);
            }

            items.Add(item);
            itemCells.Add(item, cell);
        }

        public bool Remove(T item)
        {
            if (!itemCells.Remove(item, out (int X, int Y) cell))
            {
                return false;
            }

            HashSet<T> items = cells[cell];
            items.Remove(item);

            if (items.Count == 0)
            {
                cells.Remove(cell);
            }

            return true;
        }

        public bool Update(T item, Vector2 position)
        {
            if (!itemCells.TryGetValue(item, out (int X, int Y) oldCell))
            {
                return false;
            }

            (int X, int Y) newCell = GetCell(position);
            if (oldCell == newCell)
            {
                return true;
            }

            HashSet<T> oldItems = cells[oldCell];
            oldItems.Remove(item);

            if (oldItems.Count == 0)
            {
                cells.Remove(oldCell);
            }

            if (!cells.TryGetValue(newCell, out HashSet<T>? newItems))
            {
                newItems = [];
                cells.Add(newCell, newItems);
            }

            newItems.Add(item);
            itemCells[item] = newCell;

            return true;
        }

        public IEnumerable<T> Query(Vector2 position, float radius)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(radius, nameof(radius));

            float radiusSquared = radius * radius;

            int minimumX = GetCellCoordinate(position.X - radius);
            int maximumX = GetCellCoordinate(position.X + radius);
            int minimumY = GetCellCoordinate(position.Y - radius);
            int maximumY = GetCellCoordinate(position.Y + radius);

            HashSet<T> results = [];
            for (int x = minimumX; x <= maximumX; x++)
            {
                for (int y = minimumY; y <= maximumY; y++)
                {
                    if (!cells.TryGetValue((x, y), out HashSet<T>? items))
                    {
                        continue;
                    }

                    foreach (T item in items)
                    {
                        (int X, int Y) = itemCells[item];
                        Vector2 itemPosition = new Vector2(X * CellSize, Y * CellSize);

                        if (Vector2.DistanceSquared(position, itemPosition) <= radiusSquared)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        public IEnumerable<T> QueryCell(Vector2 position)
        {
            (int X, int Y) cell = GetCell(position);

            if (!cells.TryGetValue(cell, out HashSet<T>? items))
            {
                yield break;
            }

            foreach (T item in items)
            {
                yield return item;
            }
        }

        public void Clear()
        {
            cells.Clear();
            itemCells.Clear();
        }

        public bool Contains(T item)
        {
            return itemCells.ContainsKey(item);
        }

        private (int X, int Y) GetCell(Vector2 position)
        {
            return (GetCellCoordinate(position.X), GetCellCoordinate(position.Y));
        }

        private int GetCellCoordinate(float coordinate)
        {
            return (int)MathF.Floor(coordinate / CellSize);
        }
    }
}
