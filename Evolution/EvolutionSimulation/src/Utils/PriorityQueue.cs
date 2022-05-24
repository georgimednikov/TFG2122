using System.Collections;
using System.Collections.Generic;
using System;

namespace EvolutionSimulation.Utils
{
    public class PriorityQueue<T> where T : IComparable<T>, IEquatable<T>
    {
        private List<T> items = new List<T>();
        public int Count { get { return items.Count; } }
        public void Clear() { items.Clear(); }
        public void Insert(T item)
        {
            int i = items.Count;
            items.Add(item);
            while (i > 0 && items[(i - 1) / 2].CompareTo(item) > 0)
            {
                items[i] = items[(i - 1) / 2];
                i = (i - 1) / 2;
            }
            items[i] = item;
        }

        public T Top() { return items[0]; }
        public bool Contains(T item) { return items.Contains(item); }
        public T RemoveTop()
        {
            T firstItem = items[0];
            T tempItem = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            if (items.Count > 0)
            {
                int i = 0;
                while (i < items.Count / 2)
                {
                    int j = (2 * i) + 1;
                    if ((j < items.Count - 1) && (items[j].CompareTo(items[j + 1]) > 0)) ++j;
                    if (items[j].CompareTo(tempItem) >= 0) break;
                    items[i] = items[j];
                    i = j;
                }
                items[i] = tempItem;
            }
            return firstItem;
        }
    }
}