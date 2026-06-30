# 🃏 Card Framework - Project Master Roadmap

## 🤖 AI Context & System Prompt
**Target AI / Developer Instructions:**
You are acting as a Senior Unity Developer. This document tracks the development of a cross-platform Card Game Framework (targeting WebGL and Meta Quest 3). 
* **Architecture:** Strict decoupled MVP/MVC.
* **Dependency Injection:** VContainer (No global Singletons or `GameManager.Instance`).
* **Cloud Backend:** Microsoft PlayFab (Async/Await pattern using `TaskCompletionSource`, `IPlayFabDataWrapper` for test isolation).
* **Testing Standard:** Test-Driven Development (TDD). 100% Line Coverage required for Controllers and Services using NUnit (EditMode for unit, PlayMode for integration).
* **Language:** C# 9.0+ / Unity 6000+
* **UI System:** UI Toolkit (.uxml / .uss) designed with Responsive Panel Settings (Screen-Space for Flatscreen, prepared for seamless migration to World-Space Panels for Meta Quest 3).

All newly generated code must adhere strictly to these architectural boundaries. Update checkboxes `[x]` as tasks are completed.

---

## 📌 Epic 1: Core Logic Engine
*Pure C# simulation logic, deterministic and independent of Unity's MonoBehaviour loop.*

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
*PlayFab integration, data storage, and cross-device syncing pipelines.*

- [x] **TASK-2.1: Dependency Injection Container**
  - Setup `VContainer` lifecycle (`GameLifetimeScope`).
  - Eradicate global Singletons.
- [x] **TASK-2.2: Cloud Infrastructure & Anonymous Authentication**
  - Implement `PlayFabAuthService` with device-unique fallback.
  - Implement `PlayFabDataService` with mocked wrapper for 0ms unit testing.
  - 100% Test Coverage (PlayMode & EditMode).
- [ ] **TASK-2.3: Cross-Platform Account Linking**
  - Build asynchronous 6-character alphanumeric PIN system.
  - Sync game state from flatscreen (WebGL) to VR profiles (Meta Quest 3) with a 10-minute validity window.
- [ ] **TASK-2.4: Cloud Mailbox & Cooldown Systems**
  - Integrate server-verified message inbox for daily rewards.
  - Mitigate device-clock tampering using PlayFab Title Data/CloudScript.

---

## 🖥️ Epic 3: Architecture & UI Foundation
*Screen-space canvases, decoupled presentation controllers, and input abstraction.*

- [x] **TASK-3.3: UI Presentation Layer Architecture (MVP/MVC Setup)**
  - Define `IView` contracts (`IBlackjackView`).
  - Create POCO Controllers (`BlackjackTableController`) implementing VContainer's `IStartable`.
  - EditMode tests validating controller state machines.
- [ ] **TASK-3.4: Dynamic Table UI Controller Implementation**
  - Map logic events to visual UI feedback loops.
  - Instantiate and animate physical/UI cards on draw actions.
  - Disengage logic handles cleanly upon view destruction.
- [ ] **TASK-3.5: User Notification & Modal Window System**
  - Screen-space modal canvas for errors, loading overlays, and async cloud operations.
- [ ] **TASK-3.6: Multi-Platform Input Adapter**
  - Implement universal click/drag using Unity's New Input System.
  - Abstract coordinate conversions for VR pointers vs. Mouse clicks.
- [ ] **TASK-3.7: Adaptive Screen-Space UI**
  - Responsive layout scaling across 16:9 desktop and 19.5:9 mobile formats without distortion.

---

## 🎮 Epic 4: Extended Game Loops & Metagame
*Expanding the framework to support economy and alternative game modes.*

- [ ] **TASK-4.1: Economy & Betting System**
  - Logic for placing wagers, resolving payouts, and syncing currency via `ICloudSaveService`.
- [ ] **TASK-4.2: Texas Holdem UI Implementation**
  - Table controller for multiplayer/AI poker states.
- [ ] **TASK-4.3: Solitaire UI Implementation**
  - Drag-and-drop controller for tableau manipulation.

---

## 🥽 Epic 5: XR Integration & Deployment
*Meta Quest 3 spatial design and final deployment pipelines.*

- [ ] **TASK-5.1: Meta Quest 3 Rig Setup**
  - Implement XR Interaction Toolkit or Oculus Integration for hand-tracking/controllers.
- [ ] **TASK-5.2: Spatial UI Adaptation**
  - Convert Screen-Space Canvases to World-Space Canvases for the VR environment.
- [ ] **TASK-5.3: WebGL Optimization & Build**
  - Compress assets, strip unused engine code, and finalize browser build compatibility.