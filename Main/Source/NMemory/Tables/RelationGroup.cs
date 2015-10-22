// ----------------------------------------------------------------------------------
// <copyright file="RelationGroup.cs" company="NMemory Team">
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

namespace NMemory.Tables
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class RelationGroup
    {
        private readonly List<IRelationInternal> referring;
        private readonly List<IRelationInternal> referred;
        private readonly ReadOnlyCollection<IRelationInternal> referringReadOnly;
        private readonly ReadOnlyCollection<IRelationInternal> referredReadOnly;

        public RelationGroup()
        {
            this.referred = new List<IRelationInternal>();
            this.referring = new List<IRelationInternal>();

            this.referredReadOnly = new ReadOnlyCollection<IRelationInternal>(this.referred);
            this.referringReadOnly = new ReadOnlyCollection<IRelationInternal>(this.referring);
        }

        public IList<IRelationInternal> Referring 
        {
            get { return this.referring; }
        }

        public IList<IRelationInternal> Referred
        {
            get { return this.referred; }
        }

        public ReadOnlyCollection<IRelationInternal> ReferringReadOnly
        {
            get { return this.referringReadOnly; }
        }

        public ReadOnlyCollection<IRelationInternal> ReferredReadOnly
        {
            get { return this.referredReadOnly; }
        }
    }
}
