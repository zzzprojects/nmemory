// ----------------------------------------------------------------------------------
// <copyright file="CombinedKeyInfoService" company="NMemory Team">
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

namespace NMemory.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using NMemory.Indexes;
    using NMemory.Services.Contracts;

    internal sealed class CombinedKeyInfoService : IKeyInfoService
    {
        private readonly IKeyInfoService[] factories;

        private CombinedKeyInfoService()
        {
            this.factories = new IKeyInfoService[0];
        }

        private CombinedKeyInfoService(
            CombinedKeyInfoService existing,
            IKeyInfoService addition)
        {
            int length = existing.factories.Length;

            this.factories = new IKeyInfoService[length + 1];

            Array.Copy(existing.factories, this.factories, length);
            this.factories[length] = addition;
        }

        public static CombinedKeyInfoService Empty
        {
            get 
            { 
                return new CombinedKeyInfoService(); 
            }
        }

        public bool TryCreateKeyInfo<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector, 
            out IKeyInfo<TEntity, TKey> result) where TEntity : class
        {
            result = null;
            bool success = false;

            foreach (IKeyInfoService factory in this.factories)
            {
                if (factory.TryCreateKeyInfo(keySelector, out result))
                {
                    success = true;
                    break;
                }
            }

            return success;
        }

        public bool TryCreateKeyInfoHelper(
            Type keyType,
            out IKeyInfoHelper result)
        {
            result = null;
            bool success = false;

            foreach (IKeyInfoService factory in this.factories)
            {
                if (factory.TryCreateKeyInfoHelper(keyType, out result))
                {
                    success = true;
                    break;
                }
            }

            return success;
        }

        public CombinedKeyInfoService Add(IKeyInfoService factory)
        {
            return new CombinedKeyInfoService(this, factory);
        }
    }
}
