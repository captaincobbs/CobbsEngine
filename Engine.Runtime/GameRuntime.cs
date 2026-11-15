using Engine.Core;
using Engine.Core.Services;

namespace Engine.Runtime
{
    public sealed class GameRuntime : IDisposable
    {
        private readonly EngineHost _host;
        private bool _started;
        private bool _disposed;

        public bool IsRunning => _started && !_disposed;

        public GameRuntime(EngineHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public void Start()
        {
            ThrowIfDisposed();

            if (_started)
            {
                throw new InvalidOperationException("GameRuntime has already started.");
            }

            if (_host.State != LifecycleState.Initialized)
            {
                throw new InvalidOperationException("The engine host must be initialized before starting a game runtime.");
            }

            _started = true;
        }

        public void Update(TimeSpan elapsed)
        {
            ThrowIfDisposed();

            if (!_started)
            {
                throw new InvalidOperationException("GameRuntime must be started before it can update.");
            }

            if (elapsed < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _started = false;
            _disposed = true;
        }

        public T Get<T>() where T : class, IService
        {
            ThrowIfDisposed();
            return _host.ServiceManager.Get<T>();
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }
}
