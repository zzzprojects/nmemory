// ----------------------------------------------------------------------------------
// <copyright file="QueryCompiler.cs" company="NMemory Team">
//     Copyright (C) 2012 NMemory Team
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Execution.Optimization;
    using NMemory.Execution.Optimization.Rewriters;

    public class QueryCompiler : QueryCompilerBase
    {
        public bool EnableCompilationCaching { get; set; }

        public bool EnableOptimization { get; set; }

        protected override IEnumerable<IExpressionRewriter> GetRewriters(Expression expression, TransformationContext context)
        {
            return base.GetRewriters(expression, context)
                .Concat(this.GetCustomRewriters(expression, context));
        }

        private IEnumerable<IExpressionRewriter> GetCustomRewriters(Expression expression, TransformationContext context)
        {
            if (this.EnableOptimization)
            {
                yield return new InnerJoinLogicalRewriter();

                yield return new OuterJoinLogicalRewriter();
            }

            yield return new PropertyAccessRewriter();

            yield break;
        }
    }
}
