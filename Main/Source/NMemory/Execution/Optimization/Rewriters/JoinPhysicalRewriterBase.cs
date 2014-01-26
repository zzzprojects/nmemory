// ----------------------------------------------------------------------------------
// <copyright file="JoinPhysicalRewriterBase.cs" company="NMemory Team">
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

namespace NMemory.Execution.Optimization.Rewriters
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Indexes;
    using NMemory.Services;
    using NMemory.Services.Contracts;
    using NMemory.Tables;

    public abstract class JoinPhysicalRewriterBase : ExpressionRewriterBase
    {
        protected abstract MethodInfo Original
        {
            get;
        }

        protected abstract MethodInfo Indexed
        {
            get;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.Method.IsGenericMethod ||
                node.Method.GetGenericMethodDefinition() != this.Original)
            {
                // Not GroupJoin
                return base.VisitMethodCall(node);
            }

            ConstantExpression secondExpression = node.Arguments[1] as ConstantExpression;

            if (secondExpression == null)
            {
                // Not constant expression
                return base.VisitMethodCall(node);
            }

            ITable table = secondExpression.Value as ITable;

            if (table == null)
            {
                // Not table
                return base.VisitMethodCall(node);
            }

            LambdaExpression secondKeySelector = 
                ExpressionHelper.SkipQuoteNode(node.Arguments[3]) as LambdaExpression;

            Type keyType = secondKeySelector.Body.Type;
            IKeyInfoHelper helper = this.GetKeyInfoHelper(keyType);

            if (helper == null)
            {
                // Cannot detect key type
                return base.VisitMethodCall(node);
            }

            // Try to parse expression
            MemberInfo[] keyMembers;

            if (!helper.TryParseKeySelectorExpression(
                secondKeySelector.Body, 
                false, 
                out keyMembers))
            {
                // Cannot parse key
                return base.VisitMethodCall(node);
            }

            IIndex matchedIndex = null;
            int[] mapping = null;

            foreach (IIndex index in table.Indexes)
            {
                if (KeyExpressionHelper.TryGetMemberMapping(
                    index.KeyInfo.EntityKeyMembers, 
                    keyMembers, 
                    out mapping))
                {
                    matchedIndex = index;
                    break;
                }
            }

            if (matchedIndex == null)
            {
                // No matched index was found
                return base.VisitMethodCall(node);
            }

            var indexServices = GetKeyInfoHelper(matchedIndex.KeyInfo);

            if (indexServices == null)
            {
                return base.VisitMethodCall(node);
            }

            // Create key converter
            ParameterExpression keyConverterParam = Expression.Parameter(keyType, "x");

            LambdaExpression keyConverter = 
                Expression.Lambda(
                    KeyExpressionHelper.CreateKeyConversionExpression(keyConverterParam, matchedIndex.KeyInfo.EntityKeyMembers, mapping, helper, indexServices),
                    keyConverterParam);

            // Create key emptyness detector
            ParameterExpression keyEmptinessDetectorParam = Expression.Parameter(keyType, "x");

            LambdaExpression keyEmptinessDetector =
                Expression.Lambda(
                    KeyExpressionHelper.CreateKeyEmptinessDetector(
                        keyEmptinessDetectorParam, 
                        helper),
                    keyEmptinessDetectorParam);

            Type[] generics = node.Method.GetGenericArguments();

            // GroupJoinIndexed call
            MethodInfo indexedMethod = this.Indexed
                .MakeGenericMethod(
                    generics[0],
                    generics[1],
                    generics[2],
                    matchedIndex.KeyInfo.KeyType,
                    generics[3]);

            Expression indexedCall =
                Expression.Call(
                    node.Object,
                    indexedMethod,
                    this.Visit(node.Arguments[0]),
                    Expression.Constant(matchedIndex),
                    ExpressionHelper.SkipQuoteNode(node.Arguments[2]),
                    keyEmptinessDetector,
                    keyConverter,
                    ExpressionHelper.SkipQuoteNode(node.Arguments[4]));

            Type elementType = ReflectionHelper.GetElementType(indexedCall.Type);

            // AsQueryable call
            return Expression.Call(
                null,
                QueryMethods.AsQueryable.MakeGenericMethod(elementType), 
                indexedCall);
        }

        private static IKeyInfoHelper GetKeyInfoHelper(IKeyInfo keyInfo)
        {
            var provider = keyInfo as IKeyInfoHelperProvider;

            if (provider == null)
            {
                return null;
            }

            return provider.KeyInfoHelper;
        }

        private IKeyInfoHelper GetKeyInfoHelper(Type keyType)
        {
            IKeyInfoService service = null;

            if (this.Database != null)
            {
                service = this
                    .Database
                    .DatabaseEngine
                    .ServiceProvider
                    .GetService<IKeyInfoService>();
            }

            if (service == null)
            {
                // Should we really fall back?
                // Tests would fail without this, add injection?
                service = DefaultServiceConfigurations.CreateDefaultKeyInfoService();
            }

            IKeyInfoHelper result;
            service.TryCreateKeyInfoHelper(keyType, out result);

            return result;
        }
    }
}
