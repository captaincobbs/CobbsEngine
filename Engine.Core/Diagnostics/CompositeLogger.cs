using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core.Diagnostics
{
    public class CompositeLogger : ILogger
    {
        private readonly List<ILogger> _loggers = [];

        public int Priority => 1;

        public int Weight => 1;

        public void Initialize(IServiceProvider services)
        {
        }

        public void Shutdown()
        {
        }

        public void Message(string message, string source)
        {
            foreach (ILogger logger in _loggers)
            {
                logger.Message(message, source);
            }
        }

        public void Warning(string message, string source)
        {
            foreach (ILogger logger in _loggers)
            {
                logger.Warning(message, source);
            }
        }

        public void Error(string message, string source)
        {
            foreach (ILogger logger in _loggers)
            {
                logger.Error(message, source);
            }
        }

        public void Exception(Exception ex, string message, string source)
        {
            foreach (ILogger logger in _loggers)
            {
                logger.Exception(ex, message, source);
            }
        }

        public void Debug(string message, string source)
        {
            foreach (ILogger logger in _loggers)
            {
                logger.Debug(message, source);
            }
        }

        public void Assert(bool condition, string message, string source)
        {
            foreach (ILogger logger in _loggers)
            {
                logger.Assert(condition, message, source);
            }
        }
    }
}
