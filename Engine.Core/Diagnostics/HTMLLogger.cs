using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Diagnostics
{
    public class HTMLLogger : ILogger
    {
        public int LoadingWeight => 1;

        public async Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
        {

        }

        public void Shutdown()
        {
        }

        public void Message(string message, string source)
        {
            Log(message, MessageType.Message, source);
        }

        public void Warning(string message, string source)
        {
            Log(message, MessageType.Warning, source);
        }

        public void Error(string message, string source)
        {
            Log(message, MessageType.Error, source);
        }

        public void Exception(Exception ex, string message, string source)
        {
            Log(message, MessageType.Exception, source, ex);
        }

        public void Debug(string message, string source)
        {
            Log(message, MessageType.Debug, source);
        }

        public void Assert(bool condition, string message, string source)
        {
            if (condition)
            {
                Log(message, MessageType.Assert, source);
            }
        }

        private static void Log(string message, MessageType type, string source, Exception? ex = null)
        {

        }
    }
}
