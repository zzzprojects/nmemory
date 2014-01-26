// ----------------------------------------------------------------------------------
// <copyright file="QueryCompilerBase.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Execution.Optimization;
    using NMemory.Execution.Optimization.Rewriters;
    using NMemory.Modularity;

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
            ParameterExpression parameter = 
                Expression.Parameter(typeof(IExecutionContext), "executionContext");

            // Create new transformation context
            ITransformationContext context = new TransformationContext(parameter);
            TransformationStepRecorder recorder = new TransformationStepRecorder();
            
            // Execute optimization
            expression = this.TransformExpression(expression, context, recorder);

            // Compile query
            Func<IExecutionContext, T> executable = 
                this.CompileCore(expression, parameter) as Func<IExecutionContext, T>;

            // Compose result
            IExecutionPlanInfo info = new ExecutionPlanInfo(recorder.Steps);
            IExecutionPlan<T> plan = new ExecutionPlan<T>(executable, info);

            return plan;
        }

        protected virtual Expression PreprocessExpression(
            Expression expression, 
            ITransformationContext context)
        {
            return expression;
        }

        protected virtual IEnumerable<IExpressionRewriter> GetRewriters(
            Expression expression, 
            ITransformationContext context)
        {
            yield break;
        }

        protected virtual Expression PostprocessExpression(
            Expression expression, 
            ITransformationContext context)
        {
            return expression;
        }

        protected IList<IExpressionRewriter> ReviewRewriters(
            IList<IExpressionRewriter> rewriters)
        {
            return rewriters;
        }

        private Expression TransformExpression(
            Expression expression,
            ITransformationContext context,
            TransformationStepRecorder recorder)
        {
            expression = this.PreprocessExpression(expression, context);

            if (context.IsFinished)
            {
                return expression;
            }

            // Set up the rewriter chain
            IList<IExpressionRewriter> allRewriters =
                new IExpressionRewriter[] 
                { 
                    new DatabaseParameterRewriter(context), 
                    new StoredProcedureParameterRewriter(context), 
                }
                .Concat(
                    this.GetRewriters(expression, context))
                .Concat(new IExpressionRewriter[] 
                { 
                    new TableScanRewriter(),
                    new QueryableRewriter()
                })
                .ToList();

            // Initialize rewriters
            foreach (IDatabaseComponent databaseComponent in allRewriters)
            {
                databaseComponent.Initialize(this.database);
            }

            // Last chance to alter rewriters
            allRewriters = this.ReviewRewriters(allRewriters);

            // Record the initial state
            recorder.Record(new TransformationStep(expression));

            // Execute rewriters
            foreach (IExpressionRewriter rewriter in allRewriters)
            {
                if (rewriter == null)
                {
                    continue;
                }

                expression = rewriter.Rewrite(expression);
                recorder.Record(new TransformationStep(expression));
            }

            expression = this.PostprocessExpression(expression, context);
            recorder.Record(new TransformationStep(expression));

            // Reduce the expression
            while (expression.CanReduce)
            {
                expression = expression.Reduce();
                recorder.Record(new TransformationStep(expression));
            }

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
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
