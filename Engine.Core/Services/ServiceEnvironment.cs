using Engine.Core.Platform;

namespace Engine.Core.Services
{
    public class ServiceEnvironment
    {
        public Version BackendVersion;
        public RuntimePlatform Platform;
        public ApplicationMode ApplicationMode;
        public GraphicsAPI GraphicsBackend;

        public ServiceEnvironment(Version backendVersion, RuntimePlatform platform, ApplicationMode applicationMode, GraphicsAPI graphicsBackend)
        {
            BackendVersion = backendVersion;
            Platform = platform;
            ApplicationMode = applicationMode;
            GraphicsBackend = graphicsBackend;
        }
    }
}
