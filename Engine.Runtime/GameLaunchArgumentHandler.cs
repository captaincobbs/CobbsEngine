namespace Engine.Runtime
{
    public static class GameLaunchArgumentHandler
    {
        public static GameLaunchOptions Parse(IReadOnlyList<string> arguments)
        {
            return new();
        }
    }
}
