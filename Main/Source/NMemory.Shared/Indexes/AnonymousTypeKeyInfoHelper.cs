// ----------------------------------------------------------------------------------
// <copyright file="AnonymousTypeKeyInfoHelper.cs" company="NMemory Team">
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
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Exceptions;

    public class AnonymousTypeKeyInfoHelper : IKeyInfoHelper
    {
        private readonly Type anonymousType;
        private readonly PropertyInfo[] orderedProperties;

        public AnonymousTypeKeyInfoHelper(Type anonymousType)
        {
            if (!ReflectionHelper.IsAnonymousType(anonymousType))
            {
                throw new ArgumentException(ExceptionMessages.Missing, "anonymousType");
            }

            this.anonymousType = anonymousType;

            // Does GetPropeties really ensure appropriately ordered property list?

            // This code ensures an ordered property list based on the order of generic 
            // arguments

            // Anonymous types are generic
            Type genericAnonType = this.anonymousType.GetGenericTypeDefinition();
            Type[] genericArgs = genericAnonType.GetGenericArguments();

            // The type of the properties are generic
            int propCount = genericArgs.Length;
            this.orderedProperties = new PropertyInfo[propCount];

            PropertyInfo[] genericProps = genericAnonType.GetProperties();

            for (int i = 0; i < propCount; i++)
            {
                for (int j = 0; j < propCount; j++)
                {
                    if (genericProps[j].PropertyType == genericArgs[i])
                    {
                        this.orderedProperties[i] =
                            this.anonymousType.GetProperty(genericProps[j].Name);

                        break;
                    }
                }
            }
        }

        public int GetMemberCount()
        {
            return this.anonymousType.GetProperties().Length;
        }

        public Expression CreateKeyFactoryExpression(params Expression[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            if (arguments.Length != this.orderedProperties.Length)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "arguments");
            }

            for (int i = 0; i < this.orderedProperties.Length; i++)
            {
                if (this.orderedProperties[i].PropertyType != arguments[i].Type)
                {
                    throw new ArgumentException(ExceptionMessages.Missing, "arguments");
                }
            }

            return Expression.New(this.anonymousType.GetConstructors()[0], arguments, this.orderedProperties);
        }

        public Expression CreateKeyMemberSelectorExpression(Expression source, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (this.anonymousType != source.Type)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "arguments");
            }

            if (this.orderedProperties.Length <= index)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "arguments");
            }

            PropertyInfo property = this.orderedProperties[index];

            return Expression.Property(source, property);
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

            result = null;

            if (keySelector.Type != this.anonymousType)
            {
                return false;
            }

            NewExpression resultCreator = keySelector as NewExpression;

            if (resultCreator == null)
            {
                return false;
            }

            if (resultCreator.Type != this.anonymousType)
            {
                return false;
            }

            List<MemberInfo> resultList = new List<MemberInfo>();

            foreach (Expression arg in resultCreator.Arguments)
            {
                Expression expr = arg;

                if (!strict)
                {
                    expr = ExpressionHelper.SkipConversionNodes(expr);
                }

                MemberExpression member = expr as MemberExpression;

                if (member == null)
                {
                    return false;
                }

                //if (member.Expression.NodeType != ExpressionType.Parameter)
                //{
                //    throw new ArgumentException("Invalid expression", "keySelector");
                //}

                resultList.Add(member.Member);
            }

            result = resultList.ToArray();
            return true;
        }
    }
}
