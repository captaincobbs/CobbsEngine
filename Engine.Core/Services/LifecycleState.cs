namespace Engine.Core.Services
{
    public enum LifecycleState
    {
        /// <summary>Service hasn't been initialized yet and is not ready to use</summary>
        NotStarted,
        /// <summary>Service is currently initialized and is not ready to use</summary>
        Initializing,
        /// <summary>Service initialized properly and is ready to use</summary>
        Initialized,
        /// <summary>Service's initialization was cancelled before it completed and is not ready to be used</summary>
        Cancelled,
        /// <summary>Service's initialization ran into a significant enough error to prevent it from being ready to use</summary>
        Failed,
        /// <summary>Service is in the process of shutting down</summary>
        ShuttingDown,
        /// <summary>Service successfully shut down and might be disposed</summary>
        Shutdown,
    }
}
