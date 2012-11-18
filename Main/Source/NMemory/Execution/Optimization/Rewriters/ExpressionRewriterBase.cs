// ----------------------------------------------------------------------------------
// <copyright file="ExpressionRewriterBase.cs" company="NMemory Team">
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

namespace NMemory.Execution.Optimization.Rewriters
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using NMemory.Modularity;

    public abstract class ExpressionRewriterBase : ExpressionVisitor, IExpressionRewriter, IDatabaseComponent
    {
        private IDatabase database;

        protected ILoggingPort LoggingPort
        {
            get 
            {
                if (this.database != null)
                {
                    return this.database.DatabaseEngine.LoggingPort; 
                }

                return null;
            }
        }

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }

        public Expression Rewrite(Expression expression)
        {
            return this.Visit(expression);
        }

        protected virtual IList<Expression> VisitExpressions(IList<Expression> original)
        {
            List<Expression> sequence = null;
            int num = 0;
            int count = original.Count;

            for (int i = 0; i < original.Count; i++)
            {
                Expression item = this.Visit(original[i]);

                if (sequence != null)
                {
                    sequence.Add(item);
                }
                else if (item != original[num])
                {
                    sequence = new List<Expression>(count);

                    for (int j = 0; j < i; j++)
                    {
                        sequence.Add(original[j]);
                    }

                    sequence.Add(item);
                }
            }

            if (sequence != null)
            {
                return sequence;
            }

            return original;
        }
    }
}
