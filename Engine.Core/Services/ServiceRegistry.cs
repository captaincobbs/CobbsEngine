using System.Reflection;

namespace Engine.Core.Services;

/// <summary>
/// External entry point for service registration and service access.
/// Owns no global state so tests and multiple game instances remain isolated.
/// </summary>
public sealed class ServiceRegistry
{
    public ServiceEnvironment Environment { get; }

    /// <summary>Contains discovered registration metadata-- never constructs services.</summary>
    public ServiceRegistrar Registrar { get; }
    public ServiceManager ServiceManager { get; }

    public ServiceRegistry(ServiceEnvironment environment)
    {
        Environment = environment;
        Registrar = new();
        ServiceManager = new(Registrar, environment);
    }

    public void ScanAssemblies(IEnumerable<Assembly> assemblies) => Registrar.ScanAssemblies(assemblies);
}
