// ----------------------------------------------------------------------------------
// <copyright file="DefaultEntityMemberInfoServices`2.cs" company="NMemory Team">
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

namespace NMemory.Common
{
    using System;
    using System.Linq.Expressions;

    public class DefaultEntityMemberInfoServices<TEntity, TMember>
        : IEntityMemberInfoServices<TEntity, TMember>
    {
        private readonly Func<TEntity, TMember> getter;
        private readonly Action<TEntity, TMember> setter;

        public DefaultEntityMemberInfoServices(IEntityMemberInfo memberInfo)
        {
            this.setter = CreateSetter(memberInfo).Compile();
            this.getter = CreateGetter(memberInfo).Compile();
        }

        private static Expression<Action<TEntity, TMember>> CreateSetter(IEntityMemberInfo memberInfo)
        {
            ParameterExpression entity = Expression.Parameter(memberInfo.EntityType);
            ParameterExpression value = Expression.Parameter(memberInfo.MemberType);

            Expression setterBody = Expression.Assign(
                Expression.PropertyOrField(entity, memberInfo.Member.Name),
                value);

            return Expression.Lambda<Action<TEntity, TMember>>(setterBody, entity, value);
        }

        private static Expression<Func<TEntity, TMember>> CreateGetter(IEntityMemberInfo memberInfo)
        {
            ParameterExpression entity = Expression.Parameter(memberInfo.EntityType);

            Expression setterBody = Expression.MakeMemberAccess(entity, memberInfo.Member);

            return Expression.Lambda<Func<TEntity, TMember>>(setterBody, entity);
        }
        
        public TMember GetValue(TEntity entity)
        {
            return this.getter.Invoke(entity);
        }

        public void SetValue(TEntity entity, TMember value)
        {
            this.setter.Invoke(entity, value);
        }
    }
}
