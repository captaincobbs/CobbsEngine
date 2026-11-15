namespace Engine.Core.Settings
{
    public interface ISettingsManager
    {
        SettingsApplyPhase Phase { get; }

        void Register<T>(ISettingsSection<T> section) where T : class;
        void Register(IEnumerable<ISettingsSection> sections);
        void Register(params ISettingsSection[] sections);
        T Get<T>(string key) where T : class;
        bool TryGet<T>(string key, out T? settings) where T : class;

        SettingsUpdateResult<T> Update<T>(string key, Func<T, T> update) where T : class;
        Task<SettingsLoadResult> LoadAsync(CancellationToken cancellation = default);
        Task<SettingsSaveResult> SaveAsync(CancellationToken cancellation = default);
        SettingsApplyReport AdvanceTo(SettingsApplyContext context);
        SettingsApplyReport Apply(SettingsApplyContext context);
    }
}