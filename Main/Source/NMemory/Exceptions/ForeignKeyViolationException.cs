// ----------------------------------------------------------------------------------
// <copyright file="ForeignKeyViolationException.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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
    using System.Linq;
    using NMemory.Indexes;

    public class ForeignKeyViolationException : NMemoryException
    {
        public ForeignKeyViolationException(
            IIndex primaryIndex, 
            IIndex foreignIndex, 
            object primaryKey)
            : base(
                ErrorCode.RelationError, 
                GetMessage(primaryIndex, foreignIndex, primaryKey))
        {
        }

        private static string GetMessage(
            IIndex primaryIndex, 
            IIndex foreignIndex,
            object primaryKey)
        {
            string foreignTable = 
                foreignIndex.Table.EntityType.Name;

            string foreignColumns = 
                string.Join(", ", foreignIndex.KeyInfo.EntityKeyMembers.Select(m => m.Name));

            string primaryTable = 
                primaryIndex.Table.EntityType.Name;

            string primaryColumns =
                string.Join(", ", primaryIndex.KeyInfo.EntityKeyMembers.Select(m => m.Name));

            return string.Format(
                ExceptionMessages.ForeignKeyViolation,
                foreignTable,
                foreignColumns,
                primaryKey,
                primaryTable,
                primaryColumns);

        }
    }
}
