using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Common
{
    /// <summary>
    /// Egyszerű objektumpár
    /// </summary>
    public class MutableTuple<TFirst, TSecond>
    {
        public TFirst First { get; set; }
        public TSecond Second { get; set; }
        public MutableTuple(TFirst first, TSecond second)
        {
            this.First = first;
            this.Second = second;
        }

        public MutableTuple()
        {

        }
    }
}
