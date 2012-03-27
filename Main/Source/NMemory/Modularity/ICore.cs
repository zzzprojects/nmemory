using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;

namespace NMemory.Modularity
{
    /// <summary>
    /// Provides essential functionality for the database engine.
    /// </summary>
    public interface ICore : IDatabaseComponent
    {
        void RegisterEntityType<T>();

        void OnTableCreated(ITable table);

        T CreateEntity<T>();
    }
}
