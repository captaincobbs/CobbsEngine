using System.Diagnostics;
using System.Reflection;

using Engine.Core.Exceptions;
using Engine.Core.Services;
using Engine.Core.Settings;

using static Engine.Core.LaunchArgumentHandler;

namespace Engine.Core
{
    public static class Engine
    {
        public static Version EngineVersion { get; }

        static Engine()
        {
            EngineVersion = GetEngineVersion();
        }

        public static EngineHost CreateHost(string[] args, AssemblyMetadata metadata, EngineRuntime runtime)
        {
            LaunchArgumentParseResult launchArguments = Parse(args);
            FilePaths paths = FilePaths.FromMetadata(metadata);

            if (launchArguments.HasErrors)
            {
                throw new LaunchArgumentException(launchArguments.Issues);
            }

            EngineOptions launchOptions = launchArguments.Options;
            SettingsManager settings = new SettingsManager(paths.Settings);
            // There should be some way to "combine" interpreted launch arg override options + loaded options

            Diagnostics.Configure(launchOptions.Diagnostics, paths.Diagnostics);

            Diagnostics.Message("Engine boot started...", nameof(Engine));
            EngineEnvironment.Initialize(runtime, EngineVersion);

            ServiceRegistry registry = new();
            registry.ScanAssemblies([
                typeof(Engine).Assembly
            ]);
            return new(registry, settings, launchArguments.UnclaimedArguments, metadata, paths);
        }

        private static Version GetEngineVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return Version.Parse(FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? "0.0.0.0");
        }
    }
}
