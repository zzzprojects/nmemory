using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Exceptions;
using NMemory.Tables;
using System;

namespace NMemory.Common.Visitors
{
    internal class TableSearchVisitor : ExpressionVisitor
    {
        public static IList<ITable> FindTables(Expression expression)
        {
            TableSearchVisitor visitor = new TableSearchVisitor() { AllowMultiple = true };
            return visitor.SearchTables(expression);
        }

        public static IList<Type> FindEntityTypes(Expression expression)
        {
            return FindTables(expression).Select(t => t.EntityType).ToList();
        }

        private List<ITable> tables;

        public TableSearchVisitor()
        {
            this.tables = new List<ITable>();
        }

        public bool AllowMultiple { get; set; }

        public ITable SearchTable(Expression expression)
        {
            this.tables.Clear();

            this.Visit(expression);

            return this.tables.FirstOrDefault();
        }

        public IList<ITable> SearchTables(Expression expression)
        {
            this.tables.Clear();

            this.Visit(expression);

            return this.tables.AsReadOnly();
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            ITable table = constant.Value as ITable;

            if (table != null && !tables.Contains(table))
            {
                if (!this.AllowMultiple && tables.Count > 0)
                {
                    throw new NMemoryException(ErrorCodes.GenericError, "Only one table per query supported.");
                }
                
                this.tables.Add(table);
            }

            return base.VisitConstant(constant);
        }
    }
}
