using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.StoredProcedures;
using NMemory.Common;

namespace NMemory.Indexes
{
	public class SimpleKeyInfo<TEntity, TKey> : IKeyInfo<TEntity, TKey>
	{
		private Func<TEntity, TKey> keyAccess;
		private MemberInfo memberInfo;

		public SimpleKeyInfo(Expression<Func<TEntity, TKey>> expression)
		{
            this.keyAccess = expression.Compile();

            MemberExpression member = ExpressionHelper.FindMemberExpression(expression.Body);

            if (member == null)
            {
                throw new ArgumentException();
            }

            this.memberInfo = member.Member;
		}

		public MemberInfo[] KeyMembers
		{
			get { return new MemberInfo[] { memberInfo }; }
		}

		public Type KeyType
		{
			get { return typeof(TKey); }
		}

        public TKey GetKey(TEntity entity)
        {
            return this.keyAccess.Invoke(entity);
        }
    }
}
