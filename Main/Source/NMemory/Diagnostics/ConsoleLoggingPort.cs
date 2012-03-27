using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Diagnostics.Messages;

namespace NMemory.Diagnostics
{
    public class ConsoleLoggingPort : ILoggingPort
    {
        public void Send(Message msg)
        {
            Console.WriteLine(msg.ToString());
        }
    }
}
