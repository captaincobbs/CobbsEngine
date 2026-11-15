using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Channels;

using Engine.Core.Services.Attributes;

namespace Engine.Core
{
    public static partial class Diagnostics
    {
        [RegisterService(typeof(ILogger))]
        public class HTMLLogger : ILogger
        {
            private bool _initialized = false;
            private const int MaxLogsPerFile = 1000;
            private const string ContinuationMarker = "{ContinuationNotice}";

            private const string LogMarker = "<!-- LOGS -->";
            private readonly string _gameName;
            private readonly Channel<LogMessage> _channel;

            private string _templateSuffix = string.Empty;
            private string _templatePrefix = string.Empty;
            private string _sessionId = string.Empty;

            private int _partNumber;
            private int _logsInCurrentFile;

            private Task? _worker;
            private StreamWriter? _writer;

            public HTMLLogger(string gameName)
            {
                _gameName = gameName;
                _channel = Channel.CreateUnbounded<LogMessage>(
                    new UnboundedChannelOptions()
                    {
                        SingleReader = true,
                        SingleWriter = false,
                    }
                );
            }

            public async Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
            {
                cancellation.ThrowIfCancellationRequested();
                string template = LoadTemplate();

                template = template.Replace("{GameName}", WebUtility.HtmlEncode(_gameName));
                template = template.Replace("{Version}", WebUtility.HtmlEncode(EngineEnvironment.EngineVersion?.ToString() ?? "null"));
                template = template.Replace("{InitialTime}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                if (!template.Contains(ContinuationMarker, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"The template file is missing the required continuation marker '{ContinuationMarker}'.");
                }

                int markerIndex = template.IndexOf(LogMarker, StringComparison.Ordinal);

                if (markerIndex < 0)
                {
                    throw new InvalidOperationException($"Log marker '{LogMarker}' not found in template.");
                }

                _templatePrefix = template[..markerIndex];
                _templateSuffix = template[(markerIndex + LogMarker.Length)..];

                _sessionId = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss-fff");
                _partNumber = 0;

                await OpenNextFileAsync(isContinuation: false).ConfigureAwait(false);

                _initialized = true;
                _worker = ConsumeLogsAsync();
            }

            private async Task ConsumeLogsAsync()
            {
                try
                {
                    await foreach (LogMessage log in _channel.Reader.ReadAllAsync().ConfigureAwait(false))
                    {
                        if (_logsInCurrentFile >= MaxLogsPerFile)
                        {
                            await CloseCurrentFileAsync().ConfigureAwait(false);
                            await OpenNextFileAsync(isContinuation: true).ConfigureAwait(false);
                        }

                        string html = FormatMessage(log);
                        await _writer!.WriteLineAsync(html).ConfigureAwait(false);
                        _logsInCurrentFile++;
                    }
                }
                catch (Exception ex)
                {
                    _channel.Writer.TryComplete(ex);
                    System.Console.Error.WriteLine($"[HTMLLogger] Background writer failed: {ex}");
                    throw;
                }
            }

            public void Shutdown()
            {
                if (!_initialized)
                {
                    return;
                }

                _initialized = false;
                _channel.Writer.TryComplete();

                try
                {
                    _worker?.GetAwaiter().GetResult();
                }
                finally
                {
                    CloseCurrentFileAsync().GetAwaiter().GetResult();
                }
            }

            public void Log(LogMessage log, bool _ = false)
            {
                if (!_channel.Writer.TryWrite(log))
                {
                    System.Console.Error.WriteLine($"[HTMLLogger] Dropped log after shutdown: {log.Message}");
                }
            }

            private static string FormatMessage(LogMessage log)
            {
                string type = log.Type.ToString().ToLowerInvariant();
                string label = WebUtility.HtmlEncode(log.Type.ToString());
                string source = WebUtility.HtmlEncode(log.Source);
                string message = WebUtility.HtmlEncode(log.Message);

                string callStack = log.CallStack?.ToString() ?? string.Empty;
                string details = callStack;

                if (log.Exception is not null)
                {
                    if (!string.IsNullOrWhiteSpace(details))
                    {
                        details += "\n\n";
                    }

                    details += $"Exception:\n{log.Exception}";
                }

                details = details.TrimEnd('\r', '\n');

                string traceHtml = string.Empty;

                if (!string.IsNullOrWhiteSpace(details))
                {
                    string encodedDetails = WebUtility.HtmlEncode(details);

                    traceHtml =
                        "<button type=\"button\" class=\"trace-toggle\" " +
                        "onclick=\"toggleStackTrace(this)\" " +
                        "aria-expanded=\"false\">TRACE</button>" +
                        $"<pre hidden>{encodedDetails}</pre>";
                }

                return
                    $"<div class=\"{type} log\">" +
                        $"<span class=\"time\">{log.Timestamp:HH:mm:ss.fff}</span>" +
                        $"<span class=\"flags\">{label}</span>" +
                        $"<span class=\"source\">{source}</span>" +
                        traceHtml +
                        $"<span class=\"message\">{message}</span>" +
                    "</div>";
            }

            private static string LoadTemplate()
            {
                Assembly assembly = typeof(HTMLLogger).Assembly;

                using Stream stream = assembly.GetManifestResourceStream("Engine.Core.Diagnostics.HTMLLogger.html") ?? throw new InvalidOperationException("HTML Log template was not found");
                using StreamReader reader = new(stream);
                return reader.ReadToEnd();
            }

            private async Task OpenNextFileAsync(bool isContinuation)
            {
                _partNumber++;
                _logsInCurrentFile = 0;

                string fileName = GetFileName(_sessionId, _partNumber);

                _writer = new StreamWriter(fileName, append: false, encoding: new UTF8Encoding(false));

                string notice = isContinuation ? $"Continued from {GetFileName(_sessionId, _partNumber - 1)}" : string.Empty;

                string prefix = _templatePrefix.Replace(ContinuationMarker, WebUtility.HtmlEncode(notice));

                await _writer.WriteAsync(prefix).ConfigureAwait(false);
                await _writer.FlushAsync().ConfigureAwait(false);
            }

            private async Task CloseCurrentFileAsync()
            {
                if (_writer is null)
                {
                    return;
                }

                await _writer.WriteAsync(_templateSuffix).ConfigureAwait(false);
                await _writer.FlushAsync().ConfigureAwait(false);
                await _writer.DisposeAsync().ConfigureAwait(false);

                _writer = null;
            }

            private static string GetFileName(string sessionId, int partNumber)
            {
                return $"log_{sessionId}_part{partNumber:D3}.html";
            }
        }
    }
}