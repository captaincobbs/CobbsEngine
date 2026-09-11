namespace Engine.Core.Services
{
    public enum LifecycleState
    {
        NotStarted,
        Initializing,
        Initialized,
        Cancelled,
        Failed,
        ShuttingDown,
        Shutdown,
    }
}
