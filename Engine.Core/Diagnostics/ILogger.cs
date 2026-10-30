using Engine.Core.Services;

namespace Engine.Core
{
    public static partial class Diagnostics
    {
        // TODO: Configurable log coloring with settings, works for console and debugger, insert HTML color tags for HTMLLogger
        // TODO: Add a log settings to allow configuration of which log types include stack traces, and which log types are logged to which loggers

        internal interface ILogger : IService
        {
            void Log(LogMessage log, bool isBufferedReplay = false);
        }
    }
}