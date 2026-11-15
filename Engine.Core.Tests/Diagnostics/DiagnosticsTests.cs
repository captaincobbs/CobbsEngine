using CoreDiagnostics = global::Engine.Core.Diagnostics;
using LogMessage = global::Engine.Core.Diagnostics.LogMessage;
using MessageType = global::Engine.Core.Diagnostics.MessageType;

using FluentAssertions;

namespace Engine.Core.Tests.Diagnostics;

[Collection("Diagnostics")]
public sealed class DiagnosticsTests : IDisposable
{
    public DiagnosticsTests()
    {
        CoreDiagnostics.Reset();
    }

    public void Dispose()
    {
        CoreDiagnostics.Reset();
    }

    [Fact]
    public void Initialize_Replays_Buffered_Messages_In_Order()
    {
        CoreDiagnostics.Message("first", "test");
        CoreDiagnostics.Warning("second", "test");

        RecordingLogger logger = new();

        CoreDiagnostics.Initialize(logger);

        logger.Entries.Should().SatisfyRespectively(
            entry =>
            {
                entry.Message.Message.Should().Be("first");
                entry.IsBufferedReplay.Should().BeTrue();
            },
            entry =>
            {
                entry.Message.Message.Should().Be("second");
                entry.IsBufferedReplay.Should().BeTrue();
            });

        CoreDiagnostics.Initialized.Should().BeTrue();
    }

    [Fact]
    public void Message_After_Initialization_Is_Not_A_Buffered_Replay()
    {
        RecordingLogger logger = new();
        CoreDiagnostics.Initialize(logger);

        CoreDiagnostics.Message("live", "test");

        logger.Entries.Should().ContainSingle();
        logger.Entries[0].Message.Message.Should().Be("live");
        logger.Entries[0].IsBufferedReplay.Should().BeFalse();
    }

    [Fact]
    public void Warning_Before_Initialization_Is_Mirrored_And_Buffered()
    {
        DiagnosticsOptions options = new();
        options.SetMirroredMessageTypes([MessageType.Warning]);
        CoreDiagnostics.Configure(options, Path.GetTempPath());

        StringWriter consoleOutput = new();
        TextWriter originalError = Console.Error;

        try
        {
            Console.SetError(consoleOutput);

            CoreDiagnostics.Warning("boot warning", "test");
        }
        finally
        {
            Console.SetError(originalError);
        }

        RecordingLogger logger = new();
        CoreDiagnostics.Initialize(logger);

        consoleOutput.ToString().Should().Contain("boot warning");
        logger.Entries.Should().ContainSingle(entry =>
            entry.Message.Message == "boot warning" &&
            entry.IsBufferedReplay);
    }

    [Fact]
    public async Task Message_Written_While_Flushing_Is_Eventually_Delivered()
    {
        CoreDiagnostics.Message("before flush", "test");

        BlockingReplayLogger logger = new();
        Task initialization = Task.Run(() => CoreDiagnostics.Initialize(logger));

        await logger.ReplayStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));

        CoreDiagnostics.Message("during flush", "test");

        logger.AllowReplay.TrySetResult();
        await initialization.WaitAsync(TimeSpan.FromSeconds(1));

        logger.Entries.Select(entry => entry.Message.Message)
            .Should()
            .ContainInOrder("before flush", "during flush");
    }

    [Fact]
    public void Shutdown_Can_Be_Called_More_Than_Once()
    {
        CoreDiagnostics.Initialize(new RecordingLogger());

        Action shutdownTwice = () =>
        {
            CoreDiagnostics.Shutdown();
            CoreDiagnostics.Shutdown();
        };

        shutdownTwice.Should().NotThrow();
        CoreDiagnostics.Initialized.Should().BeFalse();
    }

    private sealed class RecordingLogger : CoreDiagnostics.ILogger
    {
        private readonly List<LogEntry> _entries = [];

        public IReadOnlyList<LogEntry> Entries => _entries;

        public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
            => Task.CompletedTask;

        public void Shutdown()
        {
        }

        public void Log(LogMessage log, bool isBufferedReplay = false)
        {
            _entries.Add(new LogEntry(log, isBufferedReplay));
        }
    }

    private sealed class BlockingReplayLogger : CoreDiagnostics.ILogger
    {
        private readonly List<LogEntry> _entries = [];
        private int _hasBlocked;

        public TaskCompletionSource ReplayStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource AllowReplay { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public IReadOnlyList<LogEntry> Entries => _entries;

        public Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
            => Task.CompletedTask;

        public void Shutdown()
        {
        }

        public void Log(LogMessage log, bool isBufferedReplay = false)
        {
            if (isBufferedReplay && Interlocked.Exchange(ref _hasBlocked, 1) == 0)
            {
                ReplayStarted.TrySetResult();
                AllowReplay.Task.GetAwaiter().GetResult();
            }

            _entries.Add(new LogEntry(log, isBufferedReplay));
        }
    }

    private sealed record LogEntry(LogMessage Message, bool IsBufferedReplay);
}

[CollectionDefinition("Diagnostics", DisableParallelization = true)]
public sealed class DiagnosticsCollection;
