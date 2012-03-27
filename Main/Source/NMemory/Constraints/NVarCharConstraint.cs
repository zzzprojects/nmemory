using System;
using System.Linq.Expressions;
using NMemory.Exceptions;

namespace NMemory.Constraints
{
    public class NVarCharConstraint<TEntity> : ConstraintBase<TEntity, string>
    {
        private int maxLength;

        public NVarCharConstraint(Expression<Func<TEntity, string>> propertySelector, int maxLength)
            : base(propertySelector)
        {
            this.maxLength = maxLength;
        }

        protected override string Apply(string value)
        {
            if (value != null && value.Length > maxLength)
            {
                throw new ConstraintException(string.Format("Column '{0}' cannot be longer than {1} characters.", this.PropertyName, this.maxLength));
            }

            return value;
        } 
    }
}
