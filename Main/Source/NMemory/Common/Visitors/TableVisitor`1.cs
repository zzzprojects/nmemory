using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.Tables;

namespace NMemory.Common.Visitors
{
    public class TableVisitor<T> : ExpressionVisitor
    {
        private ITable table;

        internal ITable<T> SearchTable(Expression expr)
        {
            Visit(expr);

            return this.table as ITable<T>;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            ITable currentTable = c.Value as ITable;

            if (currentTable != null && currentTable.EntityType == typeof(T))
            {
                this.table = currentTable;
            }

            return base.VisitConstant(c);
        }
    }
}
