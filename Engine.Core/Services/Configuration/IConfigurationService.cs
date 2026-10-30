namespace Engine.Core.Services.Configuration
{
    public interface IConfigurationService : IService
    {
        Task<T> LoadAsync<T>(ConfigurationKey key, T defaultValue, CancellationToken cancellation = default);
        Task SaveAsync<T>(ConfigurationKey key, T value, CancellationToken cancellation = default);
        Task<T> LoadAsync<T>(CancellationToken cancellation = default) where T : IConfigurable;
        Task SaveAsync<T>(T obj, CancellationToken cancellation = default) where T : IConfigurable;
    }

    public readonly record struct ConfigurationKey(string Scope, string Name);
}
