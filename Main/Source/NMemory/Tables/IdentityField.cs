using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using NMemory.Common;

namespace NMemory.Tables
{
    internal class IdentityField<TEntity>
    {
        private Func<TEntity, long> identityGetter;
        private DynamicPropertySetter<TEntity> identitySetter;
        private IdentitySpecification<TEntity> identity;
        private long nextIdentity;
        private Type identityType;

        internal IdentityField(IdentitySpecification<TEntity> identitySpecification, IEnumerable<TEntity> initialEntities)
        {
            this.identity = identitySpecification;

            this.nextIdentity = this.identity.Seed;

            MemberExpression member = ExpressionHelper.FindMemberExpression(identitySpecification.IdentityColumn.Body);
            PropertyInfo identityInfo = member.Member as PropertyInfo;

            this.identityType = identityInfo.PropertyType;
            this.identitySetter = DynamicMethodBuilder.CreatePropertySetter<TEntity>(identityInfo);
            this.identityGetter = this.identity.IdentityColumn.Compile();

            if (initialEntities != null)
            {
                this.nextIdentity =
                    initialEntities.Max(e => this.identityGetter(e))
                    + this.identity.Increment;
            }
        }

        internal void Generate(TEntity entity)
        {
            long nextValue = Interlocked.Add(ref this.nextIdentity, this.identity.Increment);
            long currentValue = nextValue - this.identity.Increment;

            // Int64 to IntX conversion
            object currentValueConverted = Convert.ChangeType(currentValue, this.identityType);

            this.identitySetter(entity, currentValueConverted);
        }

    }
}
