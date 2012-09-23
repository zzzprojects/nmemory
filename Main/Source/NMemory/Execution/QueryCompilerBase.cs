using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Execution.Optimization;
using NMemory.Execution.Optimization.Modifiers;
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

        protected virtual IEnumerable<IExpressionModifier> GetModifiers(Expression expression, TransformationContext context)
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
            IEnumerable<IExpressionModifier> modifiers = this.GetModifiers(expression, context);
            // Add essential modifiers
            modifiers =
                new IExpressionModifier[] 
                { 
                    new SharedStoreProcedureDatabaseParameterModifier(context), 
                    new StoredProcedureParameterModifier(context), 
                }
                .Concat(modifiers.Concat(new IExpressionModifier[] 
                { 
                    new TableModifier() 
                }));

            // Initialize modifiers
            foreach (IDatabaseComponent databaseComponent in modifiers)
            {
                databaseComponent.Initialize(this.database);
            }

            // Execute modifiers
            foreach (IExpressionModifier modifier in modifiers)
            {
                expression = modifier.ModifyExpression(expression);
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
