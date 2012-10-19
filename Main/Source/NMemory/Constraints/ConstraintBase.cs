// ----------------------------------------------------------------------------------
// <copyright file="ConstraintBase.cs" company="NMemory Team">
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
    using NMemory.Common;
    using NMemory.Exceptions;

    public abstract class ConstraintBase<TEntity, TProperty> : IConstraint<TEntity>
    {
        private Func<TEntity, TProperty> propertyGetter;
        private Action<TEntity, TProperty> propertySetter;
        private string propertyName;

        protected ConstraintBase(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            MemberExpression member = propertySelector.Body as MemberExpression;

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
            this.propertySetter = DynamicMethodBuilder.CreateSinglePropertySetter<TEntity, TProperty>(propertyInfo);
            this.propertyName = propertyInfo.Name;
        }

        private ConstraintBase()
        {
        }

        protected string PropertyName
        {
            get { return this.propertyName; }
        }

        public void Apply(TEntity entity)
        {
            TProperty originalValue = this.propertyGetter.Invoke(entity);
            this.propertySetter.Invoke(entity, this.Apply(originalValue));
        }

        protected abstract TProperty Apply(TProperty value);
    }
}
