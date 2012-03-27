using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common;

namespace NMemory.Constraints
{
    public abstract class ConstraintBase<TEntity, TProperty> : IConstraint<TEntity>
    {
        private Func<TEntity, TProperty> propertyGetter;
        private Action<TEntity, TProperty> propertySetter;
        private string propertyName;

        private ConstraintBase()
        {
        }

        protected ConstraintBase(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            MemberExpression member = propertySelector.Body as MemberExpression;

            if (member == null)
            {
                throw new ArgumentException("","propertySelector");
            }

            PropertyInfo propertyInfo = member.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                throw new ArgumentException("", "propertySelector");
            }

            this.propertyGetter = propertySelector.Compile();
            this.propertySetter = DynamicMethodBuilder.CreateSingleSetter<TEntity, TProperty>(propertyInfo);
            this.propertyName = propertyInfo.Name;
        }

        public void Apply(TEntity entity)
        {
            TProperty originalValue = this.propertyGetter.Invoke(entity);
            this.propertySetter.Invoke(entity, this.Apply(originalValue));
        }

        protected abstract TProperty Apply(TProperty value);

        protected string PropertyName
        {
            get { return this.propertyName; }
        }

    }
}
