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
        private readonly AssemblyMetadata metadata;


        public IReadOnlyList<string> UnclaimedLaunchArguments { get; }

        private CancellationTokenSource? _tokenSource;
        private readonly Lock _stateLock = new();

        internal EngineHost(ServiceRegistry registry, ISettingsManager settings, IReadOnlyList<string> unclaimedArguments, AssemblyMetadata metadata, FilePaths paths)
        {
            State = LifecycleState.NotStarted;
            ServiceRegistry = registry;
            ServiceManager = registry.ServiceManager;
            Settings = settings;
            UnclaimedLaunchArguments = unclaimedArguments;
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
                await Settings.LoadAsync(tokenSource.Token);
                Settings.AdvanceTo(new(SettingsApplyPhase.Engine, ServiceManager));

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
            // Get CompositeLogger here and attach it to diagnostics, feed it all other instances of other loggers? Maybe make Loggers not be a service?
        }

        public void RegisterAssemblies(IEnumerable<Assembly> assemblies)
        {
            ArgumentNullException.ThrowIfNull(assemblies);

            if (State != LifecycleState.NotStarted)
            {
                throw new InvalidOperationException("Assemblies must be registered before engine initialization.");
            }

            ServiceRegistry.ScanAssemblies(assemblies);
        }

        public void Shutdown()
        {
            try
            {
                State = LifecycleState.ShuttingDown;
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

        public void Dispose()
        {
            Shutdown();
        }
    }
}
