using System.Collections.Generic;
using NMemory.Diagnostics.Messages;
using NMemory.Modularity;

namespace NMemory.Diagnostics
{
    public class MessageBuffer : ILoggingPort
    {
        public List<Message> Messages { get; private set; }

        public void Clear()
        {
            this.Messages.Clear();
        }

        public MessageBuffer()
        {
            this.Messages = new List<Message>();
        }

        public void Send(Message msg)
        {
            this.Messages.Add(msg);
        }
    }
}
