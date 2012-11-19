// ----------------------------------------------------------------------------------
// <copyright file="QueryableEx.cs" company="NMemory Team">
//     Copyright (C) 2012 NMemory Team
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
    using System.Linq.Expressions;
    using NMemory.Common.Visitors;
    using NMemory.Indexes;
    using NMemory.Linq.Helpers;
    using NMemory.Tables;
    using NMemory.Transactions;
    using NMemory.Utilities;

    public static class QueryableEx
    {
        #region Update

        public static IEnumerable<T> Update<T>(this IQueryable<T> queryable, Expression<Func<T, T>> updater)
            where T : class
        {
            return Update<T>(queryable, updater, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public static IEnumerable<T> Update<T>(this IQueryable<T> queryable, Expression<Func<T, T>> updater, Transaction transaction)
            where T : class
        {
            if (queryable == null)
            {
                throw new ArgumentNullException("queryable");
            }

            TableQuery<T> query = queryable as TableQuery<T>;

            if (query == null)
            {
                throw new ArgumentException("Delete command can be executed only on NMemory queries.", "queryable");
            }

            IBulkTable<T> table = query.Database.Tables.FindTable<T>() as IBulkTable<T>;

            if (table == null)
            {
                throw new ArgumentException("The database associated with the command does contain the appropriate table.", "queryable");
            }

            return table.Update(query, updater, transaction);
        }

        #endregion

        #region Delete

        public static int Delete<T>(this IQueryable<T> queryable)
            where T : class
        {
            return Delete<T>(queryable, Transaction.TryGetAmbientEnlistedTransaction());
        }
        
        public static int Delete<T>(this IQueryable<T> queryable, Transaction transaction)
            where T : class
        {
            if (queryable == null)
            {
                throw new ArgumentNullException("queryable");
            }

            TableQuery<T> query = queryable as TableQuery<T>;

            if (query == null)
            {
                throw new ArgumentException("Delete command can be executed only on NMemory queries.", "queryable");
            }

            IBulkTable<T> table = query.Database.Tables.FindTable<T>() as IBulkTable<T>;

            if (table == null)
            {
                throw new ArgumentException("The database associated with the command does contain the appropriate table.", "queryable");
            }

            return table.Delete(query, transaction);
        }

        #endregion

        public static IEnumerable<T> Execute<T>(this IQueryable<T> queryable)
        {
            return Execute<T>(queryable, null, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public static IEnumerable<T> Execute<T>(this IQueryable<T> queryable, Transaction transaction)
        {
            return Execute<T>(queryable, null, transaction);
        }

        public static IEnumerable<T> Execute<T>(this IQueryable<T> queryable, IDictionary<string, object> parameters)
        {
            return Execute<T>(queryable, parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public static IEnumerable<T> Execute<T>(this IQueryable<T> queryable, IDictionary<string, object> parameters, Transaction transaction)
        {
            if (queryable == null)
            {
                throw new ArgumentNullException("queryable");
            }

            TableQuery<T> query = queryable as TableQuery<T>;

            if (query == null)
            {
                throw new ArgumentException("Execute command can be executed only on NMemory queries.", "queryable");
            }

            return new TableQueryWrapper<T>(query, parameters, transaction);
        }

        #region JoinIndexed

        internal static IEnumerable<TResult> JoinIndexedCore<TOuter, TOuterKey, TInner, TInnerKey, TResult>(
            IEnumerable<TOuter> outer,
            IIndex<TInner, TInnerKey> inner,
            Func<TOuterKey, TInnerKey> keyToIndexKey,
            Func<TOuter, TOuterKey> outerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)

            where TInner : class
        {
            foreach(TOuter outerItem in outer)
            {
                TOuterKey outerKey = outerKeySelector.Invoke(outerItem);
                TInnerKey key = keyToIndexKey.Invoke(outerKey);

                foreach(TInner inner_item in inner.Select(key))
                {
                    TResult result = resultSelector.Invoke(outerItem, inner_item);

                    yield return result;
                }
            }
        }

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

        #endregion



        public static int Count<T>(IQueryable<T> source)
        {
            if (source is ITable)
            {
                return (int)(source as ITable).Count;
            }

            if (source is IIndexedQueryable<T>)
            {
                return (int)(source as IIndexedQueryable<T>).Index.Count;
            }

            return source.Count();
        }

        public static IQueryable<T> AsIndexedQueryable<T>(this IEnumerable<T> source, IIndex index)
        {
            return new IndexedQueryable<T>(source.AsQueryable(), index);
        }
    }
}
