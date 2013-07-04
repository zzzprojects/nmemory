// ----------------------------------------------------------------------------------
// <copyright file="NumericConstraint.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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
    using NMemory.Common;
    using NMemory.Exceptions;
    using NMemory.Execution;

    public class NumericConstraint<TEntity> : ConstraintBase<TEntity, decimal>
    {
        private readonly int allDigits;
        private readonly int fractionalPartDigits;
        private readonly long threshold;

        public NumericConstraint(
            IEntityMemberInfo<TEntity, decimal> member,
            int precision,
            int scale)
            : base(member)
        {
            Validate(precision, scale);

            this.allDigits = precision;
            this.fractionalPartDigits = scale;
            this.threshold = (long)Math.Pow(10, this.WholePartDigits);
        }

        public NumericConstraint(
            Expression<Func<TEntity, decimal>> memberSelector, 
            int precision, 
            int scale) 
            : base(memberSelector)
        {
            Validate(precision, scale);

            this.allDigits = precision;
            this.fractionalPartDigits = scale;
            this.threshold = (long)Math.Pow(10, this.WholePartDigits);
        }

        private static void Validate(int precision, int scale)
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
        }

        private int WholePartDigits
        {
            get
            {
                return this.allDigits - this.fractionalPartDigits;
            }
        }

        protected override decimal Apply(decimal value, IExecutionContext context)
        {
            if (value >= this.threshold)
            {
                throw new ConstraintException(
                    string.Format("Column '{0}' overflowed.", this.MemberName));
            }

            return Math.Round(value, this.fractionalPartDigits, MidpointRounding.AwayFromZero);
        }
    }
}
