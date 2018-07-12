// ----------------------------------------------------------------------------------
// <copyright file="ConstraintBase.cs" company="NMemory Team">
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
// ----------------------------------------------------------------------------------

namespace NMemory.Constraints
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Exceptions;
    using NMemory.Execution;

    public abstract class ConstraintBase<TEntity, TMember> : IConstraint<TEntity>
    {
        private readonly IEntityMemberInfo<TEntity, TMember> member;
        private readonly IEntityMemberInfoServices<TEntity, TMember> memberServices;

        protected ConstraintBase(Expression<Func<TEntity, TMember>> memberSelector)
            : this(ParseSelectorExpression(memberSelector))
        {
        }

        protected ConstraintBase(IEntityMemberInfo<TEntity, TMember> member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            this.member = member;

            var servicesProvider = 
                this.member as IEntityMemberInfoServicesProvider<TEntity, TMember>;

            if (servicesProvider != null)
            {
                this.memberServices = 
                    servicesProvider.EntityMemberInfoServices;
            }

            if (this.memberServices == null)
            {
                this.memberServices = 
                    new DefaultEntityMemberInfoServices<TEntity, TMember>(this.member);
            }
        }

        private ConstraintBase()
        {
        }

        protected string MemberName
        {
            get { return this.member.Member.Name; }
        }

        public void Apply(TEntity entity, IExecutionContext context)
        {
            TMember originalValue = this.memberServices.GetValue(entity);
            TMember newValue = this.Apply(originalValue, context);

            this.memberServices.SetValue(entity, newValue);
        }

        protected abstract TMember Apply(TMember value, IExecutionContext context);

        private static IEntityMemberInfo<TEntity, TMember> ParseSelectorExpression(
            Expression<Func<TEntity, TMember>> selector)
        {
            return new DefaultEntityMemberInfo<TEntity, TMember>(selector);
        }
    }
}
