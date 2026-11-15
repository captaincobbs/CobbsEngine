using System.Text.Json.Nodes;

namespace Engine.Core.Settings
{
    /// <summary>
    /// A transient replacement for part of a settings section. Overrides are applied
    /// after settings.json is read and are never written back by <see cref="ISettingsManager.SaveAsync"/>.
    /// </summary>
    public sealed record SettingsOverride(
        string SectionKey,
        IReadOnlyList<string> PropertyPath,
        JsonNode? Value);
}
