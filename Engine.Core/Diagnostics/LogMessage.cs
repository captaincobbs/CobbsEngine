using System.Diagnostics;

namespace Engine.Core
{
    public static partial class Diagnostics
    {
        public readonly struct LogMessage
        {
            public string Message { get; }
            public MessageType Type { get; }
            public string Source { get; }
            public Exception? Exception { get; }
            public DateTime Timestamp { get; }
            public StackTrace? CallStack { get; }

            public LogMessage(string message, MessageType type, string source, Exception? exception = null)
            {
                Message = message;
                Type = type;
                Source = source;
                Exception = exception;
                Timestamp = DateTime.UtcNow;

                if (exception is not null || type is MessageType.Error or MessageType.Assert)
                {
                    CallStack = new(skipFrames: 2, fNeedFileInfo: true);
                }
            }
        }
    }
}