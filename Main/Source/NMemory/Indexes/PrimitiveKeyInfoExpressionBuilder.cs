using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Indexes
{
    internal class PrimitiveKeyInfoExpressionBuilder : IKeyInfoExpressionBuilder
    {
        private Type primitiveType;

        public PrimitiveKeyInfoExpressionBuilder(Type primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        public Expression CreateKeyFactoryExpression(params Expression[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            if (arguments.Length != 1)
	        {
                throw new ArgumentException("", "arguments");
	        }

            if (arguments[0].Type != primitiveType)
            {
                throw new ArgumentException("", "arguments");
            }

            return arguments[0];
        }

        public Expression CreateKeyMemberSelectorExpression(Expression source, int index)
        {
            if (index != 0)
            {
                throw new ArgumentException("", "index");
            }

            return source;
        }
    }
}
