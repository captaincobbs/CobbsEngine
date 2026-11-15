using System.Diagnostics;
using System.Reflection;

using Engine.Core.Exceptions;
using Engine.Core.Platform;
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

        /// <summary>
        /// Creates the engine host from application-owned assemblies. Module packages
        /// are data-only and are never accepted through this composition boundary.
        /// </summary>
        public static EngineHost CreateHost(
            string[] args,
            AssemblyMetadata metadata,
            EngineRuntime runtime,
            params Assembly[] trustedAssemblies)
        {
            ArgumentNullException.ThrowIfNull(args);
            ArgumentNullException.ThrowIfNull(trustedAssemblies);
            if (trustedAssemblies.Any(assembly => assembly is null))
            {
                throw new ArgumentException("Trusted assemblies cannot contain null.", nameof(trustedAssemblies));
            }

            LaunchArgumentParseResult launchArguments = Parse(args);
            FilePaths paths = FilePaths.FromMetadata(metadata);

            if (launchArguments.HasErrors)
            {
                throw new LaunchArgumentException(launchArguments.Issues);
            }

            SettingsManager settings = new SettingsManager(paths.Settings);
            settings.Register(new EngineSettingsSection());

            Diagnostics.Message("Engine boot started...", nameof(Engine));
            if (launchArguments.Bootstrap.Headless)
            {
                runtime = runtime with { ApplicationMode = ApplicationMode.Headless };
            }

            EngineEnvironment.Initialize(runtime, EngineVersion);

            ServiceRegistry registry = new();
            registry.ScanAssemblies([typeof(Engine).Assembly, .. trustedAssemblies]);
            return new(registry, settings, launchArguments.SettingsOverrides, launchArguments.UnclaimedArguments, metadata, paths);
        }

        private static Version GetEngineVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return Version.Parse(FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? "0.0.0.0");
        }
    }
}
