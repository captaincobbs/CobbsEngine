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

            public void Configure(DiagnosticsSettings options)
            {
                foreach (ILogger logger in _loggers)
                {
                    logger.Configure(options);
                }
            }

            public void Shutdown()
            {
                foreach (ILogger logger in _loggers)
                {
                    logger.Shutdown();
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