using Engine.Core.Platform;

namespace Engine.Core
{
    public static class EngineEnvironment
    {
        public static RuntimePlatform RuntimePlatform { get; private set; }
        public static ApplicationMode ApplicationMode { get; private set; }
        public static GraphicsAPI GraphicsAPI { get; private set; }
        public static Version? EngineVersion { get; private set; } = null;

        private static bool _initialized = false;

        internal static void Initialize(EngineRuntime platform, Version engineVersion)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Cannot initialize engine environment twice");
            }

            RuntimePlatform = platform.RuntimePlatform;
            ApplicationMode = platform.ApplicationMode;
            GraphicsAPI = platform.GraphicsAPI;
            EngineVersion = engineVersion;

            _initialized = true;
        }
    }
}
