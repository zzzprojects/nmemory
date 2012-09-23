using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Execution.Optimization;
using NMemory.Execution.Optimization.Modifiers;

namespace NMemory.Execution
{
    public class QueryCompiler : QueryCompilerBase
    {
        protected override IEnumerable<IExpressionModifier> GetModifiers(Expression expression, TransformationContext context)
        {
            return base.GetModifiers(expression, context)
                .Concat(GetCustomModifiers(expression, context));
        }

        private IEnumerable<IExpressionModifier> GetCustomModifiers(Expression expression, TransformationContext context)
        {
            yield return new PropertyAccessModifier();
            yield break;
        }

        public bool EnableCompilationCaching { get; set; }

        public bool EnableOptimization { get; set; }

    }
}
