using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;
using System.Linq.Expressions;

namespace NMemory.Linq.Helpers
{
    public class IndexedQueryable<T> : IIndexedQueryable<T>
    {
        private IIndex index;
        private IQueryable<T> query;

        public IndexedQueryable(IQueryable<T> query, IIndex index)
        {
            this.index = index;
            this.query = query;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return this.query.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.query.GetEnumerator();
        }

        public Type ElementType
        {
            get { return this.query.ElementType; }
        }

        public Expression Expression
        {
            get { return this.query.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return this.query.Provider; }
        }

        public IIndex Index
        {
            get { return this.index; }
        }
    }
}
