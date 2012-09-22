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

        internal void InitializeBasedOnData(IEnumerable<TEntity> initialEntities)
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
                this.nextIdentity = currentNextIdentity.Value + this.identitySpecification.Increment;
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
