using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.Exceptions;
using System.Reflection;

namespace NMemory.Constraints
{
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
                throw new ArgumentException("","propertySelector");
            }

            PropertyInfo propertyInfo = member.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                throw new ArgumentException("", "propertySelector");
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
