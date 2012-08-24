using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NMemory.Tables
{
    public interface IRelationContraint
    {
        PropertyInfo PrimaryField { get; }

        PropertyInfo ForeignField { get; }
    }
}
