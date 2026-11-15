using Engine.Core.Services.Attributes;

namespace Engine.Core
{
    public static partial class Diagnostics
    {
        [RegisterService(typeof(ILogger), priority: 1)]
        internal class ConsoleLogger : ILogger
        {


            public void Configure(DiagnosticsSettings options)
            {

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