// ----------------------------------------------------------------------------------
// <copyright file="LinqJoinKeyHelper.cs" company="NMemory Team">
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

namespace NMemory.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Linq.Expressions;
    using NMemory.Common.Expressions;

    internal class LinqJoinKeyHelper
    {
        public static void CreateKeySelectors(
            Type outerType,
            Type innerType,
            IExpressionBuilder[] outerKeyCreator,
            IExpressionBuilder[] innerKeyCreator,
            out LambdaExpression outerKey,
            out LambdaExpression innerKey)
        {
            // Key selector parameters
            ParameterExpression outerKeyParam =
                Expression.Parameter(outerType, "outer");
            ParameterExpression innerKeyParam =
                Expression.Parameter(innerType, "inner");

            // Create key member accessor expressions
            List<Expression> outerKeyMemberAccess = new List<Expression>();
            List<Expression> innerKeyMemberAccess = new List<Expression>();

            for (int i = 0; i < outerKeyCreator.Length; i++)
            {
                Expression outerAccess = outerKeyCreator[i].Build(outerKeyParam);
                Expression innerAccess = innerKeyCreator[i].Build(innerKeyParam);

                // KEY PART: try to unify the type of the expressions
                ExpressionHelper.TryUnifyValueTypes(ref outerAccess, ref innerAccess);

                outerKeyMemberAccess.Add(outerAccess);
                innerKeyMemberAccess.Add(innerAccess);
            }

            // Build key selector lambdas
            outerKey = ExpressionHelper.CreateMemberSelectorLambdaExpression(
                outerKeyMemberAccess.ToArray(),
                outerKeyParam);

            innerKey = ExpressionHelper.CreateMemberSelectorLambdaExpression(
                innerKeyMemberAccess.ToArray(),
                innerKeyParam);
        }
    }
}
