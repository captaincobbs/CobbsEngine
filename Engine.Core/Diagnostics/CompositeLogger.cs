using Engine.Core.Services.Attributes;

namespace Engine.Core
{
    public static partial class Diagnostics
    {
        [RegisterService(typeof(ILogger))]
        internal class CompositeLogger : ILogger
        {
            private readonly List<ILogger> _loggers = [];

            internal CompositeLogger(params ILogger[] loggers)
            {
                _loggers.AddRange(loggers);
            }

            public async Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
            {
                IEnumerable<Task> initializeTasks = _loggers.Select(logger => logger.InitializeAsync(services, progress, cancellation));
                await Task.WhenAll(initializeTasks);

                progress.Report(1f);
            }

            public void Shutdown()
            {
                foreach (ILogger logger in _loggers)
                {
                    logger.Shutdown();
                }
            }

            public void Message(string message, string source)
            {
                LogMessage log = new(message, MessageType.Message, source);
                foreach (ILogger logger in _loggers)
                {
                    logger.Log(log);
                }
            }

            public void Warning(string message, string source)
            {
                LogMessage log = new(message, MessageType.Warning, source);
                foreach (ILogger logger in _loggers)
                {
                    logger.Log(log);
                }
            }

            public void Error(string message, string source)
            {
                LogMessage log = new(message, MessageType.Error, source);
                foreach (ILogger logger in _loggers)
                {
                    logger.Log(log);
                }
            }

            public void Exception(Exception ex, string message, string source)
            {
                LogMessage log = new(message, MessageType.Exception, source, ex);
                foreach (ILogger logger in _loggers)
                {
                    logger.Log(log);
                }
            }

            public void Debug(string message, string source)
            {
                LogMessage log = new(message, MessageType.Debug, source);
                foreach (ILogger logger in _loggers)
                {
                    logger.Log(log);
                }
            }

            public void Assert(bool condition, string message, string source)
            {
                if (!condition)
                {
                    LogMessage log = new(message, MessageType.Assert, source);
                    foreach (ILogger logger in _loggers)
                    {
                        logger.Log(log);
                    }
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
                    LogMessage log = new(message, MessageType.Assert, source, ex);
                    foreach (ILogger logger in _loggers)
                    {
                        logger.Log(log);
                    }
                }
            }

            public void Log(LogMessage log, bool isBufferedReplay = false)
            {
                foreach (ILogger logger in _loggers)
                {
                    logger.Log(log, isBufferedReplay);
                }
            }
        }
    }
}