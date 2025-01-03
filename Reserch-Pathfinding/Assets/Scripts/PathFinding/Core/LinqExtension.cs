using System;
using System.Collections.Generic;

namespace PathFinder.Core
{
    public static class LinqExtension
    {
        public static IEnumerable<T> MinBy<T, U>(this IEnumerable<T> source, Func<T, U> selector)
        {
            return SelectBy(source, selector, (a, b) => Comparer<U>.Default.Compare(a, b) < 0);
        }

        public static IEnumerable<T> MaxBy<T, U>(this IEnumerable<T> source, Func<T, U> selector)
        {
            return SelectBy(source, selector, (a, b) => Comparer<U>.Default.Compare(a, b) > 0);
        }

        private static IEnumerable<T> SelectBy<T, U>(IEnumerable<T> source, Func<T, U> selector, Func<U, U, bool> comparer)
        {
            var list = new LinkedList<T>();
            U prevKey = default(U);
            foreach (var item in source)
            {
                var key = selector(item);
                if (list.Count == 0 || comparer(key, prevKey))
                {
                    list.Clear();
                    list.AddLast(item);
                    prevKey = key;
                }
                else if (Comparer<U>.Default.Compare(key, prevKey) == 0)
                {
                    list.AddLast(item);
                }
            }

            return list;
        }
    }
}