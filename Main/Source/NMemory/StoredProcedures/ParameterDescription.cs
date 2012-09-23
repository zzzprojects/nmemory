using System;

namespace NMemory.StoredProcedures
{
    public class ParameterDescription
    {
        public ParameterDescription(string name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; private set; }

        public Type Type { get; private set; }
    }
}
