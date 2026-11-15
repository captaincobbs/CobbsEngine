using System.Text.Json;
using System.Text.Json.Nodes;

using Engine.Core.Settings;

using static Engine.Core.Diagnostics;

namespace Engine.Core
{
    internal static class LaunchArgumentHandler
    {
        internal sealed class LaunchArgumentParseResult
        {
            public BootstrapOptions Bootstrap { get; }
            public IReadOnlyList<SettingsOverride> SettingsOverrides { get; }
            public IReadOnlyList<string> UnclaimedArguments { get; }
            public IReadOnlyList<LaunchArgumentIssue> Issues { get; }

            public bool HasErrors => Issues.Any(issue => issue.IsError);

            public LaunchArgumentParseResult(
                BootstrapOptions bootstrap,
                IReadOnlyList<SettingsOverride> settingsOverrides,
                IReadOnlyList<string> unclaimedArguments,
                IReadOnlyList<LaunchArgumentIssue> issues)
            {
                Bootstrap = bootstrap;
                SettingsOverrides = settingsOverrides;
                UnclaimedArguments = unclaimedArguments;
                Issues = issues;
            }
        }

        internal sealed record LaunchArgumentIssue(string Argument, string Message, bool IsError);

        internal static LaunchArgumentParseResult Parse(IReadOnlyList<string> arguments)
        {
            ArgumentNullException.ThrowIfNull(arguments);

            BootstrapOptions bootstrap = new();
            List<SettingsOverride> overrides = [];
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

                ParseOption(arguments, ref index, ref bootstrap, overrides, unclaimedArguments, issues);
            }

            return new LaunchArgumentParseResult(bootstrap, overrides, unclaimedArguments, issues);
        }

        private static void ParseOption(
            IReadOnlyList<string> arguments,
            ref int index,
            ref BootstrapOptions bootstrap,
            List<SettingsOverride> overrides,
            List<string> unclaimedArguments,
            List<LaunchArgumentIssue> issues)
        {
            string rawArgument = arguments[index];
            string optionText = rawArgument[2..];
            int equalsIndex = optionText.IndexOf('=');

            string name = equalsIndex >= 0 ? optionText[..equalsIndex] : optionText;
            string? inlineValue = equalsIndex >= 0 ? optionText[(equalsIndex + 1)..] : null;

            switch (name.ToLowerInvariant())
            {
                case "console":
                    if (ParseNoArgumentOption(rawArgument, inlineValue, issues))
                    {
                        overrides.Add(new(EngineSettingsSection.SectionKey, ["debug", "enableConsole"], JsonValue.Create(true)));
                    }
                    return;

                case "headless":
                    if (ParseNoArgumentOption(rawArgument, inlineValue, issues))
                    {
                        bootstrap = bootstrap with { Headless = true };
                    }
                    return;

                case "safe-mode":
                case "no-mods":
                    if (ParseNoArgumentOption(rawArgument, inlineValue, issues))
                    {
                        overrides.Add(new(EngineSettingsSection.SectionKey, ["modules", "enabled"], JsonValue.Create(false)));
                    }
                    return;

                case "log-mirror":
                    ParseLogMirrorOption(arguments, ref index, rawArgument, inlineValue, overrides, issues);
                    return;

                case "set":
                    ParseSettingsOverride(arguments, ref index, rawArgument, inlineValue, overrides, issues);
                    return;

                default:
                    unclaimedArguments.Add(rawArgument);
                    return;
            }
        }

        private static bool ParseNoArgumentOption(string rawArgument, string? inlineValue, List<LaunchArgumentIssue> issues)
        {
            if (inlineValue is null)
            {
                return true;
            }

            issues.Add(new LaunchArgumentIssue(rawArgument, $"{rawArgument} does not take a value.", IsError: true));
            return false;
        }

        private static void ParseLogMirrorOption(
            IReadOnlyList<string> arguments,
            ref int index,
            string rawArgument,
            string? inlineValue,
            List<SettingsOverride> overrides,
            List<LaunchArgumentIssue> issues)
        {
            string? value = TakeValue(arguments, ref index, rawArgument, inlineValue, "--log-mirror", issues);
            if (value is null)
            {
                return;
            }

            if (value.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                overrides.Add(new(EngineSettingsSection.SectionKey, ["diagnostics", "mirroredTypes"], new JsonArray()));
                return;
            }

            IEnumerable<MessageType> messageTypes;
            if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                messageTypes = Enum.GetValues<MessageType>();
            }
            else
            {
                List<MessageType> parsedTypes = [];
                foreach (string part in value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!Enum.TryParse(part, ignoreCase: true, out MessageType messageType) || !Enum.IsDefined(messageType))
                    {
                        issues.Add(new LaunchArgumentIssue(rawArgument, $"'{part}' is not a valid log message type.", IsError: true));
                        continue;
                    }

                    parsedTypes.Add(messageType);
                }

                if (parsedTypes.Count == 0)
                {
                    return;
                }

                messageTypes = parsedTypes;
            }

            JsonArray jsonTypes = [];
            foreach (MessageType messageType in messageTypes)
            {
                jsonTypes.Add(messageType.ToString());
            }

            overrides.Add(new(EngineSettingsSection.SectionKey, ["diagnostics", "mirroredTypes"], jsonTypes));
        }

        private static void ParseSettingsOverride(
            IReadOnlyList<string> arguments,
            ref int index,
            string rawArgument,
            string? inlineValue,
            List<SettingsOverride> overrides,
            List<LaunchArgumentIssue> issues)
        {
            string? specification = TakeValue(arguments, ref index, rawArgument, inlineValue, "--set", issues);
            if (specification is null)
            {
                return;
            }

            int valueSeparator = specification.IndexOf('=');
            if (valueSeparator <= 0)
            {
                issues.Add(new LaunchArgumentIssue(rawArgument, "--set requires <section.property>=<json-value>.", IsError: true));
                return;
            }

            string path = specification[..valueSeparator];
            string valueText = specification[(valueSeparator + 1)..];
            string[] rawSegments = path.Split('.');
            if (rawSegments.Length < 2 || rawSegments.Any(string.IsNullOrWhiteSpace))
            {
                issues.Add(new LaunchArgumentIssue(rawArgument, "A settings override path must contain a section and at least one property.", IsError: true));
                return;
            }

            JsonNode? value;
            try
            {
                value = JsonNode.Parse(valueText);
            }
            catch (JsonException)
            {
                // Bare values are convenient for string settings. JSON syntax still
                // controls booleans, numbers, null, arrays, and objects.
                value = JsonValue.Create(valueText);
            }

            overrides.Add(new SettingsOverride(rawSegments[0], rawSegments[1..], value));
        }

        private static string? TakeValue(
            IReadOnlyList<string> arguments,
            ref int index,
            string rawArgument,
            string? inlineValue,
            string optionName,
            List<LaunchArgumentIssue> issues)
        {
            if (inlineValue is not null)
            {
                return inlineValue;
            }

            if (index + 1 >= arguments.Count || arguments[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                issues.Add(new LaunchArgumentIssue(rawArgument, $"{optionName} requires a value.", IsError: true));
                return null;
            }

            return arguments[++index];
        }
    }

    public sealed record BootstrapOptions
    {
        public bool Headless { get; init; }
    }
}
