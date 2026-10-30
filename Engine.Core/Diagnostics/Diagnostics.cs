using System.Diagnostics;

using Engine.Core.Collections;

namespace Engine.Core
{

    public static partial class Diagnostics
    {
        private static readonly object _lock = new();
        private static readonly CircularBuffer<LogMessage> _buffer = new(1000);

        // Determines which types are mirrored to the console while buffering
        private static HashSet<MessageType> MirroredTypes = [
            MessageType.Warning,
            MessageType.Error,
            MessageType.Exception,
        ];

        private static ILogger? _logger;
        private static DiagnosticsState _state = DiagnosticsState.Buffering;

        public static bool Initialized
        {
            get
            {
                lock (_lock)
                {
                    return _state == DiagnosticsState.Buffering;
                }
            }
        }

        public static IReadOnlySet<MessageType> MirrorBuffered
        {
            get
            {
                lock (_lock)
                {
                    return new HashSet<MessageType>(MirroredTypes);
                }
            }
        }

        internal static void Configure(DiagnosticsOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            MessageType[] mirroredTypes = [.. options.MirroredTypes];

            lock (_lock)
            {
                if (_state != DiagnosticsState.Buffering)
                {
                    throw new InvalidOperationException("[Diagnostics] Can only call Configure() before initialization!");
                }

                MirroredTypes.Clear();

                foreach (MessageType type in mirroredTypes)
                {
                    MirroredTypes.Add(type);
                }
            }
        }

        internal static void Initialize(ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(logger);

            lock (_lock)
            {
                if (_state != DiagnosticsState.Buffering)
                {
                    throw new InvalidOperationException($"[Diagnostics] Cannot initialize while in {_state}");
                }

                _logger = logger;
                _state = DiagnosticsState.Active;
            }

            Flush(logger);
        }

        internal static void Shutdown()
        {
            lock (_lock)
            {
                _logger = null;
                _state = DiagnosticsState.Buffering;
            }
        }

        public static void Message(string message, string source) => Submit(new LogMessage(message, MessageType.Message, source));

        public static void Warning(string message, string source) => Submit(new LogMessage(message, MessageType.Warning, source));

        public static void Error(string message, string source) => Submit(new LogMessage(message, MessageType.Error, source));

        public static void Exception(Exception ex, string message, string source) => Submit(new LogMessage(message, MessageType.Exception, source, ex));

        public static void Debug(string message, string source) => Submit(new LogMessage(message, MessageType.Debug, source));

        public static void Assert(bool condition, string message, string source)
        {
            if (!condition)
            {
                Submit(new LogMessage(message, MessageType.Assert, source));
            }
        }

        public static void Assert(Action function, string message, string source)
        {
            try
            {
                function.Invoke();
            }
            catch (Exception ex)
            {
                Submit(new LogMessage(message, MessageType.Assert, source, ex));
            }
        }

        public static void Time(Action action, string functionName)
        {
            ArgumentNullException.ThrowIfNull(action);
            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                action();
            }
            catch (Exception ex)
            {
                Exception(ex, $"Uncaught Exception when timing '{functionName}'", nameof(Diagnostics));
                return;
            }
            finally
            {
                watch.Stop();
            }

            Debug($"{functionName} took {watch.ElapsedMilliseconds}", "Diagnostics");
        }

        private static void Submit(LogMessage message)
        {
            ILogger? logger = null;
            bool mirrorToConsole = false;

            lock (_lock)
            {
                if (_state == DiagnosticsState.Active && _logger is not null)
                {
                    logger = _logger;
                }
                else
                {
                    _buffer.Add(message);
                }
            }

            if (mirrorToConsole)
            {
                WriteConsoleMessage(message);
            }

            logger?.Log(message, isBufferedReplay: false);
        }

        private static void Flush(ILogger logger)
        {
            while (true)
            {
                LogMessage[] batch;

                lock (_lock)
                {
                    if (_state != DiagnosticsState.Flushing || !ReferenceEquals(_logger, logger))
                    {
                        return;
                    }

                    if (_buffer.Count == 0)
                    {
                        _state = DiagnosticsState.Active;
                        return;
                    }

                    batch = _buffer.DrainToArray();
                }

                try
                {
                    foreach (LogMessage message in batch)
                    {
                        logger.Log(message, isBufferedReplay: true);
                    }
                }
                catch
                {
                    lock (_lock)
                    {
                        if (_state == DiagnosticsState.Flushing && ReferenceEquals(_logger, logger))
                        {
                            RestoreBatchAtFront(batch);
                            _logger = null;
                            _state = DiagnosticsState.Buffering;
                        }
                    }

                    throw;
                }
            }
        }

        private static void RestoreBatchAtFront(LogMessage[] batch)
        {
            LogMessage[] bufferedDuringFlush = _buffer.DrainToArray();

            foreach (LogMessage message in batch)
            {
                _buffer.Add(message);
            }

            foreach (LogMessage message in bufferedDuringFlush)
            {
                _buffer.Add(message);
            }
        }

        private static void WriteConsoleMessage(LogMessage message)
        {
            string output =
                $"[{message.Timestamp:HH:mm:ss.fff}] " +
                $"[{message.Type}] " +
                $"[{message.Source}] " +
                message.Message;

            if (message.Exception is not null)
            {
                output += Environment.NewLine + message.Exception;
            }

            Console.Error.WriteLine(output);
        }

        private enum DiagnosticsState
        {
            Buffering,
            Flushing,
            Active,
        }
    }
}