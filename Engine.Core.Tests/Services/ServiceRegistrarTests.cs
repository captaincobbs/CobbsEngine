using System.Reflection;

using Engine.Core.Services;
using Engine.Core.Services.Attributes;

using FluentAssertions;

namespace Engine.Core.Tests.Services;

public sealed class ServiceRegistrarTests
{
    const int ATTRIBUTED_SERVICE_PRIORITY = 20;
    const int ATTRIBUTED_SERVICE_WEIGHT = 75;
    const int DEPENDENCY_SERVICE_PRIORITY = 20;
    const int DEPENDENCY_SERVICE_WEIGHT = 75;

    [Fact]
    public void ScanAssembly_Check_Metadata_Without_Constructing()
    {
        ServiceRegistrar registrar = new();
        registrar.ScanAssembly(typeof(AttributedTestService).Assembly);

        ServiceRegistrationInfo registration = registrar
            .GetRegisteredServices<ITestService>()
            .Single(info => info.ImplementationType == typeof(AttributedTestService));

        registration.ServiceType.Should().Be(typeof(ITestService));
        registration.Priority.Should().Be(ATTRIBUTED_SERVICE_PRIORITY);
        registration.LoadingWeight.Should().Be(ATTRIBUTED_SERVICE_WEIGHT);
        registration.Dependencies.Should().ContainSingle().Which.Should().Be(typeof(IDependencyService));
        AttributedTestService.InstancesCreated.Should().Be(0);
    }

    [Fact]
    public void RemoveAssembly_Removes_Metadata_Properly()
    {
        ServiceRegistrar registrar = new();
        Assembly assembly = typeof(AttributedTestService).Assembly;

        registrar.ScanAssembly(assembly);
        registrar.RemoveAssembly(assembly);

        registrar.GetRegisteredServices<ITestService>().Should().BeEmpty();

        registrar.ScanAssembly(assembly);
        registrar.GetRegisteredServices<ITestService>().Should().NotBeEmpty();
    }

    #region Data Types
    private interface ITestService : IService;
    private interface IDependencyService : IService;

    [RegisterService(typeof(ITestService), priority: ATTRIBUTED_SERVICE_PRIORITY, loadingWeight: ATTRIBUTED_SERVICE_WEIGHT)]
    [DependsOn(typeof(IDependencyService))]
    private sealed class AttributedTestService : ITestService
    {
        public static int InstancesCreated { get; private set; }
        public AttributedTestService() => InstancesCreated++;
        public async Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation) { progress.Report(1f); }
        public void Shutdown() { }
    }

    [RegisterService(typeof(IDependencyService), priority: DEPENDENCY_SERVICE_PRIORITY, loadingWeight: DEPENDENCY_SERVICE_WEIGHT)]
    private sealed class DependencyTestService : IDependencyService
    {
        public static int InstancesCreated { get; private set; }
        public DependencyTestService() => InstancesCreated++;
        public async Task InitializeAsync(IServiceProvider services, IProgress<float> progress, CancellationToken cancellation) { progress.Report(1f); }
        public void Shutdown() { }
    }
    #endregion
}
