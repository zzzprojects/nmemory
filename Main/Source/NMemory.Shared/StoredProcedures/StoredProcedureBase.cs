// ----------------------------------------------------------------------------------
// <copyright file="StoredProcedureBase.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.StoredProcedures
{
    using System.Collections.Generic;
    using NMemory.Common;
    using NMemory.Exceptions;

    public class StoredProcedureBase
    {
        private List<ParameterDescription> parameters;
        private IList<ParameterDescription> protectedParameters;

        public StoredProcedureBase()
        {
            this.parameters = new List<ParameterDescription>();
            this.protectedParameters = this.parameters.AsReadOnly();
        }

        public IList<ParameterDescription> Parameters
        {
            get
            {
                return this.protectedParameters;
            }
        }

        protected void SetParameters(IEnumerable<ParameterDescription> parameters) 
        {
            this.parameters.Clear();
            this.parameters.AddRange(parameters);
        }

        protected void VerifyParameters(IDictionary<string, object> parameters)
        {
            for (int i = 0; i < this.Parameters.Count; i++)
            {
                ParameterDescription parameter = this.Parameters[i];
                object value = null;

                if (parameters.TryGetValue(parameter.Name, out value))
                {
                    VerifyParameterValue(parameter, value);
                }
                else
                {
                    // Parameter is missing
                    throw new ParameterException();
                }
            }
        }

        private static void VerifyParameterValue(ParameterDescription parameter, object value)
        {
            if (value == null)
            {
                if (!ReflectionHelper.IsNullable(parameter.Type))
                {
                    // Value is null, but the type is not nullable
                    throw new ParameterException();
                }
            }
            else
            {
                if (!ReflectionHelper.IsCastableTo(
                    value.GetType(),
                    parameter.Type))
                {
                    // Incompatible type
                    throw new ParameterException();
                }
            }
        }
    }
}
