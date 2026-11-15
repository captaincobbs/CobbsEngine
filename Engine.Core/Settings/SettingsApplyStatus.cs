namespace Engine.Core.Settings
{
    public enum SettingsApplyStatus
    {
        /// <summary>Successfully applied setting</summary>
        Applied,
        /// <summary>Valid setting, but it is not available yet</summary>
        Deferred,
        /// <summary>Valid settings, but the current platform/device cannot use it</summary>
        Unsupported,
        /// <summary>Something went wrong while applying the setting</summary>
        Failed,
    }
}
