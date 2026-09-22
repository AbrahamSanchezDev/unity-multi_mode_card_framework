# Engineering Backlog

**Project**: Multi-Mode Card Framework  
**Status**: MVP complete  
**Last Updated**: September 2026  
**Maintainer**: Solo developer

This backlog combines the current delivery summary with the project’s milestone history. It preserves the completed MVP outcome while keeping the important engineering work visible for portfolio review, teammate handoff, and future iteration.

## MVP Delivery

The MVP is available through the [WebGL demo](https://abrahamsanchezdev.github.io/unity-multi_mode_card_framework/) and the [Meta Quest 3 VR release](https://github.com/AbrahamSanchezDev/unity-multi_mode_card_framework/releases/tag/VR_v1).

### Completed Epics

| Epic | Outcome | Status |
| --- | --- | --- |
| Core game logic | Pure C# models, deck utilities, and Blackjack, Solitaire, and Texas Hold'em engines | Complete |
| Architecture and DI | Layered MVC/MVP boundaries, assembly definitions, and VContainer composition root | Complete |
| Presentation and UI | UI Toolkit screens, adaptive layouts, card views, table controllers, and interactions | Complete |
| Cloud and persistence | PlayFab authentication, cloud data, economy, server time, mailbox, and account linking | Complete |
| XR and deployment | Spatial card interaction, Quest presentation, WebGL validation, and production releases | Complete |

### MVP Acceptance Checklist

- [x] Game rules run in platform-independent Core code.
- [x] Blackjack, Solitaire, and Texas Hold'em are playable.
- [x] Presentation code is separated from game rules.
- [x] Runtime dependencies are composed through VContainer.
- [x] PlayFab services are accessed through testable interfaces and adapters.
- [x] WebGL demo is published and playable in a browser.
- [x] Meta Quest 3 build is available through GitHub Releases.
- [x] EditMode and PlayMode test assemblies are part of the project.
- [x] Spatial card interaction and tactile feedback are implemented.

---

## Project History and Milestone Record

The work below documents the major development phases that led to the delivered MVP. These items are intentionally kept as a historical record of completed work rather than future speculative scope.

### Epic 1: Core Logic Engine

_Pure C# simulation logic, deterministic and independent of Unity's MonoBehaviour loop._

- [x] TASK-1.1: Blackjack Engine Implementation
  - Hand evaluation, bust detection, dealer soft-17 rules.
- [x] TASK-1.2: Texas Holdem Engine Base
  - Core card tracking and simulation.
- [x] TASK-1.3: Solitaire Engine Base
  - Tableau, foundation, and stockpile rule enforcement.
- [x] TASK-1.4: Core Data Models
  - Standardized CardData, Deck shuffling, and generic enums.

### Epic 2: Persistence & Cloud Backend

_PlayFab integration, data storage, and cross-device sync pipelines._

- [x] TASK-2.1: Dependency Injection Container
  - Setup VContainer lifecycle via GameLifetimeScope.
  - Remove direct global singleton access patterns.
- [x] TASK-2.2: Cloud Infrastructure & Anonymous Authentication
  - Implement PlayFab auth and data services with mock-friendly wrappers.
  - Keep external APIs behind interfaces for test isolation.

### Epic 3: Architecture & UI Foundation

_Screen-space canvases, decoupled presentation controllers, and input abstraction._

- [x] TASK-3.3: UI Presentation Layer Architecture (MVP/MVC Setup)
  - Define view contracts and POCO controllers.
  - Validate controller state transitions through EditMode tests.
- [x] TASK-3.4: Dynamic Table UI Controller Implementation
  - Map logic events to UI feedback loops.
  - Animate cards and clean up on view destruction.
- [x] TASK-3.5: User Notification & Modal Window System
  - Screen-space modal canvas for errors, async cloud workflows, and overlays.
- [x] TASK-3.6: Multi-Platform Input Adapter
  - Implement platform-agnostic input abstraction for pointer and spatial interactions.
- [x] TASK-3.7: Adaptive Screen-Space UI & Responsiveness
  - Refactor USS layouts and responsive panel behavior for multiple display sizes.

### Epic 4: Cloud Infrastructure & Metagame

_Expansion of persistence, economy, and cross-platform session flows._

- [x] TASK-4.1: PlayFab SDK & Silent Authentication
  - Integrate PlayFab SDK with decoupled injection safety mappings.
- [x] TASK-4.2: Economy & Betting System (Cloud Core)
  - Model currency and in-game credit flows with cloud-backed services.
  - Deliver automated EditMode validation for wagering states.
- [x] TASK-4.3: Cross-Platform Account Linking
  - Build asynchronous PIN-based sync flow between browser and Quest contexts.
- [x] TASK-4.4: Cloud Mailbox & Anti-Cheat Cooldowns
  - Integrate server-verified rewards and time-based gating.
- [x] TASK-4.5: Alternative Game UI (Solitaire & Texas Hold'em Layouts)
  - Extend card table presentation and interaction flows for additional game modes.

### Epic 5: XR Integration & Deployment

_Meta Quest 3 spatial design, cross-platform validation, and production build pipelines._

- [x] TASK-5.1: Meta Quest 3 Rig Setup & Interaction Binding
  - Integrate XR Interaction Toolkit with VContainer runtime setup.
  - Wire XR grab and release events into spatial card interaction logic.
  - Add haptic impulse feedback routines.
- [x] TASK-5.2: Spatial UI & VR Dashboard Adaptation
  - Adapt UI Toolkit documents and world-space layouts for Quest comfort.
  - Support cross-device sync UI and input flow.
- [x] TASK-5.3: WebGL Build & Performance Optimization
  - Configure production player settings and validate browser performance.
  - Test silent auth behavior in the browser environment.
- [x] TASK-5.4: Multi-Platform Validation & Build Pipeline
  - Validate state consistency between WebGL and Meta Quest 3 builds.
  - Ship production export profiles and release artifacts.

---

## Future Backlog

These items are intentionally outside the completed MVP. They should be prioritized by user feedback, portfolio value, and available development time.

### P1: Quality and Maintenance

- [ ] Add a repeatable CI workflow for Unity EditMode tests and coverage reporting.
- [ ] Capture a current coverage baseline and keep it aligned with the README metrics.
- [ ] Add automated smoke checks for the published WebGL build.
- [ ] Add a short troubleshooting guide for PlayFab configuration and WebGL authentication.
- [ ] Review and reduce runtime allocations during large card-tableau updates.

### P2: Gameplay Depth

- [ ] Add additional Solitaire variations or difficulty settings.
- [ ] Expand Texas Hold'em betting rules and player-state flows.
- [ ] Add saved match history and richer player progression.
- [ ] Add accessibility options for UI scale, contrast, and input sensitivity.

### P3: XR Polish

- [ ] Improve Quest onboarding and spatial UI comfort settings.
- [ ] Expand haptic feedback profiles for card, button, and table interactions.
- [ ] Profile Quest performance on a representative range of table states.
- [ ] Add a guided VR playtest checklist and capture regression notes.

## Engineering Rules

- Keep Core free of `UnityEngine`, `MonoBehaviour`, PlayFab, and XR references.
- Put game rules in `Assets/Project/Scripts/Core/`.
- Put Unity presentation and interaction code in `Assets/Project/Scripts/Presentation/`.
- Keep PlayFab integrations behind interfaces in `Assets/Project/Scripts/Cloud/`.
- Use the New Input System for player interactions.
- Register runtime dependencies through `Assets/Project/Scripts/Architecture/DI/GameLifetimeScope.cs`.
- Prefer deterministic EditMode tests for domain and service logic; use PlayMode tests for scene and integration behaviour.
- Do not introduce speculative systems, unsupported platform claims, or placeholder documentation links.

## Related Documentation

- [PROJECT_ARCHITECTURE.md](PROJECT_ARCHITECTURE.md): architectural decisions and layer boundaries.
- [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md): current folder and assembly layout.
- [PROJECT_ROADMAP.md](PROJECT_ROADMAP.md): milestone history and completion state.
- [README.md](../../../README.md): portfolio overview and playable builds.
