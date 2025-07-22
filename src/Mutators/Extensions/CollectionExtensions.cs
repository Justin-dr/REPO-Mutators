using System;
using System.Collections.Generic;

namespace Mutators.Extensions
{
    internal static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T thing in source)
            {
                action(thing);
            }
        }
    }
}