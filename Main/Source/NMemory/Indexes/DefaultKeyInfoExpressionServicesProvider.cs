// ----------------------------------------------------------------------------------
// <copyright file="DefaultKeyInfoExpressionServicesProvider.cs" company="NMemory Team">
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

namespace NMemory.Indexes
{
    using NMemory.Common;
    using NMemory.Exceptions;
    using System;

    public class DefaultKeyInfoExpressionServicesProvider : IKeyInfoExpressionServicesProvider
    {
        private IKeyInfoExpressionServices expressionServices;

        private DefaultKeyInfoExpressionServicesProvider()
        {
        }

        public DefaultKeyInfoExpressionServicesProvider(Type keyType)
        {
            if (keyType == null)
            {
                throw new ArgumentNullException("keyType");
            }

            this.Initialize(keyType);
            
            if (this.expressionServices == null)
            {
                throw new ArgumentException(ExceptionMessages.Missing, "keySelector");
            }
        }

        public static bool TryCreate(
            Type keyType, 
            out DefaultKeyInfoExpressionServicesProvider provider)
        {
            if (keyType == null)
            {
                provider = null;
                return false;
            }

            provider = new DefaultKeyInfoExpressionServicesProvider();
            provider.Initialize(keyType);

            if (provider.expressionServices == null)
            {
                provider = null;
                return false;
            }

            return true;
        }

        private void Initialize(Type keyType)
        {
            if (keyType.IsValueType || (keyType == typeof(string)))
            {
                this.expressionServices = new PrimitiveKeyInfoExpressionServices(keyType);
            }
            else if (ReflectionHelper.IsAnonymousType(keyType))
            {
                this.expressionServices = new AnonymousTypeKeyInfoExpressionServices(keyType);
            }
            else if (ReflectionHelper.IsTuple(keyType))
            {
                this.expressionServices = new TupleKeyInfoExpressionServices(keyType);
            }
        }

        public IKeyInfoExpressionServices KeyInfoExpressionServices
        {
            get 
            {
                return this.expressionServices;
            }
        }
    }
}
