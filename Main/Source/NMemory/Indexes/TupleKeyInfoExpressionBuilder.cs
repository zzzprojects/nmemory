using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common;

namespace NMemory.Indexes
{
    internal class TupleKeyInfoExpressionBuilder : IKeyInfoExpressionBuilder
    {
        private Type tupleType;

        public TupleKeyInfoExpressionBuilder(Type tupleType)
        {
            if (!ReflectionHelper.IsTuple(tupleType))
            {
                throw new ArgumentException("", "tupleType");
            }

            this.tupleType = tupleType;
        }

        public Expression CreateKeyFactoryExpression(params Expression[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            var genericArguments = this.tupleType.GetGenericArguments();

            if (arguments.Length != genericArguments.Length)
            {
                throw new ArgumentException("", "arguments");
            }

            for (int i = 0; i < genericArguments.Length; i++)
            {
                if (genericArguments[i] != arguments[i].Type)
                {
                    throw new ArgumentException("", "arguments");
                }
            }


            return Expression.New(this.tupleType.GetConstructors()[0], arguments);
        }

        public Expression CreateKeyMemberSelectorExpression(Expression source, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Type != this.tupleType)
            {
                throw new ArgumentException("", "source");
            }

            string propertyName = string.Format("Item{0}", index + 1);
            PropertyInfo property = this.tupleType.GetProperty(propertyName);

            if (property == null)
            {
                throw new ArgumentException("", "index");
            }

            return Expression.Property(source, property);
        }
    }
}
