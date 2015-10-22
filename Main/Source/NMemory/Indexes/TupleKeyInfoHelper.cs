// ----------------------------------------------------------------------------------
// <copyright file="TupleKeyInfoHelper.cs" company="NMemory Team">
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
// ----------------------------------------------------------------------------------

namespace NMemory.Indexes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Exceptions;

    public class TupleKeyInfoHelper : IKeyInfoHelper
    {
        private static readonly int largeTupleSize = 8;
        private readonly Type tupleType;

        public TupleKeyInfoHelper(Type tupleType)
        {
            if (!ReflectionHelper.IsTuple(tupleType))
            {
                throw new ArgumentException(ExceptionMessages.Missing, "tupleType");
            }

            this.tupleType = tupleType;
        }

        public int GetMemberCount()
        {
            int result = 0;
            Type tuple = this.tupleType;

            while (tuple != null)
            {
                Type[] members = tuple.GetGenericArguments();
                tuple = null;

                result += members.Length;

                if (members.Length == largeTupleSize)
                {
                    // The last member of a large tuple is a tuple that contains additional
                    // members
                    result--;
                    tuple = members[members.Length - 1];
                }
            }

            return result;
        }

        public Expression CreateKeyFactoryExpression(params Expression[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            int tupleSize = this.GetMemberCount();

            if (arguments.Length != tupleSize)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "arguments");
            }

            int memberIndex = 0;
            var res = CreateKeyFactoryExpression(this.tupleType, ref memberIndex, arguments);

            if (memberIndex != tupleSize)
            {
                throw new InvalidOperationException(ExceptionMessages.Missing);
            }

            return res;
        }

        public Expression CreateKeyMemberSelectorExpression(Expression source, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Type != this.tupleType)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "source");
            }

            if (this.GetMemberCount() <= index)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "index");
            }

            int mod = largeTupleSize - 1;

            int restCount = index / mod;
            int item = index % mod;

            Expression expr = source;

            for (int i = 0; i < restCount; i++)
            {
                expr = Expression.Property(expr, "Rest");
            }

            return Expression.Property(expr, string.Format("Item{0}", item + 1));
        }

        public bool TryParseKeySelectorExpression(
            Expression keySelector,
            bool strict,
            out MemberInfo[] result)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            if (keySelector.Type != this.tupleType)
            {
                result = null;
                return false;
            }

            var res = TryParseSelectorCore(keySelector, strict);

            if (res == null)
            {
                result = null;
                return false;
            }

            result = res.ToArray();
            return true;
        }

        private static Expression CreateKeyFactoryExpression(
            Type tuple,
            ref int memberIndex,
            Expression[] arguments)
        {
            Type[] memberTypes = tuple.GetGenericArguments();
            List<Expression> ctorArgs = new List<Expression>();

            for (int i = 0; i < memberTypes.Length; i++)
            {
                Type memberType = memberTypes[i];
                Expression expr;

                if (i == (largeTupleSize - 1))
                {
                    // The last member of a large tuple is tuple containing additional members
                    expr = CreateKeyFactoryExpression(memberType, ref memberIndex, arguments);
                }
                else
                {
                    expr = arguments[memberIndex];
                    memberIndex++;
                }

                ctorArgs.Add(expr);
            }

            return Expression.New(tuple.GetConstructors()[0], ctorArgs);
        }

        private static List<MemberInfo> TryParseSelectorCore(
            Expression expression,
            bool strict)
        {
            if (expression is NewExpression)
            {
                var newTupleExpression = (NewExpression)expression;

                return GetMemberInfoFromArguments(
                    newTupleExpression.Arguments,
                    true,
                    strict);
            }

            if (expression is MethodCallExpression)
            {
                var createTupleExpression = (MethodCallExpression)expression;

                MethodInfo createMethod = createTupleExpression.Method;

                if (createMethod.DeclaringType != typeof(Tuple) ||
                    createMethod.Name != "Create")
                {
                    return null;
                }

                return GetMemberInfoFromArguments(
                    createTupleExpression.Arguments,
                    false,
                    strict);
            }

            return null;
        }

        private static List<MemberInfo> GetMemberInfoFromArguments(
            IList<Expression> arguments, 
            bool ctorArgs,
            bool strict)
        {
            int argCount = arguments.Count;
            bool isLargeTuple = false;
            List<MemberInfo> result = new List<MemberInfo>();

            if (ctorArgs)
            {
                // The Tuple constructor supports extended tuple
                // The Tuple.Create method does not, however the 8-arg overload creates an
                // extended tuble

                if (largeTupleSize < argCount)
                {
                    // Too many arguments
                    return null;
                }

                if (largeTupleSize == argCount)
                {
                    // The last argument is a tuple extension
                    isLargeTuple = true;
                    argCount--;
                }
            }

            // Beware: argCount is not always equals to arguments.Count, see above
            for (int i = 0; i < argCount; i++)
            {
                Expression expr = arguments[i];

                if (!strict)
                {
                    expr = ExpressionHelper.SkipConversionNodes(expr);
                }

                MemberExpression member = expr as MemberExpression;

                if (member == null)
                {
                    return null;
                }

                if (member.Expression.NodeType != ExpressionType.Parameter)
                {
                    return null;
                }

                result.Add(member.Member);
            }

            if (isLargeTuple)
            {
                // Last argument is the extension
                Expression tupleExtension = arguments[arguments.Count - 1];

                if (!ReflectionHelper.IsTuple(tupleExtension.Type))
                {
                    // This should not happen
                    return null;
                }

                var additionalMembers = TryParseSelectorCore(tupleExtension, strict);

                result.AddRange(additionalMembers);
            }

            return result;
        }
    }
}
