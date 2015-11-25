// ----------------------------------------------------------------------------------
// <copyright file="ExceptionMessages.cs" company="NMemory Team">
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
    public static class ExceptionMessages
    {
        public static readonly string Missing = string.Empty;

        public static readonly string CannotCreateKeyInfo =
            "Cannot create keyinfo.";

        public static readonly string MemberAndSortOrderCountMismatch =
            "The amount of sort ordering values does not match the amount of members.";

        public static readonly string InvalidKeyInfoHelper =
            "Invalid key info helper.";

        public static readonly string GenericKeyInfoCannotBeNull =
            "Generic KeyInfo objects cannot be null.";

        public static readonly string ServiceNotFound =
            "'{0}' service was not found.";

        public static readonly string ServiceCannotBeCombined =
            "'{0}' service cannot be combined.";

        public static readonly string TableNotCreated =
            "Table was not created by the TableFactoryService";

        public static readonly string InvalidKeySelector =
            "Invalid key selector.";

        public static readonly string ForeignKeyViolation =
            "Foreign key violation [{0} :: {1}]. " +
            "The key value [{2}] does not exists in the referenced table [{3} :: {4}].";
    }
}
