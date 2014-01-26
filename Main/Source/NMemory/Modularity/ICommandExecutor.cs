// ----------------------------------------------------------------------------------
// <copyright file="ICommandExecutor.cs" company="NMemory Team">
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

namespace NMemory.Modularity
{
    using System.Collections.Generic;
    using NMemory.Execution;

    public interface ICommandExecutor : IDatabaseComponent
    {
        IEnumerator<T> ExecuteQuery<T>(
            IExecutionPlan<IEnumerable<T>> plan, 
            IExecutionContext context);

        T ExecuteQuery<T>(
            IExecutionPlan<T> plan, 
            IExecutionContext context);

        void ExecuteInsert<T>(
            T entity, 
            IExecutionContext context)
            where T : class;

        IEnumerable<T> ExecuteDelete<T>(
            IExecutionPlan<IEnumerable<T>> plan, 
            IExecutionContext context)
            where T : class;

        IEnumerable<T> ExecuteUpdater<T>(
            IExecutionPlan<IEnumerable<T>> plan, 
            IUpdater<T> updater, 
            IExecutionContext context)
            where T : class;
    }
}
