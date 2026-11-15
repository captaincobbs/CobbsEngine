using System;
using System.Threading;
using System.Threading.Tasks;

using Engine.Core;
using Engine.MonoGame;
using Engine.MonoGame.Settings;
using Engine.Runtime;

using Template;

using CobbsEngine = Engine.Core.Engine;

class Program
{
    static async Task Main(string[] args)
    {
        EngineHost host = null;
        AppDomain.CurrentDomain.ProcessExit += Program_ProcessExit;
        AppDomain.CurrentDomain.UnhandledException += Program_UnhandledException;
        AssemblyMetadata metadata = AssemblyMetadata.FromAssembly(typeof(Program).Assembly);

        try
        {
            host = CobbsEngine.CreateHost(args, metadata, PlatformUtils.GetRuntimeInfo());
            host.Settings.Register([
                new AudioSettingsSection(),
                new DisplaySettingsSection(),
                new GameSettingsSection(),
                new InputSettingsSection()
            ]);

            host.RegisterAssemblies([
                typeof(MainGame).Assembly,
                typeof(EngineGame).Assembly,
                typeof(CobbsEngine).Assembly
            ]);

            await host.InitializeAsync(CancellationToken.None);
            using (GameRuntime runtime = new(host))
            {
                runtime.Start();
                using (MainGame game = new(host, runtime))
                {
                    game.Run();
                }
            }
        }
        finally
        {
            host?.Dispose();
            AppDomain.CurrentDomain.ProcessExit -= Program_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException -= Program_UnhandledException;
        }
    }

    private static void Program_ProcessExit(object sender, EventArgs e)
    {
        
    }

    private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Diagnostics.Exception(
            e.ExceptionObject as Exception ?? new Exception("Failed to capture the unhandled exception."),
            "Unhandled Exception",
            $"{nameof(Program)}.{nameof(Program_UnhandledException)}");
    }
}
