// ----------------------------------------------------------------------------------
// <copyright file="TableQuery.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Modularity;

    public abstract class TableQuery : IQueryable, ITableQuery
    {
        private Expression expression;
        private IDatabase database;
        private IQueryProvider provider;

        #region Ctor

        internal TableQuery(IDatabase database, Expression expression, IQueryProvider provider)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            this.database = database;
            this.expression = expression;
            this.provider = provider;
        }

        internal TableQuery(IDatabase database, Expression expression)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this.database = database;
            this.expression = expression;
            this.provider = new TableQueryProvider(database);
        }

        internal TableQuery(IDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            this.database = database;
            this.expression = Expression.Constant(this);
            this.provider = new TableQueryProvider(database);
        }

        #endregion

        public IDatabase Database
        {
            get { return this.database; }
        }

        public abstract Type EntityType
        {
            get;
        }

        Type IQueryable.ElementType
        {
            get { return this.EntityType; }
        }

        Expression IQueryable.Expression
        {
            get { return this.expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this.provider; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }
    }
}
