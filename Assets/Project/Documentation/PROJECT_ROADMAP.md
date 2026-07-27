# 🃏 Card Framework - Project Master Roadmap

## 🤖 AI Context & System Prompt

**Target AI / Developer Instructions:**
You are acting as a Senior Unity Developer. This document tracks the development of a cross-platform Card Game Framework (targeting WebGL and Meta Quest 3).

- **Architecture:** Strict decoupled MVP/MVC.
- **Dependency Injection:** VContainer (No global Singletons or `GameManager.Instance`).
- **Cloud Backend:** Microsoft PlayFab (Async/Await pattern using `TaskCompletionSource`, `IPlayFabDataWrapper` for test isolation).
- **Testing Standard:** Test-Driven Development (TDD). 100% Line Coverage required for Controllers and Services using NUnit (EditMode for unit, PlayMode for integration).
- **Language:** C# 9.0+ / Unity 6000+
- **UI System:** UI Toolkit (.uxml / .uss) designed with Responsive Panel Settings (Screen-Space for Flatscreen, prepared for seamless migration to World-Space Panels for Meta Quest 3).

All newly generated code must adhere strictly to these architectural boundaries. Update checkboxes `[x]` as tasks are completed.

---

## 📌 Epic 1: Core Logic Engine

_Pure C# simulation logic, deterministic and independent of Unity's MonoBehaviour loop._

- [x] **TASK-1.1: Blackjack Engine Implementation**
  - Hand evaluation, bust detection, dealer soft-17 rules.
- [x] **TASK-1.2: Texas Holdem Engine Base**
  - Core card tracking and simulation.
- [x] **TASK-1.3: Solitaire Engine Base**
  - Tableau, foundation, and stockpile rule enforcement.
- [x] **TASK-1.4: Core Data Models**
  - Standardized `CardData`, `Deck` shuffling, and generic enums.

---

## ☁️ Epic 2: Persistence & Cloud Backend

_PlayFab integration, data storage, and cross-device syncing pipelines._

- [x] **TASK-2.1: Dependency Injection Container**
  - Setup `VContainer` lifecycle (`GameLifetimeScope`).
  - Eradicate global Singletons.
- [x] **TASK-2.2: Cloud Infrastructure & Anonymous Authentication**
  - Implement `PlayFabAuthService` with device-unique fallback.
  - Implement `PlayFabDataService` with mocked wrapper for 0ms unit testing.
  - 100% Test Coverage (PlayMode & EditMode).

---

## 🖥️ Epic 3: Architecture & UI Foundation

_Screen-space canvases, decoupled presentation controllers, and input abstraction._

- [x] **TASK-3.3: UI Presentation Layer Architecture (MVP/MVC Setup)**
  - Define `IView` contracts (`IBlackjackView`).
  - Create POCO Controllers (`BlackjackTableController`) implementing VContainer's `IStartable`.
  - EditMode tests validating controller state machines.
- [x] **TASK-3.4: Dynamic Table UI Controller Implementation**
  - Map logic events to visual UI feedback loops.
  - Instantiate and animate physical/UI cards on draw actions.
  - Disengage logic handles cleanly upon view destruction.
- [x] **TASK-3.5: User Notification & Modal Window System**
  - Screen-space modal canvas for errors, loading overlays, and async cloud operations.
- [x] **TASK-3.6: Multi-Platform Input Adapter**
  - Implement abstraction layer (`IInputContext`) for platform-agnostic interactions.
  - Create standard pointer inputs adapter mapping core press definitions.
  - Establish registration hooks for future XR controller injection pipelines.
- [x] **TASK-3.7: Adaptive Screen-Space UI & Responsiveness**
  - Refactor USS sheets using flexbox auto-wrapping and relative dimensions (`flex-basis`).
  - Eliminate layout engine parser warnings by aligning properties to native sub-specs.
  - Implement dynamic local space auto-centering for 3D physical card hand layout structures.

---

## 🎮 EPIC-04: Cloud Infrastructure & Metagame

Expanding the framework to support persistence, cross-platform linking, and economy.

- [x] **TASK-4.1: PlayFab SDK & Silent Authentication**
  - Integrate PlayFab SDK extensions with decoupled injection safety mappings.
  - Formulate hardware-invariant `ICloudService` abstract definitions.
  - Implement zero-friction `CustomID` silent login execution workflows via `IInitializable` hooks.
- [x] **TASK-4.2: Economy & Betting System (Cloud Core)**
  - Map global currency code `GD` inside PlayFab GameManager with server-time passive recharge hooks.
  - Establish loose `IEconomyService` structures to prevent runtime client memory tampering.
  - Implement dynamic multi-project credit/debit cloud network sync pipelines.
  - Deliver automated EditMode test coverage adapters validating game loop wagering states.
    - [x] **TASK-4.2.1: Dynamic UI Wallet Balance (HUD Sync)**
      - Extend `IBlackjackView` contract to handle real-time cash flow signatures.
      - Bind reactive event subscriptions between `IEconomyService` and the main UI Toolkit canvas.
      - Secure clean data updates across round transitions (debits, standard payouts, natural blackjacks, and push resolutions).
- [x] **TASK-4.3: Cross-Platform Account Linking (Old TASK-2.3)**
  - Build asynchronous 6-character alphanumeric PIN system to sync WebGL state with Meta Quest 3.
    - [x] **TASK-4.3.1: Dashboard UI & Game Switcher Layout**
      - Design the central dashboard canvas in UXML/USS featuring a multi-game selection carousel and account status views.
      - Handle active scene context switching routines to seamlessly transition between Blackjack and secondary game slots.
- [x] **TASK-4.4: Cloud Mailbox & Anti-Cheat Cooldowns (Old TASK-2.4)**
  - Integrate server-verified rewards and prevent device-clock tampering via PlayFab Time/Title Data.
- [ ] **TASK-4.5: Alternative Game UI (Solitaire & Texas Hold'em Layouts)**
  - Table and drag-and-drop controllers for secondary game states.
  - [x] **TASK-4.5.1: 3D Physical Table Presentation & DOTween Motion**
    - Implement two-phase card dealing pipeline (Fly-to-Hand + Auto Re-Center Sequence via DOTween).
    - Prevent Z-fighting depth overlap through role-based incremental local Z-offset logic (player vs. dealer).
---

## 🥽 Epic 5: XR Integration & Deployment

_Meta Quest 3 spatial design and final deployment pipelines._

- [ ] **TASK-5.1: Meta Quest 3 Rig Setup**
  - Implement XR Interaction Toolkit or Oculus Integration for hand-tracking/controllers.
- [ ] **TASK-5.2: Spatial UI Adaptation**
  - Convert Screen-Space Canvases to World-Space Canvases for the VR environment.
- [ ] **TASK-5.3: WebGL Optimization & Build**
  - Compress assets, strip unused engine code, and finalize browser build compatibility.
