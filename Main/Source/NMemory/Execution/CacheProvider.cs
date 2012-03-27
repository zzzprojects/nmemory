using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Linq;

namespace NMemory.Execution
{
    public class CacheProvider : TableQuery<object[]>
    {
        object[] values;

        public CacheProvider(Database db, object[] values)
            : base(db)
        {
            this.values = values;
        }

        public override IEnumerator<object[]> GetEnumerator()
        {
            yield return this.values;
        }
    }
}
