// -----------------------------------------------------------------------------------
// <copyright file="IdentityField.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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
// -----------------------------------------------------------------------------------

namespace NMemory.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using NMemory.Common;

    internal class IdentityField<TEntity>
    {
        private Func<TEntity, long> identityGetter;
        private DynamicPropertySetter<TEntity> identitySetter;
        private IdentitySpecification<TEntity> identitySpecification;
        private long nextIdentity;
        private Type identityType;

        internal IdentityField(IdentitySpecification<TEntity> identitySpecification)
        {
            this.identitySpecification = identitySpecification;

            this.nextIdentity = this.identitySpecification.Seed;

            MemberExpression member = ExpressionHelper.FindMemberExpression(identitySpecification.IdentityColumn.Body);
            PropertyInfo identityInfo = member.Member as PropertyInfo;

            this.identityType = identityInfo.PropertyType;
            this.identitySetter = DynamicMethodBuilder.CreatePropertySetter<TEntity>(identityInfo);
            this.identityGetter = this.identitySpecification.IdentityColumn.Compile();
        }

        internal void InitializeBasedOnData(IEnumerable<TEntity> initialEntities, bool forceMinValue = false)
        {
            long? currentNextIdentity = null;

            foreach (TEntity entity in initialEntities)
            {
                long entityIdentity = this.identityGetter(entity);

                if (!currentNextIdentity.HasValue ||
                    (this.identitySpecification.Increment < 0 && entityIdentity < currentNextIdentity.Value) ||
                    (this.identitySpecification.Increment > 0 && entityIdentity > currentNextIdentity.Value))
                {
                    currentNextIdentity = entityIdentity;
                }
            }

            if (currentNextIdentity.HasValue)
            {
	            var nextValue = currentNextIdentity.Value + this.identitySpecification.Increment;
                this.nextIdentity = nextValue > 0 ? nextValue : forceMinValue ? 1 : nextValue;
            }
        }

        internal void Generate(TEntity entity)
        {
            long nextValue = Interlocked.Add(ref this.nextIdentity, this.identitySpecification.Increment);
            long currentValue = nextValue - this.identitySpecification.Increment;

            // Int64 to IntX conversion
            object currentValueConverted = Convert.ChangeType(currentValue, this.identityType);

            this.identitySetter(entity, currentValueConverted);
        }

    }
}
