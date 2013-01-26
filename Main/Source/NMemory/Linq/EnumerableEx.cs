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
    using NMemory.Indexes;
    using System;
    using System.Collections.Generic;

    public static class EnumerableEx
    {
        public static IEnumerable<TResult> JoinIndexed<TOuter, TOuterKey, TInner, TInnerKey, TResult>(
            IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuterKey, TInnerKey> keyToIndexKey,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)

            where TInner : class
        {
            // TODO: Add validation

            return JoinIndexedCore<TOuter, TOuterKey, TInner, TInnerKey, TResult>(
                outer,
                inner,
                keyToIndexKey,
                outerKeySelector,
                resultSelector);
        }

        public static IEnumerable<TResult> GroupJoinIndexed<TOuter, TOuterKey, TInner, TInnerKey, TResult>(
            IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuterKey, TInnerKey> keyToIndexKey,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)

            where TInner : class
        {
            // TODO: Add validation

            return GroupJoinIndexedCore<TOuter, TOuterKey, TInner, TInnerKey, TResult>(
                outer,
                inner,
                keyToIndexKey,
                outerKeySelector,
                resultSelector);
        }

        private static IEnumerable<TResult> JoinIndexedCore<TOuter, TOuterKey, TInner, TInnerKey, TResult>(
            IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuterKey, TInnerKey> keyToIndexKey,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
            where TInner : class
        {
            foreach (TOuter outerItem in outer)
            {
                TOuterKey outerKey = outerKeySelector.Invoke(outerItem);
                TInnerKey key = keyToIndexKey.Invoke(outerKey);

                foreach (TInner innerItem in inner.Select(key))
                {
                    TResult result = resultSelector.Invoke(outerItem, innerItem);

                    yield return result;
                }
            }
        }

        private static IEnumerable<TResult> GroupJoinIndexedCore<TOuter, TOuterKey, TInner, TInnerKey, TResult>(
            IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuterKey, TInnerKey> keyToIndexKey,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
            where TInner : class
        {
            foreach (TOuter outerItem in outer)
            {
                TOuterKey outerKey = outerKeySelector.Invoke(outerItem);
                TInnerKey key = keyToIndexKey.Invoke(outerKey);
                
                IEnumerable<TInner> innerItems = inner.Select(key);
                TResult result = resultSelector.Invoke(outerItem, innerItems);

                yield return result;
            }
        }
    }
}
