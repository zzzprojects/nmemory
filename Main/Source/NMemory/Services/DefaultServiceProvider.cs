// ----------------------------------------------------------------------------------
// <copyright file="DefaultServiceProvider.cs" company="NMemory Team">
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

namespace NMemory.Services
{
    using System;
    using System.Collections.Generic;
    using NMemory.Modularity;

    public class DefaultServiceProvider : 
        NMemory.Modularity.IServiceProvider,
        IDatabaseComponent
    {
        private readonly Dictionary<Type, object> services;
        private IDatabase database;

        public DefaultServiceProvider()
        {
            this.services = new Dictionary<Type, object>();
        }

        public T GetService<T>() where T : class
        {
            object result;

            if (!this.services.TryGetValue(typeof(T), out result))
            {
                return null;
            }

            return (T)result;
        }

        public void Initialize(IDatabase database)
        {
            this.database = database;
            this.RegisterServices();
        }

        protected virtual void RegisterServices()
        {
        }

        protected void Replace<T>(T service)
        {
            this.services[typeof(T)] = service;
        }

        protected void Add<T>(T service)
        {
            this.services.Add(typeof(T), service);
        }

        protected bool Remove<T>()
        {
            return this.services.Remove(typeof(T));
        }
    }
}
