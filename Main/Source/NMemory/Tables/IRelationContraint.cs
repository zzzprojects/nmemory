using System.Reflection;

namespace NMemory.Tables
{
    public interface IRelationContraint
    {
        MemberInfo PrimaryField { get; }

        MemberInfo ForeignField { get; }
    }
}
