using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Modularity;
using System.Linq.Expressions;
using NMemory.Diagnostics;

namespace NMemory.Execution.Optimization.Modifiers
{
    public abstract class ExpressionModifierBase : ExpressionVisitor, IExpressionModifier, IDatabaseComponent
    {
        private Database database;

        public void Initialize(Database database)
        {
            this.database = database;
        }

        public Expression ModifyExpression(Expression expression)
        {
            return this.Visit(expression);
        }

        protected ILoggingPort LoggingPort
        {
            get { return this.database.LoggingPort; }
        }
    }
}
