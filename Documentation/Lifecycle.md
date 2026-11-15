The Engine lifecycle contains 4 main states:
1. Startup, the game is booting up and the engine is initialized, creating an engine host. Services are detected and registered to the ServiceManager, and core Managers are created / initialized. At the end of this, an engine host is returned and used for initialization. Until this phase is over, the user is shown a black screen / potentially a console window.
2. Initialization, services are started up and initialized, assets are loaded, etc. A MainGame : EngineGame instance is created from the engine host, MainGame.Initialize() and MainGame.LoadContent() are called. During this screen, the user should be shown splash screens / loading screens.
3. Runtime, the game is in its core update/draw loops, the engine is simply updating/drawing as services request
4. Shutdown, the game calls Shutdown(), which calls EngineGame.Shutdown(), which calls EngineHost.Shutdown(). EngineHost then calls ServiceManager.Shutdown(), which iterates through all registered services and shuts them down. Then, engine host handles and extra disposing and anything subscribed to process_exit can do what they need to do, and the game shuts down normally. 

### Services
Services are all implementation of the abstract class Service. Service itself has several static methods for accessing services of a specific type.

Services are registered with the `[RegisterService(typeof(Interface))]` method, all services must register themselves as the implementation of an interface.

When the ServiceManager is started, it will look through all the types in the solution for the `RegisterService` attribute. Services may also have a `[DependsOn(typeof(Interface))]` attribute declared, which marks them as requiring another service to have booted up before they can boot up. An example of a dependency might be something like ModuleService requiring ContentService. The ModuleService would handle keeping track of external modules (mods), and loading their content into the ContentService. Thus, ModuleService would add a `[DependsOn(typeof(IContentService)]` attribute, then, which initializing, the ServiceManager would put ModuleService into a tier below ContentService, so ModuleService could initialize without having to worry about ContentService not being available.

The ServiceManager then sorts them into loading tiers, services that can be initialized at the same time, according to their dependencies. These loading tiers are also repurposed for shutdown, shutting them down in opposite order.

When ServiceManager is created, it registers itself to the static Service class, so then you can call ``Service.Get<T>`` or `Service.TryGet<T>`, as an example, which directly references ServiceManager, with Service just serving as a middleman to make the shorthand for accessing more convenient.

Anything that needs to access a service can then call Service.Get.

ServiceManager also has an Update and Draw function that are called during the EngineHost.Update() and Draw(). Services may implement the IDrawable and IUpdatable interfaces, which include on/off bools, priorities, and of course, the draw/update function. ServiceManager then records which services implement IDrawable and IUpdateable, and only tries calling draw/update for those relevant classes.

Different projects may have different implementations of the same services. This might be important for cross-platform. `[RegisterService()]` has arguments for GraphicsBackend and EnginePlatform enums that they should only be used in (both enums are flags), as well as a Priority int. The service is only registered if the game is running on the correct platform, and it then chooses only the registered service with the highest priority to be "the" implementation for the game's current runtime.

If a service fails during startup, it has two options. StopInitialization and Ignore. StopInitalization is the default, since most services are necessary, however, for some services, their failing isn't necessary for the game to function, and the game can just report it and move on (e.g. FMODAudioService if FMOD doesn't exist or NetworkService if there is no internet). Some services may also be able to retry post-bootstrap (such as the User being able to click a "retry connection" button in the main menu to enable multiplayer if no network was found during initialization).

If a non-critical Service or Manager fails to initialize, then its cleanup is handled by the ServiceManager, which calls Service.Dispose(). If a service fails midway through initialization then it should always be able to be cleaned up during a Dispose(). The difference between Shutdown and Dispose, is that Shutdown is only called when the game is shutting down, whereas Dispose can be called in the middle of the service's lifecycle if something was interrupted (Dispose would also like be called BY shutdown in most cases).
## Phases
1. Startup
During this phase, the game is performing the bare minimum it needs to get the game launching. This includes parsing launch args, loading settings, creating the Service Manager, initializing the diagnostics, etc. Generally, all of the work in this phase comes from Engine.CreateHost, doing all of the work it needs to determine how the host is made.
2. Initialization
During this phase, the game is iterating through all services and creating them. Services can come from any project in the solution. Services must report their progress when initializing, so that the game can sum up all the progress and display it on the loading screen. Basically all of this phase would happen in EngineHost.InitializeAsync, with the core final details happening when it finishes and the EngineGame/MainGame instances are created using the initialized EngineHost.
3. Runtime
This phase entails most of the game's normal runtime. The idea is most of the pre-loading happened in Engine.CreateHost and Host.Initialize, so the game is free to run without concern, using services and doing normal game logic as needed.
4. Shutdown
This phase is initialized by MainGame calling Shutdown(). This calls EngineGame.Shutdown(), which calls EngineHost.Shutdown(). This starts the process of all of the services shutting down, the game and all its resources disposing, then ending with the game finally closing safely. The very last thing to shutdown should be Diagnostics, since it needs to log everything else that happens before then + close the filestream to the file it was writing to.