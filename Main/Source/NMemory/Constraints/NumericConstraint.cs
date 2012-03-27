using System;
using System.Linq.Expressions;
using NMemory.Exceptions;

namespace NMemory.Constraints
{
    public class NumericConstraint<TEntity> : ConstraintBase<TEntity, decimal>
    {
        private int allDigits;
        private int fractionalPartDigits;
        private long threshold;

        public NumericConstraint(Expression<Func<TEntity, decimal>> propertySelector, int precision, int scale) 
            : base(propertySelector)
        {
            if (scale < 0)
            {
                throw new ArgumentException("Scale cannot be less than 0.", "scale");
            }
            if (precision < 1)
            {
                throw new ArgumentException("Precision cannot be less than 1.", "precision");
            }
            if (scale > precision)
            {
                throw new ArgumentException("Scale cannot be greater than precision.", "scale");
            }


            this.allDigits = precision;
            this.fractionalPartDigits = scale;
            this.threshold = (long)Math.Pow(10, this.WholePartDigits);
        }

        private int WholePartDigits
        {
            get
            {
                return this.allDigits - this.fractionalPartDigits;
            }
        }

        protected override decimal Apply(decimal value)
        {
            if (value >= threshold)
            {
                throw new ConstraintException(string.Format("Column '{0}' overflowed.", base.PropertyName));
            }

            return Math.Round(value, this.fractionalPartDigits, MidpointRounding.AwayFromZero);
        }
    }
}
