using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using System.Linq.Expressions;
using NMemory.Exceptions;

namespace NMemory.Common.Visitors
{
    internal class TableVisitor : ExpressionVisitor
    {
        private List<ITable> tables;

        public TableVisitor()
        {
            this.tables = new List<ITable>();
        }

        public bool AllowMultiple { get; set; }
        public bool MultipleFound { get; set; }

        internal ITable SearchTable(Expression expr)
        {
            this.MultipleFound = false;
            this.tables.Clear();

            Visit(expr);

            return this.tables.FirstOrDefault();
        }

        internal ITable[] SearchTables(Expression expr)
        {
            this.MultipleFound = false;
            this.tables.Clear();

            Visit(expr);

            return this.tables.ToArray();
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            if (c.Value is ITable)
            {
                if (tables.Count > 0)
                {
                    if (this.AllowMultiple)
                    {
                        this.MultipleFound = true;
                    }
                    else
                    {
                        throw new NMemoryException(ErrorCodes.GenericError, new Exception("Only one table per query supported."));
                    }
                }
                else
                {
                    this.tables.Add(c as ITable);
                }
            }

            return base.VisitConstant(c);
        }
    }
}
