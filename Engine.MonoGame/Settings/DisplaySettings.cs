using System;

using Engine.Core.Settings;

namespace Engine.MonoGame.Settings
{
    public sealed record DisplaySettings
    {
        public Resolution Resolution { get; set; } = new(1920, 1080);
        public FrameRate TargetFPS { get; set; } = new FrameRate.Unlimited();
        public uint TargetDisplay { get; set; } = 0;
        public bool VSync { get; set; } = true;
        public WindowType WindowMode { get; set; } = WindowType.Windowed;
        public string AspectRatio { get; set; } = string.Empty;
    }

    public record Resolution(int Width, int Height);

    // Needs a JsonConverter
    public abstract record FrameRate
    {
        public sealed record Unlimited : FrameRate;
        public sealed record Fixed(int FPS) : FrameRate, IEquatable<int>
        {
            public bool Equals(int other) => FPS == other;
        }
    }

    public enum WindowType
    {
        Windowed,
        Fullscreen,
        BorderlessFullscreen,
    }

    public sealed class DisplaySettingsSection : SettingsSection<DisplaySettings>
    {
        public override string Key => "display";

        public override DisplaySettings CreateDefault() => new DisplaySettings();

        public override SettingsValidationResult<DisplaySettings> Validate(DisplaySettings settings)
        {
            DisplaySettings valid = settings with
            {
                Resolution = new(Math.Clamp(settings.Resolution.Width, 160, 10240), Math.Clamp(settings.Resolution.Height, 120, 4320)),
                TargetFPS = settings.TargetFPS switch
                {
                    FrameRate.Fixed fixedRate => new FrameRate.Fixed(Math.Clamp(fixedRate.FPS, 1, 1000)),
                    FrameRate.Unlimited => new FrameRate.Unlimited(),
                    _ => new FrameRate.Unlimited(),
                },
                TargetDisplay = Math.Clamp(settings.TargetDisplay, 0, 0), // <-- Change this to MaxDetectedDisplays later
                WindowMode = Enum.IsDefined<WindowType>(settings.WindowMode) ? settings.WindowMode : WindowType.Windowed,
                AspectRatio = settings.AspectRatio // <-- Check against known list of supported aspect ratios
            };

            return new(valid, []);
        }

        public override SettingsApplyResult Apply(DisplaySettings settings, SettingsApplyContext context)
        {
            if (context.Phase is not SettingsApplyPhase.Host || context.Phase is not SettingsApplyPhase.Runtime)
            {
                return new(SettingsApplyStatus.Deferred);
            }

            if (!context.TryGetTarget(out IDisplaySettingsTarget display))
            {
                return new(SettingsApplyStatus.Deferred);
            }

            return display.Apply(settings);
        }
    }

    public interface IDisplaySettingsTarget
    {
        SettingsApplyResult Apply(DisplaySettings settings);
    }
}
