namespace Engine.Core.Platform
{
    [Flags]
    public enum ApplicationMode
    {
        None = 0,
        Windowed            = 1 << 0,
        Headless            = 1 << 1,
        WindowedWithConsole = 1 << 2,
        All = Windowed | Headless | WindowedWithConsole,
    }
}
