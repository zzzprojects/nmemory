using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Execution
{
    public interface IExecutionPlan
    {
        Expression Plan { get; }
    }
}
