using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common.Visitors;
using NMemory.Indexes;
using NMemory.Linq.Helpers;
using NMemory.Tables;

namespace NMemory.Linq
{
	public static class QueryableEx
	{
		#region Update

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
		public static int Update<T>(this IQueryable<T> queryable, Expression<Func<T, T>> updater)
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

            IBatchTable<T> table = query.Database.Tables.FindTable<T>() as IBatchTable<T>;

            if (table == null)
            {
                throw new ArgumentException("Update query must result in table entities", "queryable");
            }

            return table.Update(query, updater);
		}

 

		#endregion

		#region Delete

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <returns></returns>
		public static int Delete<T>( this IQueryable<T> queryable )
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

            IBatchTable<T> table = query.Database.Tables.FindTable<T>() as IBatchTable<T>;

            if (table == null)
            {
                throw new ArgumentException("Delete query must result in table entities", "queryable");
            }

            return table.Delete(query);
		}

		#endregion

		#region JoinIndexed

		/// <summary>
		/// A Join operátor belső implementációja, amely indexeken éri el a belső tábla elemeit.
		/// </summary>
		internal static IEnumerable<TResult> JoinIndexedCore<TOuter, TInner, TInnerKey, TKey, TResult>(
		    IQueryable<TOuter> outer,
		    ConstantExpression inner,
		    Expression<Func<TKey, TInnerKey>> keyToIndexKey,
		    Expression outerKeySelector,
		    Expression resultSelector)
		//where TInner : class
		{

			Func<TKey, TInnerKey> keyToIndexKexFunc = keyToIndexKey.Compile();
			Func<TOuter, TKey> outerKeySelectorFunc = ( outerKeySelector as Expression<Func<TOuter, TKey>> ).Compile();
			Func<TOuter, TInner, TResult> resultSelectorFunc = ( resultSelector as Expression<Func<TOuter, TInner, TResult>> ).Compile();
			IIndex<TInner, TInnerKey> index = (IIndex<TInner, TInnerKey>)inner.Value;

#if MEASURE
			Stopwatch s = new Stopwatch();
			Stopwatch keys = new Stopwatch();
			Stopwatch results = new Stopwatch();
			s.Start();
#endif

			foreach( TOuter outer_item in outer )
			{
#if MEASURE
				keys.Start();
#endif
				TInnerKey key = keyToIndexKexFunc( outerKeySelectorFunc( outer_item ) );
#if MEASURE
				keys.Stop();
#endif

				foreach( TInner inner_item in index.Select( key ) )
				{
#if MEASURE
					results.Start();
#endif
					TResult result = resultSelectorFunc( outer_item, inner_item );
#if MEASURE
					results.Stop();
#endif

					yield return result;
				}
			}

#if MEASURE
			s.Stop();

			Console.WriteLine("JoinIndexedCore/keyek előtállítása: {0} ms", keys.ElapsedMilliseconds);
			Console.WriteLine("JoinIndexedCore/result előtállítása: {0} ms", results.ElapsedMilliseconds);
			Console.WriteLine("JoinIndexedCore: {0} ms", s.ElapsedMilliseconds);
#endif
		}

		/// <summary>
		/// Join operátor, amely indexeken éri el a belső reláció elemeit.
		/// </summary>
		public static IQueryable<TResult> JoinIndexed<TOuter, TInner, TInnerKey, TKey, TResult>(
																				this IQueryable<TOuter> outer,
																				ConstantExpression inner,
																				Expression<Func<TKey, TInnerKey>> keyToIndexKey,
																				Expression outerKeySelector,
																				Expression resultSelector
																			)
		//where TInner : class
		{

			IEnumerable<TResult> res = JoinIndexedCore<TOuter, TInner, TInnerKey, TKey, TResult>( outer, inner, keyToIndexKey, outerKeySelector, resultSelector );

			return res.AsQueryable();
		}

		#endregion

		#region JoinIndexed

		/// <summary>
		/// A Join operátor belső implementációja, amely indexeken éri el a belső tábla elemeit.
		/// </summary>
		internal static IEnumerable<TResult> JoinIndexedCore<TOuter, TInner, TInnerKey, TKey, TResult>(
			IQueryable<TOuter> outer,
			IIndex<TInner, TInnerKey> inner,
			Func<TKey, TInnerKey> keyToIndexKey,
			Func<TOuter, TKey> outerKeySelector,
			Func<TOuter, TInner, TResult> resultSelector)
		{

			Func<TKey, TInnerKey> keyToIndexKexFunc = keyToIndexKey;
			Func<TOuter, TKey> outerKeySelectorFunc = outerKeySelector;
			Func<TOuter, TInner, TResult> resultSelectorFunc = resultSelector;
			IIndex<TInner, TInnerKey> index = inner;

#if MEASURE
			Stopwatch s = new Stopwatch();
			Stopwatch keys = new Stopwatch();
			Stopwatch results = new Stopwatch();
			s.Start();
#endif

			foreach( TOuter outer_item in outer )
			{
#if MEASURE
				keys.Start();
#endif
				TInnerKey key = keyToIndexKexFunc( outerKeySelectorFunc( outer_item ) );
#if MEASURE
				keys.Stop();
#endif

				foreach( TInner inner_item in index.Select( key ) )
				{
#if MEASURE
					results.Start();
#endif
					TResult result = resultSelectorFunc( outer_item, inner_item );
#if MEASURE
					results.Stop();
#endif

					yield return result;
				}
			}

#if MEASURE
			s.Stop();

			Console.WriteLine("JoinIndexedCore/keyek előtállítása: {0} ms", keys.ElapsedMilliseconds);
			Console.WriteLine("JoinIndexedCore/result előtállítása: {0} ms", results.ElapsedMilliseconds);
			Console.WriteLine("JoinIndexedCore: {0} ms", s.ElapsedMilliseconds);
#endif
		}

		/// <summary>
		/// Join operátor, amely indexeken éri el a belső reláció elemeit.
		/// </summary>
		public static IQueryable<TResult> JoinIndexed<TOuter, TInner, TInnerKey, TKey, TResult>(

																						IQueryable<TOuter> outer,
																						IIndex<TInner, TInnerKey> inner,
																						Func<TKey, TInnerKey> keyToIndexKey,
																						Func<TOuter, TKey> outerKeySelector,
																						Func<TOuter, TInner, TResult> resultSelector

																			)
		{
			IEnumerable<TResult> res = JoinIndexedCore<TOuter, TInner, TInnerKey, TKey, TResult>( outer, inner, keyToIndexKey, outerKeySelector, resultSelector );

			return res.AsQueryable();
		}

		#endregion

		#region MergeJoin

		/// <summary>
		/// Összefésüléses összekapcsolás belső implementációja
		/// </summary>
		private static IEnumerable<TResult> MergeJoinCore<TOuter, TInner, TKey, TResult>(
			this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner,
			Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector,
			Func<TOuter, TInner, TResult> resultSelector )
			where TKey : IComparable
		{
			IEnumerator<TOuter> i = outer.GetEnumerator();
			IEnumerator<TInner> j = inner.GetEnumerator();
			i.MoveNext();
			j.MoveNext();
			List<TOuter> iElements = new List<TOuter>();
			List<TInner> jElements = new List<TInner>();
			TKey ikey;
			TKey jkey;
			TKey iGroupKey = default( TKey );
			TKey jGroupKey = default( TKey );
			bool iValid = true;
			bool jValid = true;

			do
			{
				if( iElements.Count == 0 )
				{
					iElements.Add( i.Current );
					iGroupKey = outerKeySelector( i.Current );

					do
					{
						iValid = i.MoveNext();

                        if (iValid == false)
                        {
                            break;
                        }

						ikey = outerKeySelector( i.Current );

                        if (iGroupKey.Equals(ikey))
                        {
                            iElements.Add(i.Current);
                        }
                        else
                        {
                            break;
                        }

					} 
                    while( true );
				}

				if( jElements.Count == 0 )
				{
					jElements.Add( j.Current );
					jGroupKey = innerKeySelector( j.Current );

					do
					{
						jValid = j.MoveNext();

                        if (jValid == false)
                        {
                            break;
                        }

						jkey = innerKeySelector( j.Current );

                        if (jGroupKey.Equals(jkey))
                        {
                            jElements.Add(j.Current);
                        }
                        else
                        {
                            break;
                        }
					} 
                    while( true );
				}

                if (iGroupKey.Equals(jGroupKey))
                {
                    foreach (var ii in iElements)
                    {
                        foreach (var jj in jElements)
                        {
                            yield return resultSelector(ii, jj);
                        }
                    }
                }

				int comp = iGroupKey.CompareTo(jGroupKey);

                if (comp <= 0)
                {
                    iElements.Clear();
                }

                if (comp >= 0)
                {
                    jElements.Clear();
                }
			} while( iValid && jValid );
		}

		/// <summary>
		/// Az összefésüléses összekapcsolás rendezett táblákon
		/// </summary>
		public static IQueryable<TResult> MergeJoin<TOuter, TInner, TKey, TResult>(
			this IEnumerable<TOuter> outer,
			IEnumerable<TInner> inner,
			Func<TOuter, TKey> outerKeySelector,
			Func<TInner, TKey> innerKeySelector,
			Func<TOuter, TInner, TResult> resultSelector )
			where TKey : IComparable
		{
			return QueryableEx.MergeJoinCore( outer, inner, outerKeySelector, innerKeySelector, resultSelector ).AsQueryable();
		}
		#endregion

        public static IQueryable<TResult> LeftOuterJoin<TOuter, TInner, TKey, TResult>(IQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            var joined = outer.GroupJoin(inner, outerKeySelector, innerKeySelector,
                                          (o, i) => new Tuple<TOuter, IEnumerable<TInner>>(o, i));

            Type tupleType = joined.GetType().GetGenericArguments().First();

            ReplaceVisitor rv;

            ParameterExpression oldResultParam1 = resultSelector.Parameters[0];
            ParameterExpression oldResultParam2 = resultSelector.Parameters[1];

            Expression newResultSelectorBody = resultSelector.Body;

            ParameterExpression anonParam = Expression.Parameter(tupleType);
            ParameterExpression innerParam = Expression.Parameter(typeof(TInner));

            Expression oSelector = Expression.Property(anonParam, tupleType.GetProperty("Item1"));

            rv = new ReplaceVisitor(exp => exp == oldResultParam1, exp => oSelector);
            newResultSelectorBody = rv.Visit(newResultSelectorBody);
            rv = new ReplaceVisitor(exp => exp == oldResultParam2, exp => innerParam);
            newResultSelectorBody = rv.Visit(newResultSelectorBody);

            LambdaExpression newResultSelector = Expression.Lambda(newResultSelectorBody, anonParam, innerParam);

            var res = joined.SelectMany(o => o.Item2.DefaultIfEmpty(), newResultSelector as Expression<Func<Tuple<TOuter, IEnumerable<TInner>>, TInner, TResult>>);

            return res;
        }


        public static int Count<T>(IQueryable<T> source)
        {
            if (source is ITable<T>)
            {
                return (int)(source as ITable<T>).Count;
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
