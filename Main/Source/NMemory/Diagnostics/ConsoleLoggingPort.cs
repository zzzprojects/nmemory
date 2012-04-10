using System;
using NMemory.Diagnostics.Messages;
using NMemory.Modularity;

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
