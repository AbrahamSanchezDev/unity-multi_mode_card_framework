# Engineering Backlog: Multi-Mode Card Framework

**Version**: 1.0  
**Last Updated**: June 2026  
**Status**: Active Development  
**Solo Developer Edition**

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Epic Summary](#epic-summary)
3. [EPIC-01: Core Logic Engine](#epic-01-core-logic-engine)
4. [EPIC-02: Persistence & Cloud Backend](#epic-02-persistence--cloud-backend)
5. [EPIC-03: Architecture & UI Foundation](#epic-03-architecture--ui-foundation)
6. [EPIC-04: 3D Visual Presentation](#epic-04-3d-visual-presentation)
7. [EPIC-05: XR Integration & Monetization](#epic-05-xr-integration--monetization)
8. [Development Timeline](#development-timeline)
9. [Dependencies & Critical Path](#dependencies--critical-path)
10. [Risk Register](#risk-register)
11. [Success Metrics](#success-metrics)

---

## Project Overview

**Multi-Mode Modular Card Framework**: A production-grade, multi-platform card game engine demonstrating senior-level architecture, decoupled MVC design patterns, and cross-platform deployment (WebGL, PC, Mobile, Meta Quest 3).

### Key Principles

- **Modularity**: Core C# logic completely decoupled from Unity rendering
- **Testability**: 85%+ code coverage via NUnit EditMode tests
- **Cross-Platform**: Single codebase, adaptive UI for all platforms
- **Zero Server Costs**: LootLocker backend-as-a-service for persistence
- **Solo Development**: Realistic timeline with burnout prevention strategies

### Project Metrics

| Metric                   | Value                                                       |
| ------------------------ | ----------------------------------------------------------- |
| **Total Epics**          | 5                                                           |
| **Total Tasks**          | 18                                                          |
| **Total Story Points**   | 114 SP                                                      |
| **Timeline (Part-time)** | 16-20 weeks @ 20-25 hrs/week                                |
| **Timeline (Full-time)** | 7-9 weeks @ 40 hrs/week                                     |
| **Platforms**            | WebGL, PC (Windows/Mac), Mobile (iOS/Android), Meta Quest 3 |
| **Backend**              | LootLocker (Free Tier)                                      |

---

## Epic Summary

| Epic ID                                          | Epic Name                     | Priority      | Story Points | Est. Days | Status         | Start Date | End Date |
| ------------------------------------------------ | ----------------------------- | ------------- | ------------ | --------- | -------------- | ---------- | -------- |
| [EPIC-01](#epic-01-core-logic-engine)            | Core Logic Engine             | P0 (Critical) | 28 SP        | 20d       | 🟡 Not Started | -          | -        |
| [EPIC-02](#epic-02-persistence--cloud-backend)   | Persistence & Cloud Backend   | P0 (Critical) | 21 SP        | 16d       | 🟡 Not Started | -          | -        |
| [EPIC-03](#epic-03-architecture--ui-foundation)  | Architecture & UI Foundation  | P0 (Critical) | 20 SP        | 15d       | 🟡 Not Started | -          | -        |
| [EPIC-04](#epic-04-3d-visual-presentation)       | 3D Visual Presentation        | P1 (High)     | 18 SP        | 14d       | 🟡 Not Started | -          | -        |
| [EPIC-05](#epic-05-xr-integration--monetization) | XR Integration & Monetization | P2 (Medium)   | 27 SP        | 14d       | 🟡 Not Started | -          | -        |
|                                                  | **TOTAL**                     |               | **114 SP**   | **79d**   |                |            |          |

**Status Legend**: 🟡 Not Started | 🔵 In Progress | ✅ Completed | 🔴 Blocked

---

## EPIC-01: Core Logic Engine

**Objective**: Build all game rules entirely in memory as pure C# logic (POCO) without Unity dependencies to enable TDD.

**Priority**: P0 (Blocking all other work)  
**Story Points**: 28 SP  
**Estimated Duration**: 20 developer-days (full-time)  
**Estimated Duration**: 5-6 weeks (part-time @ 20 hrs/week)  
**Dependencies**: None  
**Status**: 🟡 Not Started

### Success Criteria

- [ ] 100% of NUnit EditMode tests passing
- [ ] Zero MonoBehaviour references in Core assembly
- [ ] Hand evaluation algorithms optimized (<1µs per hand)
- [ ] Deck shuffling uses Fisher-Yates algorithm
- [ ] Blackjack and Solitaire rule engines complete

### Deliverables

```
Scripts/Core/
├── Models/
│   ├── CardData.cs
│   └── Deck.cs
├── Engines/
│   ├── BlackjackEngine.cs
│   ├── SolitaireEngine.cs
│   └── HandEvaluator.cs
├── Utils/
│   └── ShuffleAlgorithm.cs
└── Tests/EditMode/
    ├── CardEvaluationTests.cs
    ├── BlackjackTests.cs
    ├── SolitaireTests.cs
    └── DeckTests.cs
```

### Tasks

---

#### TASK-1.1: Base Data Structures Implementation (CardData, Deck)

**Description**: Create pure C# structures (POCO) to represent a card and deck. Implement Fisher-Yates shuffling algorithm.

| Attribute          | Value          |
| ------------------ | -------------- |
| **Status**         | 🟡 Not Started |
| **Story Points**   | 8 SP           |
| **Platforms**      | All            |
| **Priority**       | P0             |
| **Estimated Days** | 3-4d           |
| **Dependencies**   | None           |
| **Assignee**       | TBD            |

**Definition of Done**:

- [ ] CardData struct with Suit and Value properties
- [ ] Deck class with Initialize, Shuffle, Draw methods
- [ ] Fisher-Yates algorithm produces uniform randomness
- [ ] Unit tests verify deck size and shuffle behavior

**Deliverables**:

- `CardData.cs` - POCO card representation
- `Deck.cs` - Deck management with shuffling
- `CardEvaluationTests.cs` - Initial test suite

**Technical Notes**:

- Use bitwise flags for Suit/Value to optimize memory
- No texture/sprite references in CardData
- Return `null` on invalid Draw() calls (empty deck)

**Reference**: See [GDD Section 2.1 - Model Layer](../GDD_TSD.md#21-model-layer)

---

#### TASK-1.2: Blackjack (21) Rules and Hand Evaluator Engine

**Description**: Create BlackjackEngine (pure C# POCO, no MonoBehaviour). Implement Ace mechanics (1 or 11) and dealer AI (stand on soft 17).

| Attribute          | Value                                                                  |
| ------------------ | ---------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                         |
| **Story Points**   | 8 SP                                                                   |
| **Platforms**      | All                                                                    |
| **Priority**       | P0                                                                     |
| **Estimated Days** | 3-4d                                                                   |
| **Dependencies**   | [TASK-1.1](#task-11-base-data-structures-implementation-carddata-deck) |
| **Assignee**       | TBD                                                                    |

**Definition of Done**:

- [ ] Hand value calculator correctly handles all Ace combinations
- [ ] Dealer AI follows "stand on soft 17" rule consistently
- [ ] Busting detection works for all hand scenarios
- [ ] Unit tests cover 95%+ of hand scenarios

**Deliverables**:

- `BlackjackEngine.cs` - Game logic (pure C#, no MonoBehaviour)
- `HandValueCalculator.cs` - Hand evaluation utility
- `BlackjackTests.cs` - Comprehensive test suite

**Test Cases Required**:

```csharp
// Examples
TestHandValue_TwoAces_ShouldBe12()
TestHandValue_AceAndFace_ShouldBe21()
TestDealerMustStandOnSoft17()
TestBustingDetection()
TestTieHandling()
```

**Reference**: See [GDD Section 2.2 - Blackjack Mechanics](../GDD_TSD.md#21-texas-holdem-primary-mode)

---

#### TASK-1.3: Solitaire Structural Engine and Rules

**Description**: Create SolitaireEngine for rule validation—alternating colors and descending/ascending stacking.

| Attribute          | Value                                                                  |
| ------------------ | ---------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                         |
| **Story Points**   | 8 SP                                                                   |
| **Platforms**      | All                                                                    |
| **Priority**       | P0                                                                     |
| **Estimated Days** | 3-4d                                                                   |
| **Dependencies**   | [TASK-1.1](#task-11-base-data-structures-implementation-carddata-deck) |
| **Assignee**       | TBD                                                                    |

**Definition of Done**:

- [ ] Move validator rejects invalid moves
- [ ] Move validator permits legal moves
- [ ] Win condition detected when all cards in Foundations
- [ ] Unit tests cover all move validation logic

**Deliverables**:

- `SolitaireEngine.cs` - Game logic
- `MoveValidator.cs` - Move legality checking
- `SolitaireTests.cs` - Comprehensive test suite

**Test Cases Required**:

```csharp
TestCanPlaceCardOnTableau_ValidColor() // Alternating colors
TestCanPlaceCardOnTableau_InvalidColor() // Reject same color
TestCanPlaceCardOnFoundation_AscendingOrder() // Ace → King
TestWinCondition_AllCardsInFoundations()
TestInvalidMoveAttempts()
```

**Reference**: See [GDD Section 2.4 - Solitaire](../GDD_TSD.md#24-solitaire)

---

#### TASK-1.4: EditMode Unit Testing Suite (NUnit)

**Description**: Set up NUnit in Tests/EditMode folder. Write comprehensive unit tests for all Core logic.

| Attribute          | Value                                                                                                                                                                                                 |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                                                                                                                                                        |
| **Story Points**   | 4 SP                                                                                                                                                                                                  |
| **Platforms**      | All                                                                                                                                                                                                   |
| **Priority**       | P0                                                                                                                                                                                                    |
| **Estimated Days** | 2-3d                                                                                                                                                                                                  |
| **Dependencies**   | [TASK-1.1](#task-11-base-data-structures-implementation-carddata-deck), [TASK-1.2](#task-12-blackjack-21-rules-and-hand-evaluator-engine), [TASK-1.3](#task-13-solitaire-structural-engine-and-rules) |
| **Assignee**       | TBD                                                                                                                                                                                                   |

**Definition of Done**:

- [ ] Unity Test Runner executes all tests in <5 seconds
- [ ] 100% test pass rate
- [ ] Minimum 85% code coverage on Core assembly
- [ ] No scene instantiation or MonoBehaviour usage in tests

**Deliverables**:

- `Tests/EditMode/CardEvaluationTests.cs`
- `Tests/EditMode/BlackjackTests.cs`
- `Tests/EditMode/SolitaireTests.cs`
- `Tests/EditMode/DeckTests.cs`
- Coverage report (OpenCover)

**Test Configuration**:

```
Assets/Tests/EditMode/
├── Core/
│   ├── CardEvaluationTests.cs
│   ├── BlackjackTests.cs
│   ├── SolitaireTests.cs
│   └── DeckTests.cs
└── Project.Tests.asmdef
```

**CI Integration**:

- Every commit runs: `Unity -runTests -testPlatform editmode`
- Build fails if coverage < 85%
- Build fails if any test fails

**Reference**: See [GDD Section 3.2 - Unit Testing Strategy](../GDD_TSD.md#32-unit-testing-strategy)

---

## EPIC-02: Persistence & Cloud Backend

**Objective**: Connect to LootLocker cloud for secure, cross-platform progress retention (zero fixed server costs).

**Priority**: P0 (Blocking multiplayer and monetization)  
**Story Points**: 21 SP  
**Estimated Duration**: 16 developer-days (full-time)  
**Estimated Duration**: 4-5 weeks (part-time @ 20 hrs/week)  
**Dependencies**: [EPIC-01](#epic-01-core-logic-engine)  
**Status**: 🟡 Not Started

### Success Criteria

- [ ] Silent guest login works on all platforms
- [ ] Cross-progression functional via 8-digit PIN
- [ ] Daily rewards claim working end-to-end
- [ ] Server-side clock tampering protection active
- [ ] Cloud save persists across devices

### Deliverables

```
Scripts/Cloud/
├── LootLockerManager.cs
├── CloudSaveHandler.cs
├── DeviceIdentifierService.cs
├── PINGeneratorService.cs
├── AccountLinkingUI.cs
├── LinkingValidator.cs
├── MailboxManager.cs
├── DailyBonusValidator.cs
└── RewardClaimHandler.cs
```

### Tasks

---

#### TASK-2.1: LootLocker SDK Integration and Silent Guest Login

**Description**: Import LootLocker SDK. Implement injectable LootLockerManager with device-persistent guest authentication.

| Attribute          | Value          |
| ------------------ | -------------- |
| **Status**         | 🟡 Not Started |
| **Story Points**   | 8 SP           |
| **Platforms**      | All            |
| **Priority**       | P0             |
| **Estimated Days** | 4-5d           |
| **Dependencies**   | None           |
| **Assignee**       | TBD            |

**Definition of Done**:

- [ ] LootLocker SDK imported and configured
- [ ] Players authenticate silently on app launch
- [ ] Device ID persists across app restarts
- [ ] Authentication works offline (queues actions)
- [ ] No password entry required

**Deliverables**:

- `LootLockerManager.cs` - Singleton wrapper for SDK
- `DeviceIdentifierService.cs` - Device ID management
- LootLocker API wrapper with error handling

**Integration Points**:

- Cloud storage endpoints
- Player profile endpoints
- Authentication endpoints

**API Reference**: https://docs.lootlocker.io/

**Reference**: See [GDD Section 4.1 - Cross-Progression](../GDD_TSD.md#41-cross-progression-unified-accounts)

---

#### TASK-2.2: Asynchronous Short-Code Account Linking (Cross-Progression)

**Description**: Implement Unified Player Accounts flow—PIN generation on VR/WebGL, linking to primary account.

| Attribute          | Value                                                                  |
| ------------------ | ---------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                         |
| **Story Points**   | 8 SP                                                                   |
| **Platforms**      | All                                                                    |
| **Priority**       | P0                                                                     |
| **Estimated Days** | 4-5d                                                                   |
| **Dependencies**   | [TASK-2.1](#task-21-lootlocker-sdk-integration-and-silent-guest-login) |
| **Assignee**       | TBD                                                                    |

**Definition of Done**:

- [ ] PIN generates on secondary device (8 alphanumeric digits)
- [ ] PIN expires after 10 minutes
- [ ] User enters PIN on primary account web portal
- [ ] Profiles merge; progression data shared across devices
- [ ] Linking validated server-side

**Deliverables**:

- `PINGeneratorService.cs` - PIN creation and validation
- `AccountLinkingUI.cs` - Linking UI screen
- `LinkingValidator.cs` - Server-side linking verification

**Flow**:

```
1. Secondary Device (VR/WebGL):
   - User clicks "Link Account"
   - PINGeneratorService generates 8-digit code
   - Display PIN on screen

2. Primary Device (PC/Mobile):
   - User visits web portal
   - Enters PIN
   - Server validates PIN + merges accounts

3. Both Devices:
   - Progression now unified
   - Same chip balance, level, cosmetics
```

**Security Notes**:

- PIN valid for 10 minutes only
- One-time use (consumed after linking)
- Server-side validation required
- OAuth (Google/Steam/Apple) prevents unauthorized linking

**Reference**: See [GDD Section 4.1 - Cross-Progression](../GDD_TSD.md#41-cross-progression-unified-accounts)

---

#### TASK-2.3: Cloud-Driven Mailbox and Daily Reward Inbox System

**Description**: Connect UI to LootLocker Messages. Implement server-side 24-hour cooldown for daily bonus.

| Attribute          | Value                                                                  |
| ------------------ | ---------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                         |
| **Story Points**   | 5 SP                                                                   |
| **Platforms**      | All                                                                    |
| **Priority**       | P0                                                                     |
| **Estimated Days** | 3-4d                                                                   |
| **Dependencies**   | [TASK-2.1](#task-21-lootlocker-sdk-integration-and-silent-guest-login) |
| **Assignee**       | TBD                                                                    |

**Definition of Done**:

- [ ] Client parses remote mail items
- [ ] Reward tokens awarded via cloud (not client-side)
- [ ] Claim action blocked on strict 24-hour server cooldown
- [ ] No client clock manipulation possible
- [ ] Mailbox syncs every 5 minutes

**Deliverables**:

- `MailboxManager.cs` - Mailbox UI controller
- `DailyBonusValidator.cs` - Server-side cooldown validation
- `RewardClaimHandler.cs` - Claim processing

**Message Payload Example**:

```json
{
  "message_id": "daily_bonus_2026_06_19",
  "reward_type": "chips",
  "amount": 1000,
  "expires_at": "2026-06-20T12:00:00Z",
  "claimed": false
}
```

**Security**:

- Server tracks last claim timestamp
- Client cannot modify device clock to bypass cooldown
- Rewards injected server-side, not client-side
- Message deletion prevents duplicate claiming

**Reference**: See [GDD Section 4.2 - Cloud Mailbox System](../GDD_TSD.md#42-cloud-mailbox-system-in-game-inbox)

---

## EPIC-03: Architecture & UI Foundation

**Objective**: Set up MVC design pattern with DI, wiring systems with low coupling.

**Priority**: P0 (Foundation for gameplay systems)  
**Story Points**: 20 SP  
**Estimated Duration**: 15 developer-days (full-time)  
**Estimated Duration**: 4 weeks (part-time @ 20 hrs/week)  
**Dependencies**: [EPIC-01](#epic-01-core-logic-engine), [EPIC-02](#epic-02-persistence--cloud-backend)  
**Status**: 🟡 Not Started

### Success Criteria

- [ ] Zero global singletons (no `FindObjectOfType`)
- [ ] All dependencies injectable via DI container
- [ ] UI adapts seamlessly between flat-screen and VR
- [ ] Input maps consistently across platforms
- [ ] No code duplication between screen-space and world-space UI

### Deliverables

```
Scripts/Presentation/
├── Controllers/
│   ├── GameModeController.cs (abstract base)
│   ├── BlackjackController.cs
│   ├── SolitaireController.cs
│   └── TableController.cs
├── Views/
│   ├── CardView.cs
│   ├── ChipView.cs
│   └── PlayerStatusView.cs
├── UI/
│   ├── AdaptiveUIManager.cs
│   ├── ScreenSpaceUILayout.cs
│   └── WorldSpaceUIAdapter.cs
└── Input/
    ├── PlatformInputAdapter.cs
    └── GameplayActions.inputactions

Scripts/Core/
└── DependencyContainer.cs
```

### Tasks

---

#### TASK-3.1: Dependency Injection (DI) Container Setup

**Description**: Set up lightweight DependencyContainer (or VContainer/Zenject). Register all services.

| Attribute          | Value          |
| ------------------ | -------------- |
| **Status**         | 🟡 Not Started |
| **Story Points**   | 8 SP           |
| **Platforms**      | All            |
| **Priority**       | P0             |
| **Estimated Days** | 2-3d           |
| **Dependencies**   | None           |
| **Assignee**       | TBD            |

**Definition of Done**:

- [ ] DependencyContainer implemented (custom or via VContainer)
- [ ] LootLockerManager injected into controllers
- [ ] EconomyModel injected into controllers
- [ ] Zero usage of `FindObjectOfType` or global singletons
- [ ] Testable: Mock services can be injected

**Deliverables**:

- `Core/DependencyContainer.cs` - DI container implementation
- `Presentation/SceneContextInitializer.cs` - Scene setup
- Example: `BlackjackController` constructor takes `INetworkService`, `IEconomyService`

**Example Code**:

```csharp
public class BlackjackController : MonoBehaviour
{
    private INetworkService _network;
    private IEconomyService _economy;

    public void Initialize(INetworkService network, IEconomyService economy)
    {
        _network = network;
        _economy = economy;
    }
}

// In SceneContextInitializer.cs:
var container = new DependencyContainer();
container.Register<INetworkService>(new LootLockerNetworkService());
container.Register<IEconomyService>(new EconomyService());
var controller = container.Instantiate<BlackjackController>();
```

**Configuration Strategy**:

- Scene-based registration in `SceneContextInitializer.Awake()`
- Or assembly-scanned registration via attributes
- Choose one approach and document in README

**Reference**: See [GDD Section 3.1 - Dependency Injection Architecture](../GDD_TSD.md#31-dependency-injection-architecture)

---

#### TASK-3.2: New Input System Action Asset Mapping (Flat-Screen Platforms)

**Description**: Create `.inputactions` asset for flat-screen platforms only (Mouse, Touch, Gamepad).

| Attribute          | Value             |
| ------------------ | ----------------- |
| **Status**         | 🟡 Not Started    |
| **Story Points**   | 5 SP              |
| **Platforms**      | WebGL, PC, Mobile |
| **Priority**       | P0                |
| **Estimated Days** | 2-3d              |
| **Dependencies**   | None              |
| **Assignee**       | TBD               |

**Definition of Done**:

- [ ] `.inputactions` asset created with abstract action maps
- [ ] Mouse input (PC/WebGL) mapped to action maps
- [ ] Touch input (Mobile) mapped to same action maps
- [ ] Gamepad input mapped to same action maps
- [ ] All platforms use identical action map names

**Deliverables**:

- `Input/GameplayActions.inputactions` - Input action asset
- `Input/PlatformInputAdapter.cs` - Platform-specific adapter

**Action Map Structure**:

```
Gameplay/
├── Select (Button)
├── Cancel (Button)
├── Navigate (Value 2D)
├── Confirm (Button)

Menu/
├── Submit (Button)
├── Cancel (Button)
```

**Platform Bindings**:
| Action | PC | Mobile | Gamepad |
|--------|----|---------| --------|
| Select | Left Mouse | Touch Tap | Button South |
| Cancel | Right Mouse/Esc | Touch Back | Button East |
| Navigate | WASD / Mouse Delta | Touch Drag | Stick Left |

**Important Note**:

- Hand Tracking is **NOT** included here
- XR hand tracking handled in [EPIC-05 TASK-5.1](#task-51-meta-xr-input-system--wrist-anchor-ui-setup)
- Keep this task focused on flat-screen input only

**Reference**: See [GDD Section 2.5 - Input Systems](../GDD_TSD.md#25-input-systems)

---

#### TASK-3.3: Adaptive Hybrid UI Architecture (Screen-Space vs. World-Space)

**Description**: Design UI layouts for Shop, Mailbox, Menus. Swap between Screen-Space (flat) and World-Space (VR) on platform detection.

| Attribute          | Value                                                                            |
| ------------------ | -------------------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                                   |
| **Story Points**   | 7 SP                                                                             |
| **Platforms**      | All                                                                              |
| **Priority**       | P0                                                                               |
| **Estimated Days** | 5-6d                                                                             |
| **Dependencies**   | [TASK-3.2](#task-32-new-input-system-action-asset-mapping-flat-screen-platforms) |
| **Assignee**       | TBD                                                                              |

**Definition of Done**:

- [ ] UI layout identical in functionality on flat screens vs VR
- [ ] No code duplication between screen-space and world-space UI
- [ ] Platform detection automatic (no manual switching)
- [ ] Layout resizes correctly for all resolutions
- [ ] Touch/mouse/gamepad input works in both UI modes

**Deliverables**:

- `Presentation/UI/AdaptiveUIManager.cs` - Platform detection + swapping
- `Presentation/UI/ScreenSpaceUILayout.cs` - Screen-space layout component
- `Presentation/UI/WorldSpaceUIAdapter.cs` - World-space layout adapter

**Architecture**:

```
AdaptiveUIManager
├── DetectPlatform()
│   ├── If: WebGL/PC/Mobile → Instantiate ScreenSpaceLayout
│   └── If: Quest3 → Instantiate WorldSpaceLayout
├── ScreenSpaceLayout (Canvas in Screen Space)
│   ├── Shop Panel
│   ├── Mailbox Panel
│   └── Menu Panel
└── WorldSpaceLayout (Canvas in World Space)
    ├── Shop Panel (floats in front of player)
    ├── Mailbox Panel (floats above wrist)
    └── Menu Panel (floats at eye level)
```

**Key Principle**:

- Both layouts expose same public interface: `IUILayout`
- Controllers only interact with `IUILayout`, not specific implementation
- Swap seamlessly based on platform

**Reference**: See [GDD Section 6.1 & 6.2 - XR User Experience](../GDD_TSD.md#61-hybrid-card-interaction-model)

---

## EPIC-04: 3D Visual Presentation

**Objective**: Implement premium aesthetics, low-poly meshes, animations, and visual feedback.

**Priority**: P1 (High - portfolio impact)  
**Story Points**: 18 SP  
**Estimated Duration**: 14 developer-days (full-time)  
**Estimated Duration**: 3.5 weeks (part-time @ 20 hrs/week)  
**Dependencies**: [EPIC-01](#epic-01-core-logic-engine), [EPIC-03](#epic-03-architecture--ui-foundation)  
**Status**: 🟡 Not Started

### Success Criteria

- [ ] All cards animate smoothly with easing curves
- [ ] Particle effects trigger on game events
- [ ] 60fps stable on target desktop/mobile configs
- [ ] No memory leaks on long play sessions
- [ ] Premium shader effects render correctly on all platforms

### Deliverables

```
Assets/
├── Shaders/
│   ├── CardHolographic.shader
│   ├── FeeltTable.shader
│   └── ChipReflection.shader
├── Prefabs/
│   ├── Card3D.prefab
│   ├── Chip.prefab
│   └── Table/
├── Materials/
│   └── (Material instances)
└── Scripts/Presentation/
    ├── CardView.cs
    └── CardAnimationController.cs
```

### Tasks

---

#### TASK-4.1: URP Setup and Low-Poly Asset Optimization Pipeline

**Description**: Configure Universal Render Pipeline. Import and compress 3D assets (cards, chips, table).

| Attribute          | Value          |
| ------------------ | -------------- |
| **Status**         | 🟡 Not Started |
| **Story Points**   | 6 SP           |
| **Platforms**      | All            |
| **Priority**       | P1             |
| **Estimated Days** | 3-4d           |
| **Dependencies**   | None           |
| **Assignee**       | TBD            |

**Definition of Done**:

- [ ] URP configured in ProjectSettings
- [ ] All 3D assets imported (cards, chips, table)
- [ ] ASTC 6x6 compression applied to mobile/Quest textures
- [ ] DXT5 compression applied to desktop textures
- [ ] No memory leaks on 2-hour play sessions
- [ ] Shader compilation time < 30 seconds

**Deliverables**:

- URP asset (`UniversalRP-HighQuality` or similar)
- Compressed texture atlases
- Material templates for cards/chips/table

**Asset Optimization**:

```
Compression Strategy:
├── Mobile/Quest3:
│   ├── Color textures: ASTC 6x6
│   └── Normal maps: ASTC 6x6
├── Desktop (Windows/Mac):
│   ├── Color textures: BC1/DXT1
│   └── Normal maps: BC4/DXT5
└── WebGL:
    ├── Color textures: ASTC 8x8 (fallback to RGB)
    └── Normal maps: ASTC 8x8
```

**Performance Targets**:

- Desktop: <50MB VRAM per table instance
- Mobile: <30MB VRAM per table instance
- Quest 3: <40MB VRAM per table instance
- WebGL: <20MB RAM per table instance

**Reference**: See [GDD Section 7.1 - Graphics Pipeline](../GDD_TSD.md#71-universal-render-pipeline-urp-configurations)

---

#### TASK-4.2: Card Component View Controller (CardView with DOTween)

**Description**: Create CardView MonoBehaviour. Subscribe to UnityEvents, animate card dealing with DOTween.

| Attribute          | Value                                                                                                                                           |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                                                                                                  |
| **Story Points**   | 8 SP                                                                                                                                            |
| **Platforms**      | All                                                                                                                                             |
| **Priority**       | P1                                                                                                                                              |
| **Estimated Days** | 4-5d                                                                                                                                            |
| **Dependencies**   | [TASK-1.1](#task-11-base-data-structures-implementation-carddata-deck), [TASK-4.1](#task-41-urp-setup-and-low-poly-asset-optimization-pipeline) |
| **Assignee**       | TBD                                                                                                                                             |

**Definition of Done**:

- [ ] CardView animates card from dealer to player positions
- [ ] Card flips smoothly during deal
- [ ] No stuttering or frame drops during animation
- [ ] DOTween easing curves feel premium
- [ ] Works correctly for all game modes

**Deliverables**:

- `CardView.cs` - MVC View component
- `CardAnimationController.cs` - Animation orchestration
- `Card3D.prefab` - Reusable card prefab

**Event Subscription Example**:

```csharp
public class CardView : MonoBehaviour
{
    // Subscribe to controller events
    void OnEnable()
    {
        GameController.Instance.OnCardDealt += AnimateDeal;
        GameController.Instance.OnCardFlipped += AnimateFlip;
    }

    void AnimateDeal(CardData card, int seatIndex)
    {
        // DOTween animation from dealer to seat
        transform.DOMove(GetSeatPosition(seatIndex), 0.3f)
                 .SetEase(Ease.OutQuad);
    }
}
```

**Animation Curves**:

- Deal: 300ms, OutQuad easing
- Flip: 200ms, InOutQuad easing
- Discard: 150ms, InQuad easing

**Reference**: See [GDD Section 4.2 - Card View Controller](../GDD_TSD.md#42-card-component-view-controller-cardview-with-dotween)

---

#### TASK-4.3: Special Card Shaders (Holographic/Rare Foil Effects)

**Description**: Design custom Shader Graph shader for premium card backs with holographic effect.

| Attribute          | Value                                                                   |
| ------------------ | ----------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                          |
| **Story Points**   | 4 SP                                                                    |
| **Platforms**      | All                                                                     |
| **Priority**       | P1                                                                      |
| **Estimated Days** | 2-3d                                                                    |
| **Dependencies**   | [TASK-4.1](#task-41-urp-setup-and-low-poly-asset-optimization-pipeline) |
| **Assignee**       | TBD                                                                     |

**Definition of Done**:

- [ ] Holographic shader renders premium card backs
- [ ] Effect reacts dynamically to camera position
- [ ] Shader compiles without errors on all platforms
- [ ] GPU time < 0.5ms per card rendered

**Deliverables**:

- `Shaders/HolographicCardShader.shadergraph` - Shader Graph implementation
- Material instance for premium cards

**Shader Features**:

- **Base Color**: Card back texture
- **Holographic Layer**: Iridescent effect that shifts with view angle
- **Fresnel**: Edge brightening based on camera angle
- **Animation**: Subtle scrolling UV coordinates for dynamic look

**Performance**:

- Mobile: <0.3ms GPU time
- Desktop: <0.5ms GPU time
- Quest 3: <0.4ms GPU time

**Platforms**:

- All platforms support Shader Graph (no platform-specific fallbacks needed)

**Reference**: See [GDD Section 7.1 - Graphics Pipeline](../GDD_TSD.md#71-universal-render-pipeline-urp-configurations)

---

## EPIC-05: XR User Experience

**Objective**: Adapt card engine for VR and wire up verified monetization via IAP.

**Priority**: P2 (Medium - stretch goal for additional platform support)  
**Story Points**: 27 SP  
**Estimated Duration**: 14 developer-days (full-time)  
**Estimated Duration**: 3.5 weeks (part-time @ 20 hrs/week)  
**Dependencies**: [EPIC-03](#epic-03-architecture--ui-foundation), [EPIC-04](#epic-04-3d-visual-presentation)  
**Status**: 🟡 Not Started

### Success Criteria

- [ ] Hand tracking functional at 90fps (reprojection)
- [ ] Wrist-anchor UI follows natural arm positioning
- [ ] Avatars sync with <50ms latency
- [ ] IAP receipts validate server-side
- [ ] No double-charging possible

### Deliverables

```
Scripts/
├── Cloud/
│   ├── IAPManager.cs
│   └── PurchaseValidator.cs
└── Presentation/
    └── XR/
        ├── MetaHandTrackingAdapter.cs
        ├── WristAnchorUIManager.cs
        ├── OVRInputMapper.cs
        ├── MetaNetServicesAdapter.cs
        ├── AvatarSyncController.cs
        ├── VoIPManager.cs
        └── SpatialAnchorManager.cs
```

### Tasks

---

#### TASK-5.1: Meta XR Input System & Wrist-Anchor UI Setup

**Description**: Integrate Meta Hand Tracking (OVRInput / Hand Tracking extension). Configure wrist-anchored UI and hand skeleton interaction.

| Attribute          | Value                                                                            |
| ------------------ | -------------------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                                   |
| **Story Points**   | 9 SP                                                                             |
| **Platforms**      | Quest 3                                                                          |
| **Priority**       | P2                                                                               |
| **Estimated Days** | 4-5d                                                                             |
| **Dependencies**   | [TASK-3.3](#task-33-adaptive-hybrid-ui-architecture-screen-space-vs-world-space) |
| **Assignee**       | TBD                                                                              |

**Definition of Done**:

- [ ] Hand skeleton tracked in real-time at 90fps
- [ ] Wrist-anchored UI follows natural arm positioning
- [ ] Players can rotate wrist to view hand privately
- [ ] Raycast detection works with hand proximity
- [ ] Poke interaction detects finger touch on VR buttons

**Deliverables**:

- `MetaHandTrackingAdapter.cs` - Hand tracking integration
- `WristAnchorUIManager.cs` - Wrist anchoring logic
- `OVRInputMapper.cs` - OVRInput to action mapping

**Implementation Details**:

**Hand Tracking**:

```csharp
// OVRInput API for hand tracking
var handPose = OVRInput.GetHandState(OVRInput.Hand.Right);
var indexTip = handPose.BoneRotations[OVRPlugin.BoneId.Hand_IndexTip];
```

**Wrist Anchor**:

```csharp
// Follow wrist transform
wristUICanvas.transform.parent = OVRPlugin.GetBoneTransform(BoneId.Hand_WristRoot);
wristUICanvas.transform.localPosition = new Vector3(0, 0.1f, 0.1f);
```

**Raycast Interaction**:

- Hand pointing (index finger extended) raycasts to detect UI elements
- Poke interaction (finger curled) detects collision with UI buttons

**Privacy**:

- Hand data only stored locally
- No hand skeleton sent to network
- Wrist rotation prevents other players from seeing card hand

**Reference**: See [GDD Section 6.1 - Hybrid Card Interaction Model](../GDD_TSD.md#61-hybrid-card-interaction-model)

---

#### TASK-5.2: Real-Time Avatar P2P Synchronization (Meta Net Services)

**Description**: Configure Meta Horizon Net Services P2P APIs. Stream head, hands, controller transforms using Meta Avatars.

| Attribute          | Value                                                            |
| ------------------ | ---------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                   |
| **Story Points**   | 7 SP                                                             |
| **Platforms**      | Quest 3                                                          |
| **Priority**       | P2                                                               |
| **Estimated Days** | 3-4d                                                             |
| **Dependencies**   | [TASK-5.1](#task-51-meta-xr-input-system--wrist-anchor-ui-setup) |
| **Assignee**       | TBD                                                              |

**Definition of Done**:

- [ ] Remote avatars render smoothly at 90fps (native or reprojected)
- [ ] Head and hand tracking latency < 50ms
- [ ] Voice chat functional and intelligible
- [ ] Avatars update position 60+ times per second

**Deliverables**:

- `MetaNetServicesAdapter.cs` - Meta Net Services integration
- `AvatarSyncController.cs` - Avatar synchronization
- `VoIPManager.cs` - Voice chat orchestration

**Networking Topology**:

```
P2P Peer 1 (Headset A)
    ↕ (Meta Net Services P2P)
P2P Peer 2 (Headset B)
    ↕
P2P Peer 3 (Headset C)

Separate Channel: LootLocker (Game state, turns, chips)
```

**Avatar Sync Flow**:

```
Local Transform Changes:
  Head, RightHand, LeftHand position/rotation
    ↓
  Serialize to transform data
    ↓
  Send via Meta Net Services P2P
    ↓
  Remote headset receives
    ↓
  Update remote avatar transforms
```

**VoIP Integration**:

- Meta Horizon voice automatically handles audio routing
- In-game VoIP enabled per session (no extra config needed)
- Player muting handled via Meta OS settings

**Reference**: See [GDD Section 6.2 - Network Topology for Avatars](../GDD_TSD.md#62-network-topology-for-avatars-and-visual-synchronization)

---

#### TASK-5.3: Shared Spatial Anchors Implementation (Co-located Local Play)

**Description**: Allow multiple Quest 3 headsets to download host's spatial anchor mapping. Align virtual table over real table.

| Attribute          | Value                                                            |
| ------------------ | ---------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                   |
| **Story Points**   | 4 SP                                                             |
| **Platforms**      | Quest 3                                                          |
| **Priority**       | P2                                                               |
| **Estimated Days** | 2-3d                                                             |
| **Dependencies**   | [TASK-5.1](#task-51-meta-xr-input-system--wrist-anchor-ui-setup) |
| **Assignee**       | TBD                                                              |

**Definition of Done**:

- [ ] Host headset scans room and creates spatial anchor
- [ ] Guest headsets download anchor cloud ID
- [ ] Virtual table renders at identical physical location on all headsets
- [ ] No spatial drift between headsets

**Deliverables**:

- `SpatialAnchorManager.cs` - Anchor management
- Anchor cloud ID serialization/deserialization

**Anchor Flow**:

```
Host (Headset A):
  1. Scan physical room → creates spatial anchor
  2. Uploads anchor to Meta Cloud
  3. Broadcasts anchor cloud ID to other players

Guest (Headsets B, C):
  1. Receive anchor cloud ID
  2. Download anchor from Meta Cloud
  3. Align coordinate system to downloaded anchor
  4. Virtual table renders at same physical location
```

**Use Case**:
Friends gathering around a physical table, each wearing Quest 3. Virtual poker table floats on top of real table, visible to all simultaneously.

**Reference**: See [GDD Section 6.2 - Co-located Local Multiplayer](../GDD_TSD.md#62-network-topology-for-avatars-and-visual-synchronization)

---

#### TASK-5.4: Unity IAP Integration and Server-Side Receipt Validation

**Description**: Implement Unity IAP. Configure $0.50 and $1.99 USD products. Validate receipts server-side via LootLocker.

| Attribute          | Value                                                                                                                                                    |
| ------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Status**         | 🟡 Not Started                                                                                                                                           |
| **Story Points**   | 7 SP                                                                                                                                                     |
| **Platforms**      | PC, Mobile, Quest 3                                                                                                                                      |
| **Priority**       | P2                                                                                                                                                       |
| **Estimated Days** | 3-4d                                                                                                                                                     |
| **Dependencies**   | [TASK-2.1](#task-21-lootlocker-sdk-integration-and-silent-guest-login), [TASK-3.3](#task-33-adaptive-hybrid-ui-architecture-screen-space-vs-world-space) |
| **Assignee**       | TBD                                                                                                                                                      |

**Definition of Done**:

- [ ] Players can purchase chip bundles from in-game shop
- [ ] Receipts validated server-side before chip credit
- [ ] No double-charging possible
- [ ] IAP works on all target platforms
- [ ] Test products configured in app stores

**Deliverables**:

- `IAPManager.cs` - Unity IAP integration
- `PurchaseValidator.cs` - Server-side validation
- In-app product configurations (Google Play, Apple App Store, Meta Store)

**Product Catalog**:
| SKU | Price | Chips | Platform |
|-----|-------|-------|----------|
| `chips_micro` | $0.50 | 50,000 | All |
| `chips_standard` | $1.99 | 250,000 | All |

**Purchase Flow**:

```
1. Client:
   - User clicks "Buy Chips"
   - Initiates IAP.InitiatePurchase("chips_standard")

2. App Store:
   - Platform handles payment
   - Returns receipt token

3. Client:
   - Sends encrypted receipt to server
   - Waits for server validation

4. Server (LootLocker):
   - Validates receipt with app store
   - Confirms transaction
   - Awards chips
   - Returns success

5. Client:
   - Updates local UI
   - Syncs chip balance
```

**Security**:

- Receipt tokens **never** exposed to client
- Server-side validation is authoritative
- Double-spend protection built into app stores
- Offline transactions queued and validated on reconnect

**Platform Setup**:

- Google Play: Configure in Play Console
- Apple App Store: Configure in App Store Connect
- Meta Store: Configure in Meta App Center

**Testing**:

- Use sandbox/test products during development
- Real IAP testing on actual devices before launch

**Reference**: See [GDD Section 5.2 - Technical Purchase Architecture](../GDD_TSD.md#52-technical-purchase-architecture)

---

## Development Timeline

### Recommended Sprint Schedule (Solo Developer)

**Part-Time (@20-25 hrs/week)**:

- Phase 1: Weeks 1-4 (EPIC-01) — 100 hours
- Phase 2: Weeks 5-8 (EPIC-02, EPIC-03) — 100 hours
- Phase 3: Weeks 9-11 (EPIC-04) — 75 hours
- Phase 4: Weeks 12-15 (EPIC-05) — 100 hours
- Phase 5: Weeks 16-20 (Integration, Polish, Testing) — 100 hours
- **Total: 16-20 weeks**

**Full-Time (@40 hrs/week)**:

- Phase 1: Weeks 1-2 (EPIC-01) — 80 hours
- Phase 2: Weeks 3-4 (EPIC-02, EPIC-03) — 80 hours
- Phase 3: Weeks 5-6 (EPIC-04) — 80 hours
- Phase 4: Weeks 7-8 (EPIC-05) — 80 hours
- Phase 5: Weeks 9+ (Integration, Polish, Testing) — variable
- **Total: 7-9 weeks**

### Velocity Assumptions

- **Weekly Capacity**: 6-7 SP/week (part-time) or 16-20 SP/week (full-time)
- **Total Project**: 114 SP
- **Part-Time Timeline**: 114 ÷ 6.5 = ~17-18 weeks (+ buffer = 16-20 weeks)
- **Full-Time Timeline**: 114 ÷ 18 = ~6 weeks (+ buffer = 7-9 weeks)

### Context Switching Strategy

Alternate task types to maintain mental engagement:

- **Week 1-2**: TASK-1.1 (system design) → [Let builds compile] → TASK-2.1 (API integration)
- **Week 3**: TASK-1.2 (logic) + TASK-1.3 (logic) — alternate between them
- **Week 4**: TASK-1.4 (testing) — consolidate learnings
- **Week 5**: TASK-2.2 (UI) + TASK-2.3 (cloud) — UI vs backend
- **Week 6**: TASK-3.1 (DI) + TASK-3.2 (input) — architecture vs configuration
- **Week 7**: TASK-3.3 (UI adaptation) — visual work
- **Week 8**: TASK-4.1 (URP) + TASK-4.2 (animation) — setup then creative
- **Week 9**: TASK-4.3 (shaders) — pure creative, mental break
- **Week 10+**: EPIC-05 tasks — VR work (requires learning, good for end-of-project)

---

## Dependencies & Critical Path

### Dependency Graph

```
TASK-1.1 (Base Data Structures)
├── TASK-1.2 (Blackjack Engine)
├── TASK-1.3 (Solitaire Engine)
│   └── TASK-1.4 (Unit Tests)
└── TASK-2.1 (LootLocker) [can run parallel]
    ├── TASK-2.2 (Account Linking)
    └── TASK-2.3 (Mailbox)

TASK-2.1 + TASK-1.1
└── TASK-3.1 (DI Container)
    └── TASK-3.2 (Input System)
        └── TASK-3.3 (Adaptive UI)

TASK-1.1 + TASK-3.3
├── TASK-4.1 (URP)
│   ├── TASK-4.2 (CardView)
│   └── TASK-4.3 (Shaders)
└── [Parallel: EPIC-04]

TASK-3.3 + TASK-4.1
├── TASK-5.1 (Meta XR Input)
│   ├── TASK-5.2 (Avatar Sync)
│   └── TASK-5.3 (Spatial Anchors)
└── TASK-2.1
    └── TASK-5.4 (Unity IAP)
```

### Critical Path (Longest Sequence)

```
TASK-1.1 → TASK-1.2 → TASK-1.4 → EPIC-01 Complete
    ↓
TASK-2.1 → TASK-2.2 → EPIC-02 Complete
    ↓
TASK-3.1 → TASK-3.2 → TASK-3.3 → EPIC-03 Complete
    ↓
TASK-4.1 → TASK-4.2 → EPIC-04 Complete
    ↓
TASK-5.1 → TASK-5.2 → EPIC-05 Complete
```

### Parallel Work Opportunities

Can work simultaneously:

- TASK-1.2 & TASK-1.3 (independent game engines)
- TASK-2.1 & TASK-1.4 (cloud setup & unit testing)
- TASK-3.1 & TASK-4.1 (DI container & URP setup)
- TASK-4.2 & TASK-4.3 (card animations & shaders)

---

## Risk Register

| ID    | Risk                                            | Severity | Likelihood | Mitigation                                                         |
| ----- | ----------------------------------------------- | -------- | ---------- | ------------------------------------------------------------------ |
| R-001 | LootLocker API breaking changes                 | High     | Low        | Pin SDK version; maintain API wrapper; monitor releases            |
| R-002 | Meta XR SDK incompatibility with Unity 2022 LTS | High     | Medium     | Test on actual Quest 3 early (Week 2-3); fallback to flat-screen   |
| R-003 | Performance regression (VR fails 90fps target)  | High     | Medium     | Profile continuously; implement LOD system; batch draw calls       |
| R-004 | **Burnout from solo development**               | Medium   | Medium     | Break into 2-3 day task chunks; 1-2 week breaks per phase          |
| R-005 | Scope creep (more game variants requested)      | Medium   | High       | Enforce strict epic boundaries; document must-have vs nice-to-have |
| R-006 | Cross-platform shader compilation failures      | Medium   | Medium     | Test on actual devices early (Week 1); use ShaderVariantCollection |
| R-007 | Multiplayer state sync desynchronization        | High     | Medium     | Extensive PlayMode testing; implement rollback/retry logic         |
| R-008 | **No code review feedback (solo)**              | Low      | High       | Self-review checklist; GitHub PR self-review (24hr rule)           |

### Burnout Prevention

- ✅ Time-box tasks to 2-3 days max
- ✅ Alternate work types (logic → UI → graphics)
- ✅ Take 1-2 week breaks after each epic
- ✅ Join game dev Discord for community
- ✅ Record milestone videos and share progress

---

## Success Metrics

### Project-Level KPIs

| Metric                    | Target                        | Measurement          |
| ------------------------- | ----------------------------- | -------------------- |
| **Code Coverage**         | 85%+ on Core                  | NUnit + OpenCover    |
| **Performance (Desktop)** | 60 FPS @ 1920x1080            | Unity Profiler       |
| **Performance (Mobile)**  | 30 FPS @ native res           | Device metrics       |
| **Performance (VR)**      | 90 FPS reprojected, 45 native | XR diagnostics       |
| **Build Size (WebGL)**    | <50MB uncompressed            | Build Report         |
| **Cross-Platform Sync**   | Zero desync incidents         | Manual QA testing    |
| **Uptime**                | 99%+ availability             | LootLocker dashboard |
| **Portfolio Quality**     | Positive recruiter feedback   | Technical interviews |

### Epic-Level Acceptance

- ✅ **EPIC-01**: All tasks done; 100% test pass; >85% coverage
- ✅ **EPIC-02**: Guest login, PIN linking, mailbox all working end-to-end
- ✅ **EPIC-03**: Zero singletons; DI injecting all deps; UI works on all platforms
- ✅ **EPIC-04**: Cards animate smoothly; effects render; 60fps stable
- ✅ **EPIC-05**: Hand tracking @ 90fps; avatars sync <50ms; IAP validates

---

## Development Notes

### Git Workflow

```bash
# Create feature branch for each task
git checkout -b feature/TASK-1.1-base-data-structures

# Commit format
git commit -m "TASK-1.1: Implement CardData and Deck structures with Fisher-Yates"

# Link to this backlog in PR
git push origin feature/TASK-1.1-base-data-structures
# Then open PR referencing this backlog
```

### Daily Standup Template

Each morning, log progress:

```markdown
## [Date] Standup

**Yesterday**:

- [ ] Completed X
- [ ] Progressed on Y

**Today**:

- [ ] Plan to work on Z
- [ ] Estimated completion

**Blockers**:

- None / Description of blocker
```

### Architecture Decision Log (ADL)

Document major decisions in `docs/ADL.md`:

```markdown
## ADL-001: Pure C# Core Layer

**Decision**: Core game logic lives in pure C# (POCO) classes, not MonoBehaviours.

**Rationale**:

- Enables unit testing without scene instantiation
- Reusable across platforms and future projects
- Clean separation of concerns

**Consequences**:

- Controllers must adapt Core output to MonoBehaviours
- No direct access to Unity Transform/Rigidbody in Core
```

---

## References

- **GDD/TSD**: See `Multi_Mode_Card_Framework_GDD_TSD.md` for design details
- **LootLocker Docs**: https://docs.lootlocker.io/
- **Unity Input System**: https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/
- **Meta XR SDK**: https://developers.meta.com/horizon/documentation/
- **DOTween**: http://dotween.demigiant.com/documentation.php

---

## Quick Links

| Section                | Link                                     |
| ---------------------- | ---------------------------------------- |
| GDD/TSD                | `./Multi_Mode_Card_Framework_GDD_TSD.md` |
| Architecture Decisions | `./docs/ADL.md`                          |
| Testing Guide          | `./docs/TESTING.md`                      |
| Deployment             | `./docs/DEPLOYMENT.md`                   |

---

**Last Updated**: June 19, 2026  
**Next Review**: After EPIC-01 completion  
**Maintained By**: Solo Developer
