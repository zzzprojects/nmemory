using NMemory.Diagnostics.Messages;

namespace NMemory.Diagnostics
{
    public interface ILoggingPort
    {
        void Send(Message msg);
    }
}
