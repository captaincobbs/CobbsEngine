using Engine.Core.Settings;

namespace Engine.MonoGame.Settings
{
    public sealed record class GameSettings
    {
    }

    public sealed class GameSettingsSection : SettingsSection<GameSettings>
    {
        public override string Key => "game";

        public override GameSettings CreateDefault() => new GameSettings();

        public override SettingsValidationResult<GameSettings> Validate(GameSettings settings)
        {
            GameSettings valid = settings with
            {
            };

            return new(valid, []);
        }

        public override SettingsApplyResult Apply(GameSettings settings, SettingsApplyContext context)
        {
            if (context.Phase != SettingsApplyPhase.Runtime)
            {
                return new SettingsApplyResult(SettingsApplyStatus.Deferred);
            }

            return new(SettingsApplyStatus.Applied);
        }
    }
}
