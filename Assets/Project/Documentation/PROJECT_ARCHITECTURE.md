# Project Architecture

Multi-Mode Card Framework uses a layered architecture to keep card-game rules reusable, external services replaceable, and Unity presentation code testable.

## Architectural Goals

- Keep game rules independent from Unity and platform SDKs.
- Separate domain state from views and scene objects.
- Compose runtime dependencies in one place with VContainer.
- Isolate PlayFab and XR integrations behind application-facing contracts.
- Make core rules and service behaviour testable in EditMode.

## High-Level Design

```text
+------------------------------------------------------+
| Presentation                                         |
| Views, UI Toolkit, controllers, table interaction    |
+-----------------------------+------------------------+
                              | interfaces and DI
+-----------------------------v------------------------+
| Platform and infrastructure adapters                 |
| Cloud / PlayFab, Input, XR, Unity-facing services   |
+-----------------------------+------------------------+
                              | contracts
+-----------------------------v------------------------+
| Core                                                  |
| CardData, Deck, game engines, domain utilities       |
| Pure C#; no UnityEngine or MonoBehaviour             |
+------------------------------------------------------+
```

The dependency direction points toward Core. Core does not know about scenes, UI, PlayFab, input devices, or XR hardware.

## Layers

### Core

Location: `Assets/Project/Scripts/Core/`

Core is the reusable card-game domain. It contains models such as `CardData` and `Deck`, the `BlackjackEngine`, `SolitaireEngine`, and `TexasHoldemEngine`, plus deterministic utilities such as shuffling and hand evaluation.

Core code must not reference `UnityEngine`, `MonoBehaviour`, UI Toolkit, PlayFab, or XR packages. Changes to game rules belong here whenever they do not require presentation or platform state.

### Presentation

Location: `Assets/Project/Scripts/Presentation/`

Presentation connects domain behaviour to Unity scenes and user interaction.

- `Controllers/` contains orchestration classes such as `BlackjackTableController`, `SolitaireTableController`, and `TexasHoldemTableController`.
- `Views/` contains Unity-facing visual components, card presentation, audio feedback, and spatial card interaction.
- `Interfaces/` defines view and service contracts used by controllers.
- `Interaction/` contains table-level interaction coordination.
- `VR/` contains presentation adaptations for spatial UI and Quest interaction.

Controllers coordinate through interfaces and injected services. Views present state and raise user actions; they should not own game rules.

### Cloud

Location: `Assets/Project/Scripts/Cloud/`

Cloud code adapts PlayFab to application-facing services. The `Interfaces/` folder defines authentication, cloud save, and cloud service contracts. The `PlayFab/` folder contains concrete implementations such as `PlayFabAuthService`, `PlayFabDataService`, `PlayFabEconomyService`, and `PlayFabTimeService`.

PlayFab SDK calls stay inside this layer. Tests can provide mocks through the same interfaces without requiring a live backend.

### Input

Location: `Assets/Project/Scripts/Input/`

Input is isolated from game rules and presentation details. Platform adapters translate device input into application-level interactions. Presentation consumes those interactions through contracts instead of calling legacy Unity input APIs directly.

### XR

Location: `Assets/Project/Scripts/XR/`

XR is an optional platform layer for Meta Quest and other XR integrations. XR-specific code must not leak into Core. Shared table and card behaviour remains in Presentation when it is useful across WebGL and VR; only hardware or SDK-specific behaviour belongs in XR.

### Architecture and Dependency Injection

Locations: `Assets/Project/Scripts/Architecture/` and `Assets/Project/Scripts/DependencyInjection/`

`Architecture/DI/GameLifetimeScope.cs` is the runtime composition root. VContainer registers engines, cloud services, input services, views, and controller entry points there.

Runtime code should use constructor injection or registered entry points. Avoid global singletons, `FindObjectOfType`, and scene searches for application services. Service lifetimes should match their responsibility: stateless engines can be transient, while session-level cloud services can be scoped or singleton within the application lifetime.

## Assembly Boundaries

The repository uses assembly definitions to enforce the main boundaries:

| Assembly | Responsibility |
| --- | --- |
| `Core` | Pure C# domain models, engines, and interfaces |
| `Presentation` | Unity views, controllers, UI, and interaction |
| `Cloud` | PlayFab services and cloud adapters |
| `Input` | Platform input adapters |
| `XR` | Optional XR integrations |
| `Utils` | Shared utility contracts and helpers |
| `Tests` | EditMode tests |
| `PlayModeTests` | PlayMode and integration tests |
| `Editor` | Unity Editor tooling |

A new class should be placed in the assembly that owns its responsibility. Do not add Unity references to `Core` to solve a presentation problem.

## Data and Event Flow

A typical table action follows this direction:

```text
User input
   -> input adapter or view event
   -> presentation controller
   -> core engine
   -> domain result or state change
   -> controller
   -> view update and feedback
```

Cloud operations follow a similar adapter boundary:

```text
Controller or application service
   -> ICloudService / IAuthenticationService / ICloudSaveService
   -> PlayFab implementation
   -> asynchronous result
   -> model or view update
```

External callbacks are converted to `Task`-based APIs where appropriate. Callers should await results and avoid blocking with `.Result` or `.Wait()` in Unity.

## Testing Strategy

Tests live under `Assets/Project/Scripts/Tests/`.

- **EditMode tests** cover Core engines, models, utilities, controllers, views, and PlayFab services with mocked dependencies.
- **PlayMode tests** cover scene-dependent and integration behaviour, including PlayFab integration scenarios.
- External SDKs are wrapped behind interfaces so unit tests remain deterministic and do not require network access.

When adding a service, add tests for successful results, failures, boundary conditions, and lifecycle or event unsubscription where applicable.

## Architectural Rules

1. Put game rules and state transitions in Core.
2. Keep Core free of Unity and external SDK references.
3. Keep views focused on presentation and user actions.
4. Use interfaces at infrastructure boundaries.
5. Register runtime dependencies through VContainer.
6. Use the New Input System and platform adapters for player input.
7. Keep XR-specific code isolated from flat-screen and Core code.
8. Prefer focused tests over scene-driven tests for domain and service logic.

For the folder map, see [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md). For planned work and milestones, see [PROJECT_ROADMAP.md](PROJECT_ROADMAP.md).
