namespace Engine.Core.Collections
{
    public class DependencyGraph<T> where T : notnull
    {
        private readonly Dictionary<T, HashSet<T>> dependencies;
        private readonly Dictionary<T, HashSet<T>> dependents;

        public int Count => dependencies.Count;
        public IEnumerable<T> Nodes => dependencies.Keys;

        public DependencyGraph()
        {
            dependencies = [];
            dependents = [];
        }

        public void Add(T item)
        {
            dependencies.TryAdd(item, []);
            dependents.TryAdd(item, []);
        }

        public void Add(Dictionary<T, HashSet<T>> items)
        {
            foreach (KeyValuePair<T, HashSet<T>> item in items)
            {
                AddDependencies(item.Key, item.Value);
            }
        }

        public bool Remove(T item)
        {
            if (!dependencies.TryGetValue(item, out HashSet<T>? value))
            {
                return false;
            }

            foreach (T dependency in value)
            {
                dependents[dependency].Remove(item);
            }

            foreach (T dependent in dependents[item])
            {
                dependencies[dependent].Remove(item);
            }

            dependencies.Remove(item);
            dependents.Remove(item);

            return true;
        }

        public bool AddDependency(T item, T dependency)
        {
            Add(item);
            Add(dependency);

            if (!dependencies[item].Add(dependency))
            {
                return false;
            }

            dependents[dependency].Add(item);
            return true;
        }

        public void AddDependencies(T item, HashSet<T> dependencies)
        {
            Add(item);
            foreach (T dependency in dependencies)
            {
                AddDependency(item, dependency);
            }
        }

        public bool RemoveDependency(T item, T dependency)
        {
            if (!dependencies.TryGetValue(item, out HashSet<T>? itemDependencies))
            {
                return false;
            }

            if (!itemDependencies.Remove(dependency))
            {
                return false;
            }

            dependents[dependency].Remove(item);

            return true;
        }

        public bool HasDependency(T item, T dependency)
        {
            return dependencies.TryGetValue(item, out HashSet<T>? itemDependencies) && itemDependencies.Contains(dependency);
        }

        public IEnumerable<T> GetDependencies(T item)
        {
            if (!dependencies.TryGetValue(item, out HashSet<T>? itemDependencies))
            {
                throw new KeyNotFoundException($"The item '{item}' does not exist in the DependencyGraph.");
            }

            return itemDependencies;
        }

        public IEnumerable<T> GetDependents(T item)
        {
            if (!dependents.TryGetValue(item, out HashSet<T>? itemDependents))
            {
                throw new KeyNotFoundException($"The item '{item}' does not exist in the DependencyGraph.");
            }

            return itemDependents;
        }

        public List<List<T>> CreateTiers(Comparison<T>? comparison = null)
        {
            Dictionary<T, int> dependencyCounts = [];

            foreach (T node in Nodes)
            {
                dependencyCounts[node] = dependencies[node].Count;
            }

            Queue<T> available = [];
            foreach (KeyValuePair<T, int> node in dependencyCounts)
            {
                if (node.Value == 0)
                {
                    available.Enqueue(node.Key);
                }
            }

            List<List<T>> tiers = [];
            int processed = 0;

            while (available.Count > 0)
            {
                int tierCount = available.Count;
                List<T> tier = new(tierCount);

                for (int index = 0; index < tierCount; index++)
                {
                    T node = available.Dequeue();
                    tier.Add(node);
                    processed++;

                    foreach (T dependent in dependents[node])
                    {
                        dependencyCounts[dependent]--;

                        if (dependencyCounts[dependent] == 0)
                        {
                            available.Enqueue(dependent);
                        }
                    }
                }

                if (comparison is not null)
                {
                    tier.Sort(comparison);
                }

                tiers.Add(tier);
            }

            if (processed != Count)
            {
                List<T>? cycle = FindCircularDependency();

                if (cycle is not null)
                {
                    throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", cycle)}");

                }

                throw new InvalidOperationException($"Circular dependency detected, but unable to determine the cycle.");
            }

            return tiers;
        }


        private List<T>? FindCircularDependency()
        {
            HashSet<T> visited = [];
            HashSet<T> visiting = [];
            List<T> path = [];
            Dictionary<T, int> pathIndices = [];

            foreach (T item in Nodes)
            {
                if (FindCircularDependency(item, visited, visiting, path, pathIndices, out List<T>? cycle))
                {
                    return cycle;
                }
            }

            return null;
        }

        private bool FindCircularDependency(T item, HashSet<T> visited, HashSet<T> visiting, List<T> path, Dictionary<T, int> pathIndices, out List<T>? cycle)
        {
            cycle = null;

            if (visiting.Contains(item))
            {
                int cycleStart = pathIndices[item];

                cycle = path.GetRange(cycleStart, path.Count - cycleStart);
                cycle.Add(item);

                return true;
            }

            if (visited.Contains(item))
            {
                return false;
            }

            visiting.Add(item);
            pathIndices[item] = path.Count;
            path.Add(item);

            foreach (T dependency in dependencies[item])
            {
                if (FindCircularDependency(dependency, visited, visiting, path, pathIndices, out cycle))
                {
                    return true;
                }
            }

            path.RemoveAt(path.Count - 1);
            pathIndices.Remove(item);
            visiting.Remove(item);
            visited.Add(item);

            return false;
        }

        public void Clear()
        {
            dependencies.Clear();
            dependents.Clear();
        }
    }
}