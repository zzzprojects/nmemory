using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Execution
{
    public interface IExecutionPlan<T> : IExecutionPlan
    {
        T Execute(IExecutionContext context);
    }
}
