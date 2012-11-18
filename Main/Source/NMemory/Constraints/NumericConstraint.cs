// ----------------------------------------------------------------------------------
// <copyright file="NumericConstraint.cs" company="NMemory Team">
//     Copyright (C) 2012 NMemory Team
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
    using NMemory.Exceptions;

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
            if (value >= this.threshold)
            {
                throw new ConstraintException(string.Format("Column '{0}' overflowed.", this.PropertyName));
            }

            return Math.Round(value, this.fractionalPartDigits, MidpointRounding.AwayFromZero);
        }
    }
}
