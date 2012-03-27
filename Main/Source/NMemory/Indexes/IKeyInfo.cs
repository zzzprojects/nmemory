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
		MemberInfo[] KeyMembers { get; }

		Type KeyType { get; }
	}
}
