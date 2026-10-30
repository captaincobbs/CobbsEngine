namespace Engine.Core.Services.Configuration
{
    public sealed record LogTheme(
        string MessageColor,
        string WarningColor,
        string ErrorColor,
        string ExceptionColor,
        string DebugColor,
        string AssertColor
    );
}
