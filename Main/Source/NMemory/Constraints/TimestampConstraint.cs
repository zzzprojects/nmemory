using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Data;
using System.Linq.Expressions;

namespace NMemory.Constraints
{
    internal class TimestampConstraint<TEntity> : ConstraintBase<TEntity, Timestamp>
    {
        public TimestampConstraint(Expression<Func<TEntity, Timestamp>> propertySelector) : base(propertySelector)
        {

        }

        protected override Timestamp Apply(Timestamp value)
        {
            return Timestamp.CreateNew();
        }
    }
}
