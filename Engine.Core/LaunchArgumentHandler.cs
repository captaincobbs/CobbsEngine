using static Engine.Core.Diagnostics;

namespace Engine.Core
{
    internal static class LaunchArgumentHandler
    {
        internal sealed class LaunchArgumentParseResult
        {
            public EngineOptions Options { get; }
            public IReadOnlyList<string> UnclaimedArguments { get; }
            public IReadOnlyList<LaunchArgumentIssue> Issues { get; }

            public bool HasErrors => Issues.Any(issue => issue.IsError);

            public LaunchArgumentParseResult(EngineOptions options, IReadOnlyList<string> unclaimedArguments, IReadOnlyList<LaunchArgumentIssue> issues)
            {
                Options = options;
                UnclaimedArguments = unclaimedArguments;
                Issues = issues;
            }
        }

        internal sealed record LaunchOption(string Name, string? Value);
        internal sealed record ConsoleCommandInvocation(string Name, IReadOnlyList<string> Arguments);
        internal sealed record LaunchArgumentIssue(string Argument, string Message, bool IsError);

        internal static LaunchArgumentParseResult Parse(IReadOnlyList<string> arguments)
        {
            EngineOptions options = new();
            List<string> unclaimedArguments = [];
            List<LaunchArgumentIssue> issues = [];

            for (int index = 0; index < arguments.Count; index++)
            {
                string argument = arguments[index];

                if (argument == "--")
                {
                    for (int remaining = index + 1; remaining < arguments.Count; remaining++)
                    {
                        unclaimedArguments.Add(arguments[remaining]);
                    }

                    break;
                }

                if (!argument.StartsWith("--", StringComparison.Ordinal))
                {
                    unclaimedArguments.Add(argument);
                    continue;
                }

                ParseOption(arguments, ref index, options, unclaimedArguments, issues);
            }

            return new LaunchArgumentParseResult(options, unclaimedArguments, issues);
        }

        private static void ParseOption(IReadOnlyList<string> arguments, ref int index, EngineOptions options, List<string> unclaimedArguments, List<LaunchArgumentIssue> issues)
        {
            string rawArgument = arguments[index];
            string optionText = rawArgument[2..];

            int equalsIndex = optionText.IndexOf('=');

            string name = equalsIndex >= 0 ? optionText[..equalsIndex] : optionText;
            string? inlineValue = equalsIndex >= 0 ? optionText[(equalsIndex + 1)..] : null;

            switch (name.ToLowerInvariant())
            {
                case "console":
                    ParseNoArgumentCommand(rawArgument, inlineValue, issues);
                    options.Debug.EnableConsole = true;
                    return;

                case "log-mirror":
                    ParseLogMirrorOption(arguments, ref index, rawArgument, inlineValue, options, issues);
                    return;

                default:
                    unclaimedArguments.Add(rawArgument);
                    return;
            }
        }

        private static void ParseNoArgumentCommand(string rawArgument, string? inlineValue, List<LaunchArgumentIssue> issues)
        {
            if (inlineValue is not null)
            {
                issues.Add(new LaunchArgumentIssue(rawArgument, $"{rawArgument} does not take a value", IsError: true));
                return;
            }
        }

        private static void ParseLogMirrorOption(IReadOnlyList<string> arguments, ref int index, string rawArgument, string? inlineValue, EngineOptions options, List<LaunchArgumentIssue> issues)
        {
            string? value = inlineValue;

            if (value is null)
            {
                if (index + 1 >= arguments.Count || arguments[index + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    issues.Add(new LaunchArgumentIssue(rawArgument, "--log-mirror requires a value.", IsError: true));

                    return;
                }

                value = arguments[++index];
            }

            if (value.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                options.Diagnostics.SetMirroredMessageTypes([]);
                return;
            }

            if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                options.Diagnostics.SetMirroredMessageTypes(Enum.GetValues<MessageType>());
                return;
            }

            List<MessageType> messageTypes = [];
            foreach (string part in value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (!Enum.TryParse(part, ignoreCase: true, out MessageType messageType) || !Enum.IsDefined(messageType))
                {
                    issues.Add(new LaunchArgumentIssue(rawArgument, $"'{part}' is not a valid log message type.", IsError: true));
                    continue;
                }

                messageTypes.Add(messageType);
            }

            if (messageTypes.Count > 0)
            {
                options.Diagnostics.SetMirroredMessageTypes(messageTypes);
            }
        }
    }
}