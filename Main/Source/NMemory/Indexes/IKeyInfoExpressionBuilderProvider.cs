using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Indexes
{
    public interface IKeyInfoExpressionBuilderProvider
    {
        IKeyInfoExpressionBuilder KeyInfoExpressionBuilder { get; }
    }
}
