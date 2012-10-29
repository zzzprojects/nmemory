// ----------------------------------------------------------------------------------
// <copyright file="NotNullableConstraint.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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

namespace NMemory.Constraints
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Exceptions;

    public class NotNullableConstraint<TEntity> : IConstraint<TEntity>
    {
        private Func<TEntity, object> propertyGetter;
        private string propertyName;

        public NotNullableConstraint(Expression<Func<TEntity, object>> propertySelector)
        {
            UnaryExpression castexp = propertySelector.Body as UnaryExpression;
            MemberExpression member=null;
            if (castexp != null)
            {
                member = castexp.Operand as MemberExpression;
            }
            else
            { 
                member = propertySelector.Body as MemberExpression;
            }


            if (member == null)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "propertySelector");
            }

            PropertyInfo propertyInfo = member.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "propertySelector");
            }

            this.propertyGetter = propertySelector.Compile();
            this.propertyName = propertyInfo.Name;
        }

        public void Apply(TEntity entity)
        {
            object originalValue = this.propertyGetter.Invoke(entity);

            if (originalValue == null)
            {
                throw new ConstraintException(string.Format("Column '{0}' cannot be null", this.propertyName));
            }
        }

    }
}
