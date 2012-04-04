using System;

namespace NMemory.StoredProcedures
{
    public class Parameter<T> : IParameter
    {
        private string name;

        public Parameter(string name)
        {
            this.name = name;
        }

        public static implicit operator T(Parameter<T> parameter)
        {
            return default(T);
        }

        public string Name
        {
            get { return this.name; }
        }

        public Type Type
        {
            get { return typeof(T); }
        }
    }
}
