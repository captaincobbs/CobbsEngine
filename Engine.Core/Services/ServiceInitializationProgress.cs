namespace Engine.Core.Services
{
    public sealed record ServiceInitializationProgress
    (
        float OverallProgress,
        Type ServiceType,
        float ServiceProgress
    );
}
