using Engine.Core.Platform;

namespace Engine.Core
{
    public sealed record EngineRuntime(
        RuntimePlatform RuntimePlatform,
        ApplicationMode ApplicationMode,
        GraphicsAPI GraphicsAPI
    );
}
