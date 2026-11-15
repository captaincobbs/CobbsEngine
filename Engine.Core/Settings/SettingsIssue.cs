namespace Engine.Core.Settings
{
    public sealed record SettingsIssue(
        string? Key,
        SettingsIssueSeverity Severity,
        string Message,
        Exception? Exception = null
    );

    public enum SettingsIssueSeverity
    {
        Warning,
        Error,
    }
}
