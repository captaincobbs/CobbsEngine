using Engine.Core.Services;

namespace Engine.Core.Diagnostics
{
    public interface ILogger : IService
    {
        void Message(string message, string source);
        void Warning(string message, string source);
        void Error(string message, string source);
        void Exception(Exception ex, string message, string source);
        void Debug(string message, string source);
        void Assert(bool condition, string message, string source);
    }
}
