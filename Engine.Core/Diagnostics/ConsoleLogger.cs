using Engine.Core.Services.Attributes;

namespace Engine.Core
{
    public static partial class Diagnostics
    {
        [RegisterService(typeof(ILogger), priority: 1)]
        internal class ConsoleLogger : ILogger
        {
            public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
            {
                progress.Report(1f);
                return Task.CompletedTask;
            }

            public void Message(string message, string source)
            {
                Log(new(message, MessageType.Message, source));
            }

            public void Warning(string message, string source)
            {
                Log(new(message, MessageType.Warning, source));
            }

            public void Error(string message, string source)
            {
                Log(new(message, MessageType.Error, source));
            }

            public void Exception(Exception ex, string message, string source)
            {
                Log(new(message, MessageType.Exception, source, ex));
            }

            public void Debug(string message, string source)
            {
                Log(new(message, MessageType.Debug, source));
            }

            public void Assert(bool condition, string message, string source)
            {
                if (!condition)
                {
                    Log(new(message, MessageType.Assert, source));
                }
            }

            public void Assert(Action function, string message, string source)
            {
                try
                {
                    function();
                }
                catch (Exception ex)
                {
                    Log(new(message, MessageType.Assert, source, ex));
                }
            }

            public void Log(LogMessage log, bool isBufferedReplay = false)
            {
                if (isBufferedReplay)
                {
                    return;
                }

                string output = $"[{log.Timestamp:HH:mm:ss:fff}] [{log.Type}] [{log.Source}] {log.Message}";
                if (log.Exception is not null)
                {
                    output += Environment.NewLine + log.Exception;
                }
                Console.WriteLine(output);
            }
        }
    }
}