# Project Structure

This document describes the current Unity project layout. The structure is organized around architectural layers and platform integrations rather than individual game modes.

## Repository Layout

```text
Assets/
└── Project/
    ├── Scripts/
    │   ├── Architecture/       # Shared architectural contracts and DI composition
    │   ├── Core/               # Pure C# domain models, engines, and interfaces
    │   ├── Presentation/       # Views, controllers, UI, and interaction behaviour
    │   ├── Cloud/              # PlayFab authentication, persistence, economy, and time services
    │   ├── Input/              # Platform input adapters
    │   ├── XR/                 # Meta Quest and XR-specific integrations
    │   ├── DependencyInjection/ # Shared DI support
    │   ├── Utilities/          # Small reusable helpers and contracts
    │   └── Tests/              # EditMode and PlayMode test assemblies
    ├── Editor/                 # Unity Editor-only tools and assembly definition
    ├── UI/                     # UI Toolkit UXML layouts and USS styles
    ├── Scenes/                 # Initialization, menu, and game scenes
    ├── Prefabs/                # Reusable GameObjects and gameplay prefabs
    ├── Materials/              # Material assets
    ├── Animations/             # Animation clips and controllers
    ├── Art/                    # Audio, card artwork, and other presentation assets
    ├── Textures/               # Texture assets
    ├── Shaders/                # Shader Graph and shader assets
    ├── Data/                   # Game and application data assets
    ├── Resources/              # Runtime-loaded assets, used selectively
    ├── Config/                 # Project-specific configuration assets
    └── Documentation/          # Architecture, roadmap, backlog, and structure docs
```

## Scripts

### Core

`Assets/Project/Scripts/Core/` contains the platform-independent game domain. It must remain free of `UnityEngine` and `MonoBehaviour` references.

- `Models/`: `CardData`, `Deck`, and other game state models.
- `Engines/`: `BlackjackEngine`, `SolitaireEngine`, and `TexasHoldemEngine`.
- `Utils/`: deterministic domain helpers such as shuffling and hand evaluation.
- `Interfaces/`: contracts shared by core systems, including economy and time services.
- `Managers/`: domain-facing managers such as cloud mailbox state coordination.

### Presentation

`Assets/Project/Scripts/Presentation/` contains the Unity-facing application layer.

- `Controllers/`: POCO controllers that orchestrate views and core engines.
- `Views/`: UI Toolkit and 3D presentation components.
- `UI/`: UI-specific helpers and layout integration.
- `Interaction/`: physical card and table interaction behaviour.
- `VR/`: presentation adaptations for spatial interaction and Quest scenes.
- `Interfaces/`: view, input, modal, and presentation service contracts.

### Cloud

`Assets/Project/Scripts/Cloud/` contains PlayFab adapters and service contracts. External SDK access is isolated behind interfaces so services can be tested with mocks.

- `Interfaces/`: cloud, authentication, and save-service contracts.
- `PlayFab/`: authentication, data, economy, time, and cloud service implementations.

### Input and XR

`Input/` contains flat-screen input adapters. `XR/` contains optional XR-specific code and assembly configuration. Presentation code consumes input through interfaces instead of directly depending on a platform device.

### Tests

`Assets/Project/Scripts/Tests/` contains the automated test assemblies.

- `EditMode/`: fast tests for Core, Cloud, Presentation, and utility code.
- `PlayMode/`: scene and integration tests, including PlayFab integration coverage.
- `Tests.asmdef` and `PlayMode/PlayModeTests.asmdef`: Unity assembly definitions for each test environment.

## Assembly Definitions

The project uses assembly definitions to keep compilation boundaries explicit:

| Assembly | Responsibility |
| --- | --- |
| `Core` | Pure C# domain logic with no Unity dependency |
| `Presentation` | Unity views, controllers, UI, and interactions |
| `Cloud` | PlayFab services and cloud adapters |
| `Input` | Platform input adapters |
| `XR` | Optional Meta Quest and XR integrations |
| `Utils` | Shared utility contracts and helpers |
| `Tests` | EditMode tests |
| `PlayModeTests` | PlayMode and integration tests |
| `Editor` | Unity Editor tooling |

## Dependency Direction

```text
Presentation ──┐
Cloud ──────────┼──> Core
Input ──────────┤
XR ─────────────┘

Tests ──> Core, Presentation, Cloud, Input, XR
Editor ─> project assemblies as required by editor tooling
```

Core is the stable domain boundary. It does not depend on Unity, PlayFab, presentation code, or XR code. New game rules belong in Core; new screens and interaction behaviour belong in Presentation; external SDK integrations belong behind adapters in Cloud or XR.

## Dependency Injection

`Assets/Project/Scripts/Architecture/DI/GameLifetimeScope.cs` is the composition root. VContainer registers engines, services, views, and controllers there. Runtime systems should receive dependencies through constructors or registered entry points rather than using global singletons or scene searches.

## Documentation

- [PROJECT_ARCHITECTURE.md](PROJECT_ARCHITECTURE.md): architectural decisions and layer boundaries.
- [PROJECT_ROADMAP.md](PROJECT_ROADMAP.md): completed milestones and future work.
- [BACKLOG.md](BACKLOG.md): tracked tasks and prioritization.
- [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md): this document.

When adding a new system, update the relevant assembly definition and place it in the layer that owns its responsibility. Keep this document focused on stable boundaries and folders, not on every individual asset.
