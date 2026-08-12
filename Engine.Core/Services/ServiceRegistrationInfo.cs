using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Engine.Core.Services.Attributes;
using Engine.Core.Utilities;

using MonoGame.Framework.Utilities;

namespace Engine.Core.Services
{
    public sealed class ServiceRegistrationInfo
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public Assembly SourceAssembly { get; }

        public int Priority { get; }
        public ServiceFailureBehavior FailureBehavior { get; }

        public bool IsHeadlessOnly { get; }
        public RuntimePlatform TargetPlatforms { get; }
        public GraphicsBackend? GraphicsBackend { get; }

        public IReadOnlyList<Type> Dependencies { get; }

        public ServiceRegistrationInfo(
            Type serviceType, 
            Type implementationType, 
            Assembly sourceAssembly, 
            int priority, 
            ServiceFailureBehavior failureBehavior, 
            bool isHeadlessOnly, 
            RuntimePlatform targetPlatforms, 
            GraphicsBackend? graphicsBackend, 
            IReadOnlyList<Type> dependencies)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            SourceAssembly = sourceAssembly;
            Priority = priority;
            FailureBehavior = failureBehavior;
            IsHeadlessOnly = isHeadlessOnly;
            TargetPlatforms = targetPlatforms;
            GraphicsBackend = graphicsBackend;
            Dependencies = dependencies;
        }
    }
}
