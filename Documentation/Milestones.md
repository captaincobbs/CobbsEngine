# MonoGame Framework Development Checklist

A private, reusable MonoGame-based framework intended primarily for 2D games such as roguelikes, card games, and simulation/management games.

## Guiding Principles

- [ ] Keep `Engine.Core` platform-independent and free of MonoGame dependencies where practical
- [ ] Prefer data-driven game architecture
- [ ] Keep simulation/game state separate from rendering
- [ ] Use services for cross-cutting engine functionality
- [ ] Keep the main game thread responsive
- [ ] Treat mods/modules as first-class citizens
- [ ] Design for runtime module loading/unloading
- [ ] Keep multiplayer optional and non-invasive
- [ ] Make debugging/profiling first-class engine features
- [ ] Avoid genre-specific systems in the framework
- [ ] Support Windows, Linux, and Android
- [ ] Keep the framework private/reusable rather than designing a public SDK

---

# Milestone 1 — Solution & Engine Foundation

## Solution Structure

- [ ] Create solution
- [ ] Create `Engine.Core`
- [ ] Create `Engine.Runtime`
- [ ] Create `Game` / template project
- [ ] Create test projects
- [ ] Establish project references/dependency direction
- [ ] Establish naming conventions
- [ ] Establish nullable reference type / warning policy
- [ ] Establish coding/style conventions

## MonoGame Bootstrap

- [ ] Create minimal MonoGame application
- [ ] Establish `GameRuntime`
- [ ] Separate framework lifecycle from MonoGame `Game`
- [ ] Establish initialization lifecycle
- [ ] Establish update lifecycle
- [ ] Establish draw lifecycle
- [ ] Establish shutdown/disposal lifecycle

## Logging

- [ ] Define `ILogger`
- [ ] Define log levels
- [ ] Console logger
- [ ] File logger
- [ ] Structured/contextual logging
- [ ] Log framework startup/shutdown
- [ ] Log unhandled exceptions

## Configuration

- [ ] Define engine configuration model
- [ ] Load configuration from file
- [ ] Support defaults
- [ ] Support environment/runtime overrides
- [ ] Separate engine configuration from game configuration

---

# Milestone 2 — Service Architecture

## Service Abstractions

- [ ] Define `IService`
- [ ] Define service lifetime
- [ ] Define service startup/shutdown lifecycle
- [ ] Define service dependencies
- [ ] Define service registration

## Service Container

- [ ] Implement `ServiceContainer`
- [ ] Implement service resolution
- [ ] Implement service scopes/lifetimes
- [ ] Implement dependency validation
- [ ] Detect circular dependencies
- [ ] Handle duplicate registrations

## Service Manager

- [ ] Create convenient static `Services` facade
- [ ] Keep actual state in service containers
- [ ] Support engine/game/scene lifetimes where appropriate
- [ ] Define registration order
- [ ] Define initialization order
- [ ] Define disposal order

## Attribute-Based Registration

- [ ] Define `[RegisterService]`
- [ ] Scan loaded assemblies
- [ ] Detect service implementations
- [ ] Validate registrations
- [ ] Support explicit/manual registration
- [ ] Support registration metadata

## Environment / Platform Filtering

- [ ] Define platform abstraction
- [ ] Windows support
- [ ] Linux support
- [ ] Android support
- [ ] Define client/headless runtime types
- [ ] Define platform/environment attributes
- [ ] Skip incompatible services/modules

---

# Milestone 3 — Runtime & Application Architecture

## Runtime Context

- [ ] Define `GameContext`
- [ ] Define runtime state
- [ ] Define game/session lifecycle
- [ ] Separate engine state from game state

## Main Loop

- [ ] Establish fixed/update timing strategy
- [ ] Establish rendering timing
- [ ] Prevent blocking operations in update/draw
- [ ] Handle window/application lifecycle
- [ ] Handle suspend/resume where applicable

## Headless Runtime

- [ ] Design headless runtime
- [ ] Prevent graphics/audio initialization in headless mode
- [ ] Allow simulation without rendering
- [ ] Allow future dedicated host/server usage

---

# Milestone 4 — Core Data Systems

## Identifiers

- [ ] Define stable IDs / names
- [ ] Define resource identifiers
- [ ] Define mod-qualified identifiers
- [ ] Handle ID collisions

## Serialization

- [ ] Select primary serialization formats
- [ ] Implement configuration serialization
- [ ] Implement data serialization
- [ ] Define versioning strategy
- [ ] Handle invalid/missing data
- [ ] Support migration where necessary

## Definitions

- [ ] Define base `Def`
- [ ] Implement `DefDatabase<T>`
- [ ] Implement definition loading
- [ ] Implement definition validation
- [ ] Implement references between definitions
- [ ] Support definition inheritance/composition
- [ ] Support mod-provided definitions
- [ ] Detect duplicate definitions
- [ ] Detect missing references

## Data Loading Pipeline

- [ ] Discover data sources
- [ ] Load raw data
- [ ] Parse data
- [ ] Resolve inheritance
- [ ] Resolve references
- [ ] Validate definitions
- [ ] Finalize databases
- [ ] Expose load diagnostics

---

# Milestone 5 — Events, Commands & Scheduling

## Event System

- [ ] Define event interfaces
- [ ] Implement event bus
- [ ] Support typed events
- [ ] Support subscriptions/unsubscriptions
- [ ] Track subscriptions for cleanup
- [ ] Prevent dead subscriptions from keeping mods alive

## Commands

- [ ] Define command abstraction
- [ ] Define command execution
- [ ] Define command results/errors
- [ ] Support command queues
- [ ] Design commands with future networking in mind

## Scheduling

- [ ] Define delayed tasks
- [ ] Define recurring tasks
- [ ] Define update scheduling
- [ ] Support cancellation
- [ ] Ensure scheduled work can be cleaned up

---

# Milestone 6 — Content & Asset System

## Asset Abstraction

- [ ] Define `IAssetService`
- [ ] Abstract MonoGame `ContentManager`
- [ ] Define asset identifiers
- [ ] Support synchronous loading
- [ ] Support asynchronous loading
- [ ] Handle missing assets
- [ ] Handle asset disposal

## Content Providers

- [ ] Base game content provider
- [ ] Mod content provider
- [ ] Layered content lookup
- [ ] Asset overrides
- [ ] Asset caching
- [ ] Asset invalidation/reload

## Supported Content

- [ ] Textures
- [ ] Fonts
- [ ] Audio
- [ ] Shaders
- [ ] Data files
- [ ] UI markup/style files

---

# Milestone 7 — Module & Mod System

## Module Model

- [ ] Define `IModule`
- [ ] Define `IModuleContext`
- [ ] Define module metadata
- [ ] Define module lifecycle
- [ ] Define module capabilities
- [ ] Define module isolation boundaries

## Mod Discovery

- [ ] Define mod directory structure
- [ ] Define metadata file
- [ ] Discover installed mods
- [ ] Validate metadata
- [ ] Detect duplicate mod IDs
- [ ] Produce useful diagnostics

## Dependencies

- [ ] Define dependency metadata
- [ ] Define version constraints
- [ ] Resolve dependency graph
- [ ] Topologically sort modules
- [ ] Detect circular dependencies
- [ ] Detect missing dependencies
- [ ] Detect incompatible versions
- [ ] Define optional dependencies

## Assembly Loading

- [ ] Define framework/mod API boundary
- [ ] Implement isolated assembly loading
- [ ] Load assemblies after game startup
- [ ] Discover module implementations
- [ ] Validate mod assemblies
- [ ] Handle assembly load failures

## Mod Lifecycle

- [ ] Discover
- [ ] Load
- [ ] Initialize
- [ ] Start
- [ ] Stop
- [ ] Unload
- [ ] Reload

## Safe Unloading

- [ ] Track event subscriptions
- [ ] Track services
- [ ] Track tasks/threads
- [ ] Track timers
- [ ] Track asset references
- [ ] Track delegates/callbacks
- [ ] Define mod cleanup contract
- [ ] Verify assembly unloadability

## Mod Communication

- [ ] Define public mod API
- [ ] Mod events
- [ ] Mod services
- [ ] Mod commands
- [ ] Mod definitions
- [ ] Mod dependency APIs
- [ ] Define compatibility/versioning rules

## Runtime Patching

- [ ] Define patching abstraction
- [ ] Integrate Harmony as optional functionality
- [ ] Support prefix/postfix/transpiler-style patches as appropriate
- [ ] Track patches by mod
- [ ] Remove patches during unload/reload
- [ ] Report patch conflicts/errors

---

# Milestone 8 — Rendering Foundation

## Renderer

- [ ] Define renderer abstraction
- [ ] Manage `SpriteBatch`
- [ ] Define render stages
- [ ] Define render layers
- [ ] Define render targets
- [ ] Handle resolution/scaling
- [ ] Handle fullscreen/window modes

## Camera

- [ ] Define 2D camera
- [ ] Camera transforms
- [ ] Zoom
- [ ] Viewport handling
- [ ] Screen/world coordinate conversion

## Rendering/Data Separation

- [ ] Keep game state independent from rendering
- [ ] Define render representations
- [ ] Define render synchronization/update
- [ ] Avoid gameplay objects owning graphics resources unnecessarily

## Performance

- [ ] Sprite batching
- [ ] Minimize state changes
- [ ] Texture/resource caching
- [ ] Rendering diagnostics
- [ ] Frame timing instrumentation

---

# Milestone 9 — Input System

## Input Abstraction

- [ ] Define `IInputService`
- [ ] Keyboard input
- [ ] Mouse input
- [ ] Controller/gamepad input
- [ ] Touch input
- [ ] Abstract platform-specific input

## Actions

- [ ] Define logical actions
- [ ] Bind physical inputs to actions
- [ ] Support rebinding
- [ ] Support multiple bindings
- [ ] Support contextual input
- [ ] Support UI/gameplay input separation

---

# Milestone 10 — UI Framework

## UI Tree

- [ ] Define `UIElement`
- [ ] Define parent/child hierarchy
- [ ] Define layout lifecycle
- [ ] Define measure/arrange model
- [ ] Define rendering lifecycle
- [ ] Define input lifecycle

## Core Elements

- [ ] Panel/container
- [ ] Label/text
- [ ] Image
- [ ] Button
- [ ] Toggle
- [ ] Input field
- [ ] Scroll container
- [ ] List/repeater
- [ ] Custom drawing element

## Layout

- [ ] Width/height
- [ ] Minimum/maximum size
- [ ] Margin
- [ ] Padding
- [ ] Alignment
- [ ] Anchoring
- [ ] Relative sizing
- [ ] Scrolling

## Styling

- [ ] Define style model
- [ ] Define selectors
- [ ] Define inheritance
- [ ] Define states
- [ ] Define theme support
- [ ] Define style overrides

## Markup

- [ ] Choose markup syntax
- [ ] Build parser
- [ ] Construct UI tree from markup
- [ ] Support attributes/properties
- [ ] Support templates
- [ ] Support reusable components
- [ ] Provide useful parse errors

## Data Binding

- [ ] Define binding syntax
- [ ] One-way binding
- [ ] Two-way binding where appropriate
- [ ] Collection/list binding
- [ ] Visibility/state binding
- [ ] Binding validation
- [ ] Avoid excessive allocations during binding

## UI Resources

- [ ] UI markup assets
- [ ] UI style assets
- [ ] UI themes
- [ ] UI localization
- [ ] UI asset hot reload where practical

---

# Milestone 11 — Debugging & Developer Tools

## Debug Console

- [ ] Define `IDebugConsole`
- [ ] Console command abstraction
- [ ] Command registration
- [ ] Command discovery
- [ ] Command arguments
- [ ] Command help
- [ ] Command history
- [ ] Command output/log integration

## Useful Commands

- [ ] `help`
- [ ] `services`
- [ ] `mods`
- [ ] `reloadmod`
- [ ] `defs`
- [ ] `assets`
- [ ] `memory`
- [ ] `gc`
- [ ] `threads`
- [ ] `profiler`
- [ ] `network`
- [ ] `inspect`

## Debug UI

- [ ] Debug overlay
- [ ] Service inspector
- [ ] Mod inspector
- [ ] Definition inspector
- [ ] Asset inspector
- [ ] Runtime state inspector
- [ ] Event inspector
- [ ] Console window

## Profiler

- [ ] Frame timing
- [ ] Update timing
- [ ] Draw timing
- [ ] Service timing
- [ ] Event timing
- [ ] Task timing
- [ ] Rendering timing
- [ ] Memory/GC information
- [ ] Network timing
- [ ] Profiling history

---

# Milestone 12 — Async & Performance Infrastructure

## Main Thread

- [ ] Define main-thread dispatcher
- [ ] Detect main-thread access
- [ ] Queue work to main thread
- [ ] Prevent unsafe graphics access from worker threads

## Background Tasks

- [ ] Define task service
- [ ] Worker scheduling
- [ ] Cancellation
- [ ] Error propagation
- [ ] Task ownership/lifetime
- [ ] Mod task cleanup

## Async Asset Loading

- [ ] Background IO
- [ ] Background parsing
- [ ] Main-thread GPU/resource finalization where required
- [ ] Loading progress
- [ ] Cancellation

## Performance Rules

- [ ] Identify blocking APIs
- [ ] Identify allocation-heavy paths
- [ ] Minimize per-frame allocations
- [ ] Avoid unnecessary locks
- [ ] Avoid uncontrolled task creation
- [ ] Establish frame-time budgets
- [ ] Add performance regression tests

---

# Milestone 13 — Save/Load Infrastructure

## Save Model

- [ ] Define save format
- [ ] Define save metadata
- [ ] Define versioning
- [ ] Define serialization boundaries
- [ ] Define migration strategy

## Save System

- [ ] Save game state
- [ ] Load game state
- [ ] Atomic saves
- [ ] Backup saves
- [ ] Corruption detection
- [ ] Error recovery

## Mod Compatibility

- [ ] Store mod list
- [ ] Store mod versions
- [ ] Detect missing mods
- [ ] Detect incompatible mods
- [ ] Define mod save-data API

---

# Milestone 14 — Networking Foundation

## Networking Abstraction

- [ ] Define `INetworkService`
- [ ] Define offline mode
- [ ] Define host mode
- [ ] Define client mode
- [ ] Keep networking optional

## Transport

- [ ] Select LAN transport
- [ ] Implement host
- [ ] Implement client
- [ ] Connection lifecycle
- [ ] Disconnect handling
- [ ] Basic latency measurement

## Lobby

- [ ] Create lobby
- [ ] List players
- [ ] Player join/leave
- [ ] Ready state
- [ ] Start game

## Synchronization

- [ ] Define network commands
- [ ] Define authoritative host
- [ ] Define replicated state
- [ ] Define snapshots
- [ ] Define client state updates
- [ ] Handle late/invalid commands
- [ ] Handle disconnects

## Turn-Based Support

- [ ] Turn ownership
- [ ] Turn submission
- [ ] Turn validation
- [ ] Turn synchronization
- [ ] Host authority

---

# Milestone 15 — Cross-Platform Support

## Windows

- [ ] Windows development build
- [ ] Windows packaging
- [ ] File/path handling
- [ ] Input verification
- [ ] Graphics verification

## Linux

- [ ] Linux development build
- [ ] Case-sensitive asset/path testing
- [ ] File/path handling
- [ ] Input verification
- [ ] Graphics verification
- [ ] Native dependency verification

## Android

- [ ] Android build
- [ ] Touch input
- [ ] Lifecycle/suspend/resume
- [ ] File storage abstraction
- [ ] Asset loading
- [ ] Graphics verification
- [ ] Performance verification
- [ ] Memory constraints

## Platform Abstraction

- [ ] File system abstraction where necessary
- [ ] Path abstraction
- [ ] Platform capability detection
- [ ] Platform-specific service registration
- [ ] Platform-specific input
- [ ] Platform-specific graphics limitations

---

# Milestone 16 — Template & Developer Experience

## Game Template

- [ ] Create clean game template
- [ ] Reference framework projects/packages
- [ ] Minimal startup scene
- [ ] Example service registration
- [ ] Example definition
- [ ] Example UI
- [ ] Example debug command

## Project Bootstrap

- [ ] Document how to create a new game
- [ ] Define standard project structure
- [ ] Define content structure
- [ ] Define mod structure
- [ ] Define configuration structure

## Developer Workflow

- [ ] Debug build
- [ ] Release build
- [ ] Automated tests
- [ ] Automated build
- [ ] Cross-platform build checks
- [ ] Useful startup diagnostics

---

# Milestone 17 — Hardening & Quality

## Testing

- [ ] Unit tests for core systems
- [ ] Service dependency tests
- [ ] Definition loading tests
- [ ] Serialization tests
- [ ] Mod loading tests
- [ ] Mod unloading tests
- [ ] UI layout tests
- [ ] Networking tests
- [ ] Platform abstraction tests

## Failure Handling

- [ ] Graceful service failure
- [ ] Graceful mod failure
- [ ] Graceful asset failure
- [ ] Graceful network failure
- [ ] Clear diagnostic messages
- [ ] Crash/error reporting

## Performance

- [ ] Startup performance
- [ ] Frame performance
- [ ] Memory usage
- [ ] Asset loading performance
- [ ] Mod loading performance
- [ ] UI performance
- [ ] Network performance

## Documentation

- [ ] Architecture overview
- [ ] Service system documentation
- [ ] Definition system documentation
- [ ] Mod API documentation
- [ ] UI markup documentation
- [ ] Debug console documentation
- [ ] Networking documentation
- [ ] Platform notes

---

# Suggested Final Framework Dependency Direction

```text
                    Game / Template
                          │
                          ▼
                    Engine.Runtime
                    /      |       \
                   /       |        \
                  ▼        ▼         ▼
              Graphics     UI     Networking
                  │        │         │
                  └────────┼─────────┘
                           ▼
                     Engine.Content
                           │
                           ▼
                      Engine.Mods
                           │
                           ▼
                       Engine.Core
```

The exact dependency graph should be refined as implementation progresses. In particular, avoid allowing higher-level systems to leak dependencies back into `Engine.Core`.

---

# Recommended Milestone Order

1. [ ] Solution & Engine Foundation
2. [ ] Service Architecture
3. [ ] Runtime & Application Architecture
4. [ ] Core Data Systems
5. [ ] Events, Commands & Scheduling
6. [ ] Content & Asset System
7. [ ] Module & Mod System
8. [ ] Rendering Foundation
9. [ ] Input System
10. [ ] UI Framework
11. [ ] Debugging & Developer Tools
12. [ ] Async & Performance Infrastructure
13. [ ] Save/Load Infrastructure
14. [ ] Networking Foundation
15. [ ] Cross-Platform Support
16. [ ] Template & Developer Experience
17. [ ] Hardening & Quality

---

# Explicitly Out of Scope for the Framework

These should remain game-specific unless a future game demonstrates a strong reason to promote them into the framework:

- [ ] Combat systems
- [ ] Inventory systems
- [ ] Card systems
- [ ] Character systems
- [ ] Quest systems
- [ ] Enemy AI
- [ ] Procedural generation algorithms
- [ ] Roguelike mechanics
- [ ] FTL-style ship systems
- [ ] Slay the Spire-style card mechanics
- [ ] Pixel Dungeon-style mechanics
- [ ] Genre-specific UI
- [ ] Genre-specific entity types

The framework should provide the **primitives** those systems can use, not implement the systems themselves.
