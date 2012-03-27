using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using NMemory.Tables;

namespace NMemory.Statistics
{
    public class Histogram<TEntity> : IStatistics
    {
        #region Members

        private Dictionary<MemberInfo, IColumnData> Data;
        private ITable<TEntity> Table;

        #endregion

        public Histogram(ITable<TEntity> table)
        {
            this.Data = new Dictionary<MemberInfo, IColumnData>();
            this.Table = table;
        }

        /// <summary>
        /// Állandó szélességű statisztika gyártása
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TColumn"></typeparam>
        /// <param name="table"></param>
        /// <param name="propertySelectorExp"></param>
        /// <param name="bucketSelector"></param>
        /// <param name="bucketSize"></param>
        public void CreateFixedWidthStats<TColumn>(Expression<Func<TEntity, TColumn>> propertySelectorExp,
            Func<TColumn, int> bucketSelector, long bucketSize)
        {
            MemberExpression me = propertySelectorExp.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("propertySelectorExp should be a member selector lambda expression.");
            }

            MemberInfo mi = me.Member;
            Func<TEntity, TColumn> propertySelector = propertySelectorExp.Compile();

            FixedWidthBuckets<TColumn> cd = new FixedWidthBuckets<TColumn>(bucketSelector, bucketSize);
            cd.Analyse(Table, propertySelector);
            this.Data[mi] = cd;
        }

        /// <summary>
        /// Állandó magasságú statisztika gyártása
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TColumn"></typeparam>
        /// <param name="table"></param>
        /// <param name="propertySelectorExp"></param>
        /// <param name="bucketSize"></param>
        public void CreateFixedHeightStats<TColumn>(Expression<Func<TEntity, TColumn>> propertySelectorExp,
            long bucketSize)
            where TColumn : IConvertible
        {
            MemberExpression me = propertySelectorExp.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("propertySelectorExp should be a member selector lambda expression.");
            }

            MemberInfo mi = me.Member;
            Func<TEntity, TColumn> propertySelector = propertySelectorExp.Compile();

            FixedHeightBuckets<TColumn> cd = new FixedHeightBuckets<TColumn>(bucketSize);
            cd.Analyse(Table, propertySelector);
            this.Data[mi] = cd;
        }

        #region IStatistics Members


        public long? Values(MemberInfo member)
        {
            IColumnData data;

            if (this.Data.TryGetValue(member, out data) == true)
            {
                return data.Values;
            }
            else
            {
                return null;
            }
        }

        public long Count
        {
            get
            {
                if (this.Data.Count == 0)
                {
                    throw new InvalidOperationException();
                }

                return this.Data.Values.First().Count;
            }
        }

        public long? ValueCount(MemberInfo member, object value)
        {
            IColumnData data;

            if (this.Data.TryGetValue(member, out data) == true)
            {
                return data.ValueCount(value);
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
