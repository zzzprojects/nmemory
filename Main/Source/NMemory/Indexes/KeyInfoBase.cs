// ----------------------------------------------------------------------------------
// <copyright file="KeyInfoBase.cs" company="NMemory Team">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Exceptions;

    public abstract class KeyInfoBase<TEntity, TKey> : 
        IKeyInfo<TEntity, TKey>
        where TEntity : class
    {
        private MemberInfo[] entityKeyMembers;
        private SortOrder[] sortOrders;

        private IComparer<TKey> keyComparer;
        private Func<TEntity, TKey> keySelector;
        private Func<TKey, bool> keyEmptinessDetector;

        public KeyInfoBase(
            MemberInfo[] entityKeyMembers,
            SortOrder[] sortOrders,
            IComparer<TKey> keyComparer,
            IKeyInfoHelper helper)
        {
           
            this.entityKeyMembers = entityKeyMembers;

            if (sortOrders == null)
            {
                sortOrders = Enumerable
                    .Repeat(SortOrder.Ascending, helper.GetMemberCount())
                    .ToArray();
            }

            this.sortOrders = sortOrders;

            this.keyComparer = keyComparer;
            this.keySelector = KeyExpressionHelper
                .CreateKeySelector<TEntity, TKey>(entityKeyMembers, helper)
                .Compile();
            this.keyEmptinessDetector = KeyExpressionHelper
                .CreateKeyEmptinessDetector<TEntity, TKey>(helper)
                .Compile();
        }

        public KeyInfoBase(
            Expression<Func<TEntity, TKey>> keySelector,
            SortOrder[] sortOrders,
            IComparer<TKey> keyComparer,
            IKeyInfoHelper helper)
            : this(ParseSelector(keySelector, helper), sortOrders, keyComparer, helper)
        {

        }

        public TKey SelectKey(TEntity entity)
        {
            return this.keySelector.Invoke(entity);
        }

        public bool IsEmptyKey(TKey key)
        {
            return this.keyEmptinessDetector.Invoke(key);
        }

        public IComparer<TKey> KeyComparer
        {
            get { return this.keyComparer; }
        }

        public MemberInfo[] EntityKeyMembers
        {
            get { return this.entityKeyMembers.ToArray(); }
        }

        public SortOrder[] SortOrders
        {
            get { return this.sortOrders.ToArray(); }
        }

        public Type KeyType
        {
            get { return typeof(TKey); }
        }

        private static SortOrder[] GetDefaultSortOrders(int count)
        {
            return Enumerable.Repeat(SortOrder.Ascending, count).ToArray();
        }

        private static MemberInfo[] ParseSelector(
            Expression<Func<TEntity, TKey>> selector,
            IKeyInfoHelper helper)
        {
            MemberInfo[] result = null;

            if (!helper.TryParseKeySelectorExpression(
                selector.Body,
                true, 
                out result))
            {
                throw new ArgumentException(
                    ExceptionMessages.InvalidKeySelector, 
                    "keySelector");
            }

            return result;
        }
    }
}
