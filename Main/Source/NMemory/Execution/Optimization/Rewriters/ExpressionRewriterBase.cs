using System.Linq.Expressions;
using NMemory.Modularity;
using System.Collections.Generic;

namespace NMemory.Execution.Optimization.Rewriters
{
    public abstract class ExpressionRewriterBase : ExpressionVisitor, IExpressionRewriter, IDatabaseComponent
    {
        private IDatabase database;

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }

        public Expression Rewrite(Expression expression)
        {
            return this.Visit(expression);
        }

        protected ILoggingPort LoggingPort
        {
            get { return this.database.DatabaseEngine.LoggingPort; }
        }

        protected virtual IList<Expression> VisitExpressions(IList<Expression> original)
        {
            List<Expression> sequence = null;
            int num = 0;
            int count = original.Count;

            for (int i = 0; i < original.Count; i++)
            {
                Expression item = this.Visit(original[i]);

                if (sequence != null)
                {
                    sequence.Add(item);
                }
                else if (item != original[num])
                {
                    sequence = new List<Expression>(count);

                    for (int j = 0; j < i; j++)
                    {
                        sequence.Add(original[j]);
                    }

                    sequence.Add(item);
                }
            }

            if (sequence != null)
            {
                return sequence;
            }

            return original;
        }
    }
}
