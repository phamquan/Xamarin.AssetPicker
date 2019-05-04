using System;
using System.Collections.Generic;

namespace AssetsPicker
{
    public static class CollectionsExtensions
    {
        public static T RemoveAndGet<T>(this IList<T> list, int index)
        {
            lock (list)
            {
                T value = list[index];
                list.RemoveAt(index);
                return value;
            }
        }
    }
}
