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
    public abstract class QueryExecutorBase : IQueryExecutor
    {
        private Database database;

        public void Initialize(Database database)
        {
            this.database = database;
        }

        public object Compile(Expression expression)
        {
            expression = this.TransformExpression(expression);

            return this.CompileAndInvoke(expression);
        }


        protected virtual Expression PreprocessExpression(Expression expression, ProcessState state)
        {
            return expression;
        }

        protected virtual IEnumerable<IExpressionModifier> GetModifiers(Expression expression)
        {
            yield break;
        }
        
        protected virtual Expression PostprocessExpression(Expression expression)
        {
            return expression;
        }

        private Expression TransformExpression(Expression expression)
        {
            ProcessState state = new ProcessState();

            expression = this.PreprocessExpression(expression, state);

            if (state.IsFinished)
            {
                return expression;
            }

            // Collect expressions
            IEnumerable<IExpressionModifier> modifiers = this.GetModifiers(expression);
            // Add essential modifiers
            modifiers = modifiers.Concat(new IExpressionModifier[] { new TableModifier() });

            // Initialize databaseComponent modifiers
            foreach (IDatabaseComponent databaseComponent in modifiers)
            {
                databaseComponent.Initialize(this.database);
            }

            // Execute modifiers
            foreach (IExpressionModifier modifier in modifiers)
            {
                expression = modifier.ModifyExpression(expression);
            }

            expression = this.PostprocessExpression(expression);

            return expression;
        }

        private object CompileAndInvoke(Expression expression)
        {
            if (expression is MethodCallExpression)
            {
                Type returnType = (expression as MethodCallExpression).Method.ReturnType;
                Type funcType = typeof(Func<>).MakeGenericType(returnType);

                return Expression.Lambda(funcType, expression, null).Compile().DynamicInvoke(new object[0]);
            }
            else if (expression is ConstantExpression)
            {
                return (expression as ConstantExpression).Value;
            }
            else if (expression is LambdaExpression)
            {
                return (expression as LambdaExpression).Compile().DynamicInvoke(new object[0]);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public abstract IQueryEnumeratorFactory CreateQueryEnumeratorFactory();
    }
}
