namespace Engine.Core.Services
{
    public interface IService
    {
        virtual Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation)
        {
            progress?.Report(1f);
            return Task.CompletedTask;
        }

        virtual void Shutdown() { }
    }
}
