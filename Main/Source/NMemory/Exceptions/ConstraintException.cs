using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Exceptions
{
    public class ConstraintException : NMemoryException
    {
        public ConstraintException(string message) : base(ErrorCodes.GenericError, message)
        {

        }
    }
}
