using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common;

namespace NMemory.Indexes
{
    internal class AnonymousTypeKeyInfoExpressionBuilder : IKeyInfoExpressionBuilder
    {
        private Type anonymousType;
        private PropertyInfo[] orderedProperties;

        public AnonymousTypeKeyInfoExpressionBuilder(Type anonymousType)
        {
            if (!ReflectionHelper.IsAnonymousType(anonymousType))
            {
                throw new ArgumentException("", "anonymousType");
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
                throw new ArgumentException("", "arguments");
            }

            for (int i = 0; i < this.orderedProperties.Length; i++)
            {
                if (orderedProperties[i].PropertyType != arguments[i].Type)
                {
                    throw new ArgumentException("", "arguments");
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
                throw new ArgumentException("", "arguments");
            }

            this.EnsureOrderedPropeties();

            if (this.orderedProperties.Length <= index)
            {
                throw new ArgumentException("", "arguments");
            }

            PropertyInfo property = this.orderedProperties[index];

            return Expression.Property(source, property);
        }

        private void EnsureOrderedPropeties()
        {
            if (orderedProperties != null)
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
