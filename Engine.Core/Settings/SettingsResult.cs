namespace Engine.Core.Settings
{
    public sealed record SettingsLoadResult(bool FileExists, IReadOnlyList<SettingsSectionLoadResult> Sections, IReadOnlyList<SettingsIssue> Issues)
    {
        public bool HasErrors => Issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error);
    }

    public sealed record SettingsSectionLoadResult(string Key, bool LoadedFromFile, bool UsedDefaults, IReadOnlyList<SettingsIssue> Issues)
    {
        public bool HasErrors => Issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error);
    }

    public sealed record SettingsSaveResult(string FilePath, bool Saved, IReadOnlyList<SettingsIssue> Issues)
    {
        public bool HasErrors => Issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error);
    }

    public sealed record SettingsUpdateResult<T>(T Settings, bool Changed, IReadOnlyList<SettingsIssue> Issues)
    {
        public bool HasErrors => Issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error);
    }

    public sealed record SettingsValidationResult<T>(T Value, IReadOnlyList<SettingsIssue> Issues)
    {
        public bool HasErrors => Issues.Any(issue => issue.Severity == SettingsIssueSeverity.Error);
    }

    public sealed record SettingsApplyReport(SettingsApplyPhase Phase, IReadOnlyDictionary<string, SettingsApplyResult> Results);

    public sealed record SettingsApplyResult(SettingsApplyStatus Status, string? Message = null, Exception? Exception = null);
}
