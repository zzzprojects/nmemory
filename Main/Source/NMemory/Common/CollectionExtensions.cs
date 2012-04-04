using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NMemory.Common
{
    internal static class CollectionExtensions
    {
        public static IList<T> AsReadOnly<T>(this IList<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            return new ReadOnlyCollection<T>(collection);
        }


        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> input)
        {
            try
            {
                while (input.MoveNext())
                {
                    yield return input.Current;
                }
            }
            finally
            {
                input.Dispose();
            }
        }
    }
}
