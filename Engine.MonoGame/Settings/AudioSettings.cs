using System;

using Engine.Core.Settings;

namespace Engine.MonoGame.Settings
{
    public sealed record AudioSettings
    {
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 1f;
        public float EffectsVolume { get; set; } = 1f;
        public bool AudioInBackground { get; set; } = true;
    }

    public sealed class AudioSettingsSection : SettingsSection<AudioSettings>
    {
        public override string Key => "audio";

        public override AudioSettings CreateDefault() => new AudioSettings();

        public override SettingsValidationResult<AudioSettings> Validate(AudioSettings settings)
        {
            AudioSettings valid = settings with
            {
                MasterVolume = Math.Clamp(settings.MasterVolume, 0f, 2f),
                MusicVolume = Math.Clamp(settings.MusicVolume, 0f, 2f),
                EffectsVolume = Math.Clamp(settings.MusicVolume, 0f, 2f),
            };

            return new(valid, []);
        }

        public override SettingsApplyResult Apply(AudioSettings settings, SettingsApplyContext context)
        {
            if (context.Phase != SettingsApplyPhase.Runtime)
            {
                return new SettingsApplyResult(SettingsApplyStatus.Deferred);
            }

            return new(SettingsApplyStatus.Applied);
        }
    }
}
