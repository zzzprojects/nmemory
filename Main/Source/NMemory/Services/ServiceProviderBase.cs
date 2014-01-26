// ----------------------------------------------------------------------------------
// <copyright file="ServiceProviderBase.cs" company="NMemory Team">
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

namespace NMemory.Services
{
    using System;
    using System.Collections.Generic;
    using NMemory.Exceptions;
    using NMemory.Modularity;
    using NMemory.Services.Contracts;

    public abstract class ServiceProviderBase : 
        NMemory.Modularity.IServiceProvider,
        IDatabaseComponent
    {
        private readonly Dictionary<Type, object> services;
        private IDatabase database;

        public ServiceProviderBase()
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
        }

        protected void Replace<T>(T service) where T : class
        {
            this.services[typeof(T)] = service;
        }

        protected void Add<T>(T service) where T : class
        {
            this.services.Add(typeof(T), service);
        }

        protected bool Remove<T>() where T : class
        {
            return this.services.Remove(typeof(T));
        }

        protected void Combine<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException();
            }

            T existing = this.GetService<T>();

            if (existing == null)
            {
                this.Add<T>(service);
            }
            else
            {
                T combined = Combine(existing, service);
                this.Replace<T>(combined);
            }
        }

        protected void Combine<T>(params T[] services) where T : class
        {
            foreach (T service in services)
            {
                this.Combine(service);
            }
        }

        private T Combine<T>(T existing, T addition) where T : class
        {
            Type serviceType = typeof(T);

            if (serviceType == typeof(IKeyInfoService))
            {
                return Combine(
                    existing as IKeyInfoService, 
                    addition as IKeyInfoService) as T;
            }

            throw new NMemoryException(ExceptionMessages.ServiceCannotBeCombined, typeof(T).Name);
        }

        private IKeyInfoService Combine(
            IKeyInfoService existing, 
            IKeyInfoService addition)
        {
            var combined = existing as CombinedKeyInfoService;

            if (combined == null)
            {
                combined = CombinedKeyInfoService.Empty.Add(existing);
            }

            return combined.Add(addition);
        }
    }
}
