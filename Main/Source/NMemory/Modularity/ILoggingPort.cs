using NMemory.Diagnostics.Messages;

namespace NMemory.Modularity
{
    public interface ILoggingPort
    {
        void Send(Message msg);
    }
}
