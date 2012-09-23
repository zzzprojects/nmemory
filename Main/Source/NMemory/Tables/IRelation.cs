using System.Collections.Generic;
using NMemory.Indexes;

namespace NMemory.Tables
{
    internal interface IRelation
    {
        ITable PrimaryTable { get; }

        ITable ForeignTable { get; }

        IIndex PrimaryIndex { get; }

        IIndex ForeignIndex { get; }

        void ValidateEntity(object foreign);

        IEnumerable<object> GetReferringEntities(object primary);

        IEnumerable<object> GetReferredEntities(object foreign);
    }
}
