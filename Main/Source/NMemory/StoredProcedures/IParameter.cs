using System;

namespace NMemory.StoredProcedures
{
    public interface IParameter
    {
        string Name { get; }

        Type Type { get; }
    }
}
