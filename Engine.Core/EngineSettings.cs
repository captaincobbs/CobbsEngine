using System.Drawing;

using Engine.Core.Settings;

using MessageType = Engine.Core.Diagnostics.MessageType;

namespace Engine.Core
{
    public sealed record EngineSettings
    {
        public DebugSettings Debug { get; init; } = new();
        public DiagnosticsSettings Diagnostics { get; init; } = new();
        public ModuleSettings Modules { get; init; } = new();
    }

    public sealed record DebugSettings
    {
        public bool DebugTools { get; set; }
        public bool EnableConsole { get; set; }
    }

    public sealed record DiagnosticsSettings
    {
        public HashSet<MessageType> MirroredTypes { get; set; } = [MessageType.Error, MessageType.Warning, MessageType.Exception];
        public Dictionary<MessageType, Color> ColorOverrides { get; init; } = [];
        public string? FolderPath { get; set; }
    }

    public sealed record ModuleSettings
    {
        public bool Enabled { get; set; } = true;
    }

    internal sealed class EngineSettingsSection : SettingsSection<EngineSettings>
    {
        public const string SectionKey = "engine";

        public override string Key => SectionKey;

        public override EngineSettings CreateDefault() => new();

        public override SettingsValidationResult<EngineSettings> Validate(EngineSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            return new(
                settings with
                {
                    Debug = settings.Debug ?? new DebugSettings(),
                    Diagnostics = settings.Diagnostics ?? new DiagnosticsSettings(),
                    Modules = settings.Modules ?? new ModuleSettings(),
                },
                []);
        }

        public override SettingsApplyResult Apply(EngineSettings settings, SettingsApplyContext context)
        {
            return new(SettingsApplyStatus.Applied);
        }
    }
}
