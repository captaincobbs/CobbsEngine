# Service Architecture

## Scope

Services provide engine-wide capabilities such as logging, assets, audio, input, graphics, and scheduling. They are not a substitute for ordinary game objects or a general-purpose ECS.

## Composition

`Program` is the composition root. It creates one `ServiceEnvironment` and one Core-owned `ServiceRegistry`, scans the Core, engine, and game assemblies, and passes that registry to `MainGame`.

`ServiceRegistry` is application-owned and injected; it is not a global static. This keeps tests isolated and leaves room for multiple game/runtime instances.

## Registration

Every service contract inherits `IService`. An implementation may register one or more contracts using `[RegisterService]`; every registration has a priority and failure behavior.

`ServiceRegistrar` is metadata-only. It discovers attributes, validates registration shape, and records `ServiceRegistrationInfo`. It must never construct, select, initialize, or shut down services.

Registration metadata includes the source assembly, platform, graphics API, headless restriction, dependencies, priority, and failure behavior. Scanning the same assembly twice is idempotent. Removing an assembly removes only its registration metadata and permits it to be scanned again.

## Selection and dependencies

`ServiceManager` will later select one compatible candidate for each contract. Compatibility considers runtime platform, graphics API, and application mode.

For each contract it will order compatible registrations by descending priority; a tie for the highest priority is an error. `TryNextService` permits trying the next compatible registration only after the current candidate fails to initialize.

`[DependsOn]` declares service contract interfaces, never implementation types. The manager will select services first, require a selected candidate for every dependency, then topologically sort the selected registrations. Services initialize in dependency order and shut down in reverse order. A cycle must report the complete dependency chain.

## Lifecycle

A service is constructed and initialized at most once, then shut down at most once. A failed startup rolls back already initialized services in reverse order.

`Retry` is deliberately unsupported in v1 because it needs a defined retry schedule, cancellation model, and diagnostics policy.

## Assembly ownership

Each registration records its source assembly. Future module unloading must first shut down active services owned by the assembly, then remove their metadata, and finally prove that no events, tasks, or delegates retain the assembly.
