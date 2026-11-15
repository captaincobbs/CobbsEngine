using static Engine.Core.LaunchArgumentHandler;

namespace Engine.Core.Exceptions
{
    internal sealed class LaunchArgumentException : Exception
    {
        public IReadOnlyList<LaunchArgumentIssue> Issues { get; }

        public LaunchArgumentException(IReadOnlyList<LaunchArgumentIssue> issues) : base("One or more launch arguments were invalid")
        {
            Issues = issues;
        }

        public LaunchArgumentException(LaunchArgumentIssue issue) : base("One or more launch arguments were invalid")
        {
            Issues = new List<LaunchArgumentIssue>() { issue };
        }
    }
}
