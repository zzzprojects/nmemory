// ----------------------------------------------------------------------------------
// <copyright file="AnonymousTypeKeyInfoExpressionBuilder.cs" company="NMemory Team">
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

namespace NMemory.Indexes
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Exceptions;

    internal class AnonymousTypeKeyInfoExpressionBuilder : IKeyInfoExpressionBuilder
    {
        private Type anonymousType;
        private PropertyInfo[] orderedProperties;

        public AnonymousTypeKeyInfoExpressionBuilder(Type anonymousType)
        {
            if (!ReflectionHelper.IsAnonymousType(anonymousType))
            {
                throw new ArgumentException(ExceptionMessages.Missing, "anonymousType");
            }

            this.anonymousType = anonymousType;
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
            // Ensure an ordered propety list based on the order of generic arguments

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
                        this.orderedProperties[i] = this.anonymousType.GetProperty(genericProps[j].Name);
                        break;
                    }
                }
            }
        }
    }
}
