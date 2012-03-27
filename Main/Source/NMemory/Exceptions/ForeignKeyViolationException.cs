using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Exceptions
{
	public class ForeignKeyViolationException : NMemoryException
	{
		public ForeignKeyViolationException(string message)
			: base(ErrorCodes.RelationError, message)
		{
		}

		public ForeignKeyViolationException()
			: this(null)
		{

		}


	}
}
