// ----------------------------------------------------------------------------------
// <copyright file="NMemoryException.cs" company="NMemory Team">
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

namespace NMemory.Exceptions
{
    using System;

    public class NMemoryException : Exception
    {
        private string message;
        private ErrorCode errorCode;

        public NMemoryException()
        {
            this.errorCode = ErrorCode.GenericError;
        }

        public NMemoryException(ErrorCode errorCode, string message) 
            : base(message, null)
        {
            this.errorCode = errorCode;
            this.message = message;
        }

        public NMemoryException(ErrorCode errorCode, string message, params object[] args)
            : this(errorCode, string.Format(message, args))
        {
        }

        public NMemoryException(string message, params object[] args)
            : this(ErrorCode.GenericError, string.Format(message, args))
        {
        }

        public NMemoryException(string message)
            : this(ErrorCode.GenericError, message)
        {
        }

        public NMemoryException(ErrorCode errorCode, Exception innerException)
            : base(null, innerException)
        {
            this.errorCode = errorCode;
        }

        public ErrorCode ErrorCode
        {
            get { return this.errorCode; }
        }

        public override string Message
        {
            get
            {
                if (string.IsNullOrEmpty(this.message))
                {
                    return string.Format("Error code: {0}", ErrorCode);
                }
                else
                {
                    return string.Format("{1}. Error code: {0}", ErrorCode, this.message);
                }
            }
        }
    }
}
