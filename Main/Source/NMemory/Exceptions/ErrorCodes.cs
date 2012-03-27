using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Exceptions
{
	/// <summary>
	/// Error codes
	/// </summary>
    public enum ErrorCodes
    {
        GenericError,

        ExistingKeyFound,

        TranactionHasAlreadyStarted,

        RelationError
    }
}
