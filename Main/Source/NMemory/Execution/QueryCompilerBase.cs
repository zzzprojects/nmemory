using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Execution.Optimization;
using NMemory.Execution.Optimization.Rewriters;
using NMemory.Modularity;

namespace NMemory.Execution
{
    public abstract class QueryCompilerBase : IQueryCompiler
    {
        private IDatabase database;

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }

        public IExecutionPlan<T> Compile<T>(Expression expression)
        {
            // Define execution context as parameter
            ParameterExpression parameter = Expression.Parameter(typeof(IExecutionContext), "executionContext");
            
            // Execute optimization
            expression = this.TransformExpression(expression, parameter);

            // Reduce the expression
            while (expression.CanReduce)
            {
                expression = expression.Reduce();
            }

            // Compile query
            Func<IExecutionContext, T> executable = this.CompileCore(expression, parameter) as Func<IExecutionContext, T>;

            return new ExecutionPlan<T>(executable, expression);
        }


        protected virtual Expression PreprocessExpression(Expression expression, TransformationContext context)
        {
            return expression;
        }

        protected virtual IEnumerable<IExpressionRewriter> GetRewriters(Expression expression, TransformationContext context)
        {
            yield break;
        }

        protected virtual Expression PostprocessExpression(Expression expression, TransformationContext context)
        {
            return expression;
        }

        private Expression TransformExpression(Expression expression, ParameterExpression parameter)
        {
            TransformationContext context = new TransformationContext(parameter);

            expression = this.PreprocessExpression(expression, context);

            if (context.IsFinished)
            {
                return expression;
            }

            // Collect expressions
            IEnumerable<IExpressionRewriter> rewriters = this.GetRewriters(expression, context);
            
            // Add essential rewriters
            rewriters =
                new IExpressionRewriter[] 
                { 
                    new DatabaseParameterRewriter(context), 
                    new StoredProcedureParameterModifier(context), 
                }
                .Concat(rewriters)
                .Concat(new IExpressionRewriter[] 
                { 
                    new TableScanRewriter(),
                    new QueryableRewriter()
                });

            // Initialize rewriters
            foreach (IDatabaseComponent databaseComponent in rewriters)
            {
                databaseComponent.Initialize(this.database);
            }

            // Execute rewriters
            foreach (IExpressionRewriter rewriter in rewriters)
            {
                expression = rewriter.Rewrite(expression);
            }

            expression = this.PostprocessExpression(expression, context);

            return expression;
        }

        private Delegate CompileCore(Expression expression, ParameterExpression parameter)
        {
            if (expression is MethodCallExpression)
            {
                return Expression.Lambda(expression, parameter).Compile();
            }
            else if (expression is ConstantExpression)
            {
                return Expression.Lambda(expression, parameter).Compile();
            }
            //else if (expression is LambdaExpression)
            //{
            //    return (expression as LambdaExpression).Compile();
            //}
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
