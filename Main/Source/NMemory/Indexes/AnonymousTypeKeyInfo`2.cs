// ----------------------------------------------------------------------------------
// <copyright file="AnonymousTypeKeyInfo`2.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
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

namespace NMemory.Indexes
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public class AnonymousTypeKeyInfo<TEntity, TKey> : 
        KeyInfoBase<TEntity, TKey>, 
        IKeyInfoHelperProvider
        where TEntity : class
    {
        internal static readonly IKeyInfoHelper KeyInfoHelper = 
            new AnonymousTypeKeyInfoHelper(typeof(TKey));

        public AnonymousTypeKeyInfo(
            Expression<Func<TEntity, TKey>> keySelector, 
            SortOrder[] sortOrders) : 
            base(
                keySelector,
                sortOrders,
                new GenericKeyComparer<TKey>(sortOrders, KeyInfoHelper),
                new AnonymousTypeKeyInfoHelper(typeof(TKey)))
        {
        }

        public AnonymousTypeKeyInfo(
            Expression<Func<TEntity, TKey>> keySelector) :
            this(keySelector, null)
        {
        }

        internal AnonymousTypeKeyInfo(
            MemberInfo[] entityKeyMembers)
            : base(
                entityKeyMembers, 
                null,
                new GenericKeyComparer<TKey>(null, KeyInfoHelper),
                new AnonymousTypeKeyInfoHelper(typeof(TKey)))
        {
        }

        IKeyInfoHelper IKeyInfoHelperProvider.KeyInfoHelper
        {
            get
            {
                return KeyInfoHelper;
            }
        }
    }
}
