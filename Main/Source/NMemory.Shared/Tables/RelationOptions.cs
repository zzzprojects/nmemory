// -----------------------------------------------------------------------------------
// <copyright file="RelationOptions.cs" company="NMemory Team">
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
// -----------------------------------------------------------------------------------


namespace NMemory.Tables
{
    /// <summary>
    ///     Represents options of a relation.
    /// </summary>
    public class RelationOptions
    {
        private readonly bool cascadedDeletion;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationOptions"/> class.
        /// </summary>
        /// <param name="cascadedDeletion">
        ///     if set to <c>true</c> cascaded deletion will be enabled
        /// </param>
        public RelationOptions(bool cascadedDeletion = false)
        {
            this.cascadedDeletion = cascadedDeletion;
        }

        /// <summary>
        ///     Gets a value that indicates whether the deletion of a referred (primary) entity
        ///     should result in the deletion of the referring (foreign) entities.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool CascadedDeletion
        {
            get
            {
                return this.cascadedDeletion;
            }
        }
    }
}
