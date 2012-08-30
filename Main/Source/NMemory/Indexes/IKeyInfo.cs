using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace NMemory.Indexes
{
	public interface IKeyInfo
	{
        MemberInfo[] EntityKeyMembers { get; }

		Type KeyType { get; }
	}
}
