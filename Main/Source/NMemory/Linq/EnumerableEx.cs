// ----------------------------------------------------------------------------------
// <copyright file="EnumerableEx.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NMemory.Indexes;

    public static class EnumerableEx
    {
        public static IEnumerable<TResult> JoinIndexed<TOuter, TInner, TOuterKey, TInnerKey, TResult>(
            this IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuterKey, bool> isEmpty,
            Func<TOuterKey, TInnerKey> outerKeyToIndexKey,
            Func<TOuter, TInner, TResult> resultSelector)

            where TInner : class
        {
            if (outer == null)
            {
                throw new ArgumentNullException("outer");
            }

            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }

            if (outerKeySelector == null)
            {
                throw new ArgumentNullException("outerKeySelector");
            }

            if (isEmpty == null)
            {
                throw new ArgumentNullException("isEmpty");
            }

            if (outerKeyToIndexKey == null)
            {
                throw new ArgumentNullException("keyToIndexKey");
            }

            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            return JoinIndexedCore<TOuter, TInner, TOuterKey, TInnerKey, TResult>(
                outer,
                inner,
                outerKeySelector,
                isEmpty,
                outerKeyToIndexKey,
                resultSelector);
        }

        public static IEnumerable<TResult> GroupJoinIndexed<TOuter, TInner, TOuterKey, TInnerKey, TResult>(
            this IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuterKey, bool> isEmpty,
            Func<TOuterKey, TInnerKey> outerKeyToIndexKey,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)

            where TInner : class
        {
            if (outer == null)
            {
                throw new ArgumentNullException("outer");
            }

            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }

            if (outerKeySelector == null)
            {
                throw new ArgumentNullException("outerKeySelector");
            }

            if (isEmpty == null)
            {
                throw new ArgumentNullException("isEmpty");
            }

            if (outerKeyToIndexKey == null)
            {
                throw new ArgumentNullException("keyToIndexKey");
            }

            if (resultSelector == null)
            {
                throw new ArgumentNullException("resultSelector");
            }

            return GroupJoinIndexedCore<TOuter, TInner, TOuterKey, TInnerKey, TResult>(
                outer,
                inner,
                outerKeySelector,
                isEmpty,
                outerKeyToIndexKey,
                resultSelector);
        }

        private static IEnumerable<TResult> JoinIndexedCore<TOuter, TInner, TOuterKey, TInnerKey, TResult>(
            IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuterKey, bool> isEmpty,
            Func<TOuterKey, TInnerKey> outerKeyToIndexKey,
            Func<TOuter, TInner, TResult> resultSelector)
            where TInner : class
        {
            foreach (TOuter outerItem in outer)
            {
                TOuterKey outerKey = outerKeySelector.Invoke(outerItem);
                
                if (isEmpty.Invoke(outerKey))
                {
                    continue;
                }

                TInnerKey key = outerKeyToIndexKey.Invoke(outerKey);

                foreach (TInner innerItem in inner.Select(key))
                {
                    TResult result = resultSelector.Invoke(outerItem, innerItem);

                    yield return result;
                }
            }
        }

        private static IEnumerable<TResult> GroupJoinIndexedCore<TOuter, TInner, TOuterKey, TInnerKey, TResult>(
            IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuterKey, bool> isEmpty,
            Func<TOuterKey, TInnerKey> outerKeyToIndexKey,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
            where TInner : class
        {
            foreach (TOuter outerItem in outer)
            {
                TOuterKey outerKey = outerKeySelector.Invoke(outerItem);

                IEnumerable<TInner> innerItems = null;
                
                if (!isEmpty.Invoke(outerKey))
                {
                    TInnerKey key = outerKeyToIndexKey.Invoke(outerKey);
                    innerItems = inner.Select(key);
                }
                else
                {
                    innerItems = Enumerable.Empty<TInner>();
                }

                yield return resultSelector.Invoke(outerItem, innerItems);
            }
        }
    }
}
