using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NMemory.Tables
{
    public interface IRelationContraint
    {
        MemberInfo PrimaryField { get; }

        MemberInfo ForeignField { get; }
    }
}
