using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Modularity;
using System.Linq.Expressions;
using NMemory.Execution.Optimization;
using NMemory.Execution.Optimization.Modifiers;

namespace NMemory.Execution
{
    public abstract class QueryCompilerBase : IQueryCompiler
    {
        private IDatabase database;

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }

        public Func<IExecutionContext, T> Compile<T>(Expression expression)
        {
            // Define execution context as parameter
            ParameterExpression parameter = Expression.Parameter(typeof(IExecutionContext));
            
            // Execute optimization
            expression = this.TransformExpression(expression, parameter);

            // Reduce the expression
            while (expression.CanReduce)
            {
                expression = expression.Reduce();
            }

            // Compile query
            return this.CompileCore(expression, parameter) as Func<IExecutionContext, T>;
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
            modifiers = modifiers.Concat(new IExpressionModifier[] { new TableModifier(), new StoredProcedureParameterModifier(parameter) });

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
