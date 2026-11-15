using System;

using Engine.Core.Settings;

namespace Engine.MonoGame.Settings
{
    public sealed record class InputSettings
    {
    }

    public sealed class InputSettingsSection : SettingsSection<InputSettings>
    {
        public override string Key => "input";

        public override InputSettings CreateDefault() => new InputSettings();

        public override SettingsValidationResult<InputSettings> Validate(InputSettings settings)
        {
            InputSettings valid = settings with
            {
            };

            return new(valid, []);
        }

        public override SettingsApplyResult Apply(InputSettings settings, SettingsApplyContext context)
        {
            if (context.Phase != SettingsApplyPhase.Runtime)
            {
                return new SettingsApplyResult(SettingsApplyStatus.Deferred);
            }

            return new(SettingsApplyStatus.Applied);
        }
    }
}
