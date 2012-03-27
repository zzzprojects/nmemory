using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Concurrency.Locks
{
    public interface ILock
    {
        void EnterRead();

        void EnterWrite();
        
        void ExitRead();        

        void ExitWrite();


        void Upgrade();

        void Downgrade();


    }
}
