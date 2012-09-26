using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Execution.Optimization;
using NMemory.Execution.Optimization.Rewriters;

namespace NMemory.Execution
{
    public class QueryCompiler : QueryCompilerBase
    {
        protected override IEnumerable<IExpressionRewriter> GetRewriters(Expression expression, TransformationContext context)
        {
            return base.GetRewriters(expression, context)
                .Concat(GetCustomRewriters(expression, context));
        }

        private IEnumerable<IExpressionRewriter> GetCustomRewriters(Expression expression, TransformationContext context)
        {
            yield return new PropertyAccessRewriter();
            yield break;
        }

        public bool EnableCompilationCaching { get; set; }

        public bool EnableOptimization { get; set; }

    }
}
