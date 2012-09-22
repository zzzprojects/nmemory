using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Execution
{
    public class ExecutionPlan<T> : IExecutionPlan<T>
    {
        private Func<IExecutionContext, T> executable;
        private Expression plan;

        public ExecutionPlan(Func<IExecutionContext, T> executable, Expression plan)
        {
            this.executable = executable;
            this.plan = plan;
        }

        public T Execute(IExecutionContext context)
        {
            return this.executable.Invoke(context);
        }

        public Expression Plan
        {
            get { return this.plan; }
        }
    }
}
