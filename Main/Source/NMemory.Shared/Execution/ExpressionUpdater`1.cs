// ----------------------------------------------------------------------------------
// <copyright file="DelegateUpdater`1.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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
// --------------------------------------------------------------------------------

namespace NMemory.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ExpressionUpdater<T> : IUpdater<T>
    {
        private readonly Func<T, T> updater;
        private readonly Expression<Func<T, T>> updaterExpr;

        public ExpressionUpdater(Expression<Func<T, T>> updater)
        {
            this.updater = updater.Compile();
            this.updaterExpr = updater;
        }

        public T Update(T entity)
        {
            return this.updater.Invoke(entity);
        }


        public MemberInfo[] Changes
        {
            get 
            {
                var creator = this.updaterExpr.Body as MemberInitExpression;
                var changes = new List<MemberInfo>();

                foreach (MemberAssignment assign in creator.Bindings)
                {
                    MemberExpression memberRead = assign.Expression as MemberExpression;

                    // Check if the member is not assigned with the same member
                    if (memberRead == null || memberRead.Member.Name != assign.Member.Name)
                    {
                        changes.Add(assign.Member as PropertyInfo);
                        continue;
                    }

                    // Check if the source is not the parameter
                    if (!(memberRead.Expression is ParameterExpression))
                    {
                        changes.Add(assign.Member);
                        continue;
                    }
                }

                return changes.ToArray();
            }
        }
    }
}
