using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;

namespace NMemory.Diagnostics.Messages
{
    public class PreferredIndexMessage : Message
    {
        public IIndex OldIndex { get; set; }
        public IIndex NewIndex { get; set; }

        public override string ToString()
        {
            return string.Format("*********\n{0}\nreplaced by \n{1}\nbecause of better selectivity\n*********\n",
                                                   this.OldIndex, this.NewIndex);
        }
    }
}
