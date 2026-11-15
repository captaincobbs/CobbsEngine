namespace Engine.Core.Settings
{
    /// <summary>When the settings should be applied</summary>
    public enum SettingsApplyPhase
    {
        None,
        /// <summary>For settings applied before services initialize</summary>
        Engine,
        /// <summary>For settings applied while constructing/configuring the game host</summary>
        Host,
        /// <summary>For settings that can be applied during the game's runtime</summary>
        Runtime,
    }
}
