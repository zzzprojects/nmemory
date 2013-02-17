// ----------------------------------------------------------------------------------
// <copyright file="AnonymousTypeKeyInfoExpressionBuilder.cs" company="NMemory Team">
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

namespace NMemory.Indexes
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Exceptions;
    using System.Collections.Generic;

    internal class AnonymousTypeKeyInfoExpressionServices : IKeyInfoExpressionServices
    {
        private Type anonymousType;
        private PropertyInfo[] orderedProperties;

        public AnonymousTypeKeyInfoExpressionServices(Type anonymousType)
        {
            if (!ReflectionHelper.IsAnonymousType(anonymousType))
            {
                throw new ArgumentException(ExceptionMessages.Missing, "anonymousType");
            }

            this.anonymousType = anonymousType;
        }

        public int GetMemberCount()
        {
            return this.anonymousType.GetProperties().Length;
        }

        public Expression CreateKeyFactoryExpression(params Expression[] arguments)
        {
            this.EnsureOrderedPropeties();

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

            this.EnsureOrderedPropeties();

            if (this.orderedProperties.Length <= index)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "arguments");
            }

            PropertyInfo property = this.orderedProperties[index];

            return Expression.Property(source, property);
        }

        private void EnsureOrderedPropeties()
        {
            if (this.orderedProperties != null)
            {
                return;
            }

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

        public MemberInfo[] ParseKeySelectorExpression(
            Expression keySelector, 
            bool strict)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            MemberInfo[] result;

            if (ParseKeySelectorExpressionCore(keySelector, strict, true, out result))
            {
                return result;
            }

            throw new ArgumentException("", "keySelector");
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

            return ParseKeySelectorExpressionCore(keySelector, strict, false, out result);
        }

        private bool ParseKeySelectorExpressionCore(
            Expression keySelector,
            bool strict,
            bool throwException,
            out MemberInfo[] result)
        {
            result = null;

            if (keySelector.Type != this.anonymousType)
            {
                if (throwException)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }
                else
                {
                    return false;
                }
            }

            NewExpression resultCreator = keySelector as NewExpression;

            if (resultCreator == null)
            {
                if (throwException)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }
                else
                {
                    return false;
                }
            }

            if (resultCreator.Type != this.anonymousType)
            {
                if (throwException)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }
                else
                {
                    return false;
                }
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
                    if (throwException)
                    {
                        throw new ArgumentException("Invalid expression", "keySelector");
                    }
                    else
                    {
                        return false;
                    }
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
