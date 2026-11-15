using System.Reflection;

using Engine.Core.Services;
using Engine.Core.Settings;

using ILogger = Engine.Core.Diagnostics.ILogger;

namespace Engine.Core
{
    public sealed class EngineHost : IDisposable
    {
        public LifecycleState State { get; private set; }

        public readonly ServiceRegistry ServiceRegistry;
        public readonly ServiceManager ServiceManager;
        public readonly ISettingsManager Settings;
        public readonly FilePaths Paths;
        public readonly AssemblyMetadata Metadata;


        public IReadOnlyList<string> UnclaimedLaunchArguments { get; }
        public IReadOnlyList<SettingsOverride> LaunchSettingsOverrides { get; }

        private CancellationTokenSource? _tokenSource;
        private readonly Lock _stateLock = new();

        internal EngineHost(ServiceRegistry registry, ISettingsManager settings, IReadOnlyList<SettingsOverride> launchSettingsOverrides, IReadOnlyList<string> unclaimedArguments, AssemblyMetadata metadata, FilePaths paths)
        {
            lock (_stateLock)
            {
                State = LifecycleState.NotStarted;
            }
            ServiceRegistry = registry;
            ServiceManager = registry.ServiceManager;
            Settings = settings;
            LaunchSettingsOverrides = launchSettingsOverrides;
            UnclaimedLaunchArguments = unclaimedArguments;
            Metadata = metadata;
            Paths = paths;
        }

        // Progress should be passed in here probably? So the game can control the loading screen
        public async Task InitializeAsync(CancellationToken cancellation)
        {
            CancellationTokenSource tokenSource;

            lock (_stateLock)
            {
                if (State != LifecycleState.NotStarted)
                {
                    throw new InvalidOperationException("Engine host has already been initialized!");
                }

                State = LifecycleState.Initializing;
                _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
                tokenSource = _tokenSource;
            }

            try
            {
                await Settings.LoadAsync(LaunchSettingsOverrides, tokenSource.Token);
                Settings.AdvanceTo(new(SettingsApplyPhase.Engine, ServiceManager));

                EngineSettings engineSettings = Settings.Get<EngineSettings>(EngineSettingsSection.SectionKey);
                Diagnostics.Configure(
                    engineSettings.Diagnostics,
                    engineSettings.Diagnostics.FolderPath ?? Paths.Diagnostics);

                await ServiceManager.InitializeAsync(null, tokenSource.Token);

                Diagnostics.Initialize(ServiceManager.Get<ILogger>());
                Diagnostics.Message("Engine boot finished", nameof(EngineHost));

                lock (_stateLock)
                {
                    State = LifecycleState.Initialized;
                }
            }
            catch (OperationCanceledException)
            {
                lock (_stateLock)
                {
                    State = LifecycleState.Cancelled;
                }

                throw;
            }
            catch
            {
                lock (_stateLock)
                {
                    State = LifecycleState.Failed;
                }

                throw;
            }
        }

        public void Shutdown()
        {
            if (State is not LifecycleState.Initialized && State is not LifecycleState.Initializing)
            {
                throw new InvalidOperationException("Cannot shut down if it was not initialized");
            }

            lock (_stateLock)
            {
                try
                {
                    State = LifecycleState.Shutdown;
                    _tokenSource?.Cancel();
                    ServiceManager.Shutdown();
                }
                finally
                {
                    Diagnostics.Shutdown();
                    _tokenSource?.Dispose();
                    State = LifecycleState.Shutdown;
                }
            }
        }

        public void Dispose()
        {
            if (State == LifecycleState.Shutdown)
            {
                return;
            }

            Shutdown();
        }
    }
}
