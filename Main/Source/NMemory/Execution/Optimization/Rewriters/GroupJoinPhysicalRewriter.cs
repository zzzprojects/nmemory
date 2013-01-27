// ----------------------------------------------------------------------------------
// <copyright file="GroupJoinPhysicalRewriter.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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
    using NMemory.Common;
    using NMemory.Indexes;
    using NMemory.Tables;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public class GroupJoinPhysicalRewriter : ExpressionRewriterBase
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.Method.IsGenericMethod ||
                node.Method.GetGenericMethodDefinition() != QueryMethods.GroupJoin)
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

            // Key type
            LambdaExpression secondKeySelector = 
                ExpressionHelper.SkipQuoteNode(node.Arguments[3]) as LambdaExpression;

            Type keyType = secondKeySelector.Body.Type;
    
            DefaultKeyInfoExpressionServicesProvider provider;

            if (!DefaultKeyInfoExpressionServicesProvider.TryCreate(keyType, out provider))
            {
                // Cannot detect key
                return base.VisitMethodCall(node);
            }

            var services = provider.KeyInfoExpressionServices;

            // TODO: TryParseKeySelectorExpression
            MemberInfo[] keyMembers = 
                services.ParseKeySelectorExpression(secondKeySelector.Body, false);

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

            var indexServices = GetKeyInfoExpressionServices(matchedIndex.KeyInfo);

            if (indexServices == null)
            {
                return base.VisitMethodCall(node);
            }

            // Create key converter
            ParameterExpression keyConverterParam = Expression.Parameter(keyType, "x");

            LambdaExpression keyConverter = 
                Expression.Lambda(
                    KeyExpressionHelper.CreateKeyConversionExpression(
                        keyConverterParam,
                        services,
                        indexServices,
                        matchedIndex.KeyInfo.EntityKeyMembers,
                        mapping),
                    keyConverterParam);

            // Create key emptyness detector
            ParameterExpression keyEmptinessDetectorParam = Expression.Parameter(keyType, "x");

            LambdaExpression keyEmptinessDetector =
                Expression.Lambda(
                    KeyExpressionHelper.CreateKeyEmptinessDetector(
                        keyEmptinessDetectorParam, 
                        services),
                    keyEmptinessDetectorParam);

            Type[] generics = node.Method.GetGenericArguments();

            MethodInfo groupJoinIndexedMethod = QueryMethods
                .GroupJoinIndex
                .MakeGenericMethod(
                    generics[0],
                    generics[2],
                    generics[1],
                    matchedIndex.KeyInfo.KeyType,
                    generics[3]);

            Expression groupJoinIndexedCall =
                Expression.Call(
                    node.Object,
                    groupJoinIndexedMethod,
                    this.Visit(node.Arguments[0]),
                    Expression.Constant(matchedIndex),
                    ExpressionHelper.SkipQuoteNode(node.Arguments[2]),
                    keyEmptinessDetector,
                    keyConverter,
                    ExpressionHelper.SkipQuoteNode(node.Arguments[4]));

            Type elementType = ReflectionHelper.GetElementType(groupJoinIndexedCall.Type);

            return Expression.Call(
                null,
                QueryMethods.AsQueryable.MakeGenericMethod(elementType), 
                groupJoinIndexedCall);
        }

        private static IKeyInfoExpressionServices GetKeyInfoExpressionServices(IKeyInfo keyInfo)
        {
            var provider = keyInfo as IKeyInfoExpressionServicesProvider;

            if (provider == null)
            {
                return null;
            }

            return provider.KeyInfoExpressionServices;
        }
    }
}
