namespace Engine.Core.Services
{
    public interface IService
    {
        Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation);

        void Shutdown();
    }
}
