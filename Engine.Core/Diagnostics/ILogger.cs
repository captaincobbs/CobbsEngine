using Engine.Core.Services;

namespace Engine.Core
{
    public static partial class Diagnostics
    {
        // TODO: Configurable log coloring with settings, works for console and debugger, insert HTML color tags for HTMLLogger
        // TODO: Add a log settings to allow configuration of which log types include stack traces, and which log types are logged to which loggers

        internal interface ILogger : IService
        {
            void Message(string message, string source);
            void Warning(string message, string source);
            void Error(string message, string source);
            void Exception(Exception ex, string message, string source);
            void Debug(string message, string source);
            void Assert(bool condition, string message, string source);
            void Assert(Action function, string message, string source);
            void Log(LogMessage log, bool isBufferedReplay = false);
        }
    }
}