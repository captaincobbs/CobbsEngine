using System.Reflection;

using Engine.Core.Platform;

namespace Engine.Core.Services
{
    /// <summary>Holds info regarding the service's requirements and registration details.</summary>
    public sealed class ServiceRegistrationInfo
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public Assembly SourceAssembly { get; }

        public int Priority { get; }
        public int LoadingWeight { get; }
        public ServiceFailureBehavior FailureBehavior { get; }

        public ApplicationMode ApplicationMode { get; }
        public RuntimePlatform TargetPlatforms { get; }
        public GraphicsAPI? GraphicsBackend { get; }

        public IReadOnlyList<Type> Dependencies { get; }

        public ServiceRegistrationInfo(
            Type serviceType, 
            Type implementationType, 
            Assembly sourceAssembly, 
            int priority,
            int loadingWeight,
            ServiceFailureBehavior failureBehavior,
            ApplicationMode applicationMode, 
            RuntimePlatform targetPlatforms, 
            GraphicsAPI? graphicsBackend, 
            IReadOnlyList<Type> dependencies)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            SourceAssembly = sourceAssembly;
            Priority = priority;
            LoadingWeight = loadingWeight;
            FailureBehavior = failureBehavior;
            ApplicationMode = applicationMode;
            TargetPlatforms = targetPlatforms;
            GraphicsBackend = graphicsBackend;
            Dependencies = new List<Type>(dependencies);
        }
    }
}
