using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Exceptions
{
    public class NMemoryException : Exception
    {
		private string message;

        public virtual ErrorCodes ErrorCode
        {
            get;
            protected set;
        }

        public NMemoryException()
        {
			this.ErrorCode = ErrorCodes.GenericError;
        }

		public NMemoryException(ErrorCodes errorCode, string message) : base(message, null)
		{
			this.message = message;
		}

        public NMemoryException(ErrorCodes errorCode, Exception innerException)
            : base(null, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public override string Message
        {
            get
            {
				if(string.IsNullOrEmpty(message))
				{
					return string.Format("Error code: {0}", ErrorCode);
				}
				else
				{
					return string.Format("{1}. Error code: {0}", ErrorCode, message);
				}
            }
        }
    }
}
