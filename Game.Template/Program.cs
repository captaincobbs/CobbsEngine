using System;

using Engine.Core.Platform;
using Engine.Core.Services;

using Template;

class Program
{
    static void Main(string[] args)
    {
        ServiceEnvironment environment = new(
            backendVersion: new Version(1, 0),
            platform: RuntimePlatform.Windows,
            applicationMode: ApplicationMode.Windowed,
            graphicsBackend: GraphicsAPI.OpenGL);

        ServiceRegistry services = new(environment);
        services.ScanAssemblies(
        [
            typeof(IService).Assembly,
            typeof(Engine.MonoGame.MainGame).Assembly,
            typeof(Program).Assembly,
        ]);

        using (MainGame game = new(services))
        {
            game.Run();
        }
    }
}
