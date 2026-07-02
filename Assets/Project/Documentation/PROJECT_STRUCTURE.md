# Project Folder Structure - Multi-Mode Card Framework

```
Assets/_Project/
│
├── Scripts/                          # All C# code (organized by layer)
│   │
│   ├── Core/                         # Pure C# - NO MonoBehaviour (Assembly: Project.Core.asmdef)
│   │   ├── Core.asmdef               # Assembly Definition (no external dependencies)
│   │   │
│   │   ├── Models/
│   │   │   ├── CardData.cs           # POCO: Card representation
│   │   │   ├── Deck.cs               # POCO: Deck management
│   │   │   ├── Hand.cs               # POCO: Hand representation
│   │   │   └── GameState.cs          # POCO: Game state data
│   │   │
│   │   ├── Engines/                  # Pure game rule engines
│   │   │   ├── IGameEngine.cs        # Interface for all engines
│   │   │   ├── BlackjackEngine.cs    # Blackjack rules
│   │   │   ├── SolitaireEngine.cs    # Solitaire rules
│   │   │   └── TexasHoldemEngine.cs  # Poker rules (future)
│   │   │
│   │   ├── Utils/
│   │   │   ├── ShuffleAlgorithm.cs   # Fisher-Yates implementation
│   │   │   ├── HandEvaluator.cs      # Hand ranking logic
│   │   │   └── MathUtilities.cs      # Utility functions
│   │   │
│   │   └── Economy/
│   │       ├── EconomyModel.cs       # POCO: Economy state
│   │       ├── ChipCalculations.cs   # Economy math
│   │       └── WalletData.cs         # Player balance data
│   │
│   ├── Presentation/                 # MonoBehaviour Views & Controllers (Assembly: Project.Presentation.asmdef)
│   │   ├── Presentation.asmdef       # References: Project.Core
│   │   │
│   │   ├── Controllers/              # MVC Controllers (MonoBehaviour)
│   │   │   ├── GameModeController.cs     # Abstract base controller
│   │   │   ├── BlackjackController.cs    # Blackjack UI coordinator
│   │   │   ├── SolitaireController.cs    # Solitaire UI coordinator
│   │   │   ├── TableController.cs       # Table management
│   │   │   └── GameRootController.cs    # Scene root coordinator
│   │   │
│   │   ├── Views/                    # MVC Views (MonoBehaviour)
│   │   │   ├── CardView.cs           # 3D card view with animations
│   │   │   ├── ChipView.cs           # 3D chip view
│   │   │   ├── PlayerStatusView.cs   # Player status display
│   │   │   ├── HandDisplayView.cs    # Player hand display
│   │   │   └── BetDisplayView.cs     # Betting UI view
│   │   │
│   │   └── UI/                       # Screen & World Space UI
│   │       ├── AdaptiveUIManager.cs      # Platform detection & swapping
│   │       ├── ScreenSpaceUILayout.cs    # Desktop/Mobile UI (Canvas ScreenSpace)
│   │       ├── WorldSpaceUIAdapter.cs    # VR UI (Canvas World Space)
│   │       ├── Screens/
│   │       │   ├── MainMenuScreen.cs
│   │       │   ├── LobbyScreen.cs
│   │       │   ├── GameScreen.cs
│   │       │   ├── ShopScreen.cs
│   │       │   └── SettingsScreen.cs
│   │       └── Overlays/
│   │           ├── MailboxOverlay.cs
│   │           ├── ChipDisplayOverlay.cs
│   │           └── ChatOverlay.cs
│   │
│   ├── Input/                        # Input Handling (Assembly: Project.Input.asmdef)
│   │   ├── Input.asmdef              # References: Project.Core
│   │   │
│   │   ├── PlatformAdapters/
│   │   │   ├── IPlatformInput.cs             # Interface
│   │   │   ├── DesktopInputAdapter.cs       # PC/WebGL input
│   │   │   ├── MobileInputAdapter.cs        # Mobile touch input
│   │   │   └── VRInputAdapter.cs            # Meta XR input (basic)
│   │   │
│   │   ├── InputActionMaps.inputactions     # Unity Input System asset
│   │   └── InputManager.cs                  # Input orchestrator
│   │
│   ├── Cloud/                        # Backend Integration (Assembly: Project.Cloud.asmdef)
│   │   ├── Cloud.asmdef              # References: Project.Core, LootLocker SDK
│   │   │
│   │   ├── Interfaces/
│   │   │   ├── INetworkService.cs
│   │   │   ├── ICloudSave.cs
│   │   │   └── IEconomyService.cs
│   │   │
│   │   ├── LootLocker/
│   │   │   ├── LootLockerManager.cs      # SDK wrapper (Singleton in DI)
│   │   │   ├── LootLockerAPI.cs          # Direct API calls
│   │   │   └── LootLockerErrorHandler.cs # Error handling
│   │   │
│   │   ├── CloudSaveHandler.cs           # Persistence orchestration
│   │   ├── IAPManager.cs                 # In-App Purchase handling
│   │   ├── MailboxManager.cs             # Mailbox system
│   │   └── AccountLinkingService.cs      # Cross-progression logic
│   │
│   ├── XR/                           # Meta Quest Integration (Assembly: Project.XR.asmdef - OPTIONAL)
│   │   ├── XR.asmdef                 # References: Project.Core, Meta SDK
│   │   │
│   │   ├── Input/
│   │   │   ├── MetaHandTrackingAdapter.cs    # Hand skeleton tracking
│   │   │   ├── OVRInputMapper.cs             # OVRInput to action mapping
│   │   │   └── VRInputAdapter.cs             # VR-specific input
│   │   │
│   │   ├── UI/
│   │   │   ├── WristAnchorUIManager.cs       # Wrist-anchored UI
│   │   │   └── WorldSpaceUIAdapter.cs        # World space canvas handling
│   │   │
│   │   ├── Networking/
│   │   │   ├── MetaNetServicesAdapter.cs     # Meta Net Services P2P
│   │   │   ├── AvatarSyncController.cs       # Avatar synchronization
│   │   │   └── VoIPManager.cs                # Voice chat
│   │   │
│   │   └── Spatial/
│   │       ├── SpatialAnchorManager.cs       # Co-located play anchors
│   │       └── RoomScanManager.cs            # Room scanning
│   │
│   ├── DependencyInjection/          # DI Container (Assembly: Project.Core.asmdef)
│   │   ├── DependencyContainer.cs
│   │   ├── SceneContextInitializer.cs
│   │   └── ServiceLocator.cs         # Optional: for fallback
│   │
│   ├── Tests/                        # Unit & Integration Tests (Assembly: Project.Tests.asmdef)
│   │   ├── Tests.asmdef              # References: all other asmdef
│   │   │
│   │   ├── EditMode/                 # NUnit tests (no scene instantiation)
│   │   │   ├── Core/
│   │   │   │   ├── CardEvaluationTests.cs
│   │   │   │   ├── DeckTests.cs
│   │   │   │   ├── HandEvaluatorTests.cs
│   │   │   │   ├── BlackjackEngineTests.cs
│   │   │   │   └── SolitaireEngineTests.cs
│   │   │   ├── Utilities/
│   │   │   │   ├── ShuffleAlgorithmTests.cs
│   │   │   │   └── MathUtilitiesTests.cs
│   │   │   └── Economy/
│   │   │       └── ChipCalculationsTests.cs
│   │   │
│   │   ├── PlayMode/                 # Scene-based integration tests
│   │   │   ├── GameFlowTests.cs
│   │   │   ├── MultiplayerSyncTests.cs
│   │   │   └── InputSystemTests.cs
│   │   │
│   │   └── Mocks/                    # Mock services for testing
│   │       ├── MockNetworkService.cs
│   │       ├── MockEconomyService.cs
│   │       ├── MockCloudSave.cs
│   │       └── MockIAPService.cs
│   │
│   └── Utilities/                    # Generic utilities (Assembly: Project.Core.asmdef or shared)
│       ├── Singleton.cs              # Generic singleton base (avoid if possible!)
│       ├── ObjectPool.cs             # Object pooling
│       ├── EventSystem.cs            # Custom event handling
│       └── Extensions.cs             # Extension methods
│
├── Animations/                       # Animator Controllers & Animation Clips
│   ├── Cards/
│   │   ├── Card_Deal.anim
│   │   ├── Card_Flip.anim
│   │   └── CardAnimator.controller
│   ├── Chips/
│   │   ├── Chip_Toss.anim
│   │   └── ChipAnimator.controller
│   └── UI/
│       ├── UI_Fade.anim
│       └── UI_Scale.anim
│
├── Materials/                        # Material instances & Material Library
│   ├── Cards/
│   │   ├── Card_Standard.mat
│   │   ├── Card_Premium.mat
│   │   └── Card_Holographic.mat
│   ├── Chips/
│   │   ├── Chip_Gold.mat
│   │   ├── Chip_Silver.mat
│   │   └── Chip_Bronze.mat
│   ├── Table/
│   │   ├── FeltTable.mat
│   │   └── WoodTable.mat
│   └── UI/
│       └── UIOverlay.mat
│
├── Prefabs/                          # Reusable GameObject prefabs
│   │
│   ├── Cards/
│   │   ├── Card3D.prefab             # Reusable card with material swaps
│   │   └── CardBack_Premium.prefab
│   │
│   ├── Chips/
│   │   ├── Chip.prefab               # Animated chip with physics
│   │   └── ChipStack.prefab
│   │
│   ├── Table/
│   │   ├── PokerTable.prefab         # Complete poker table setup
│   │   ├── BlackjackTable.prefab
│   │   └── SolitaireTable.prefab
│   │
│   ├── Players/
│   │   ├── PlayerSeat.prefab         # Player position & indicators
│   │   └── DealerButton.prefab
│   │
│   ├── UI/
│   │   ├── ScreenSpaceCanvas.prefab
│   │   └── WorldSpaceCanvas.prefab
│   │
│   └── VFX/
│       ├── CardDealEffect.prefab
│       ├── ChipWinEffect.prefab
│       └── BetPlacedEffect.prefab
│
├── Shaders/                          # Shader Graph & HLSL shaders
│   ├── CardHolographic.shadergraph   # Premium card back effect
│   ├── FeeltTable.shader             # Felt surface with normal mapping
│   ├── ChipReflection.shader         # Metallic chip rendering
│   ├── CardGlow.shader               # Card highlight effect
│   └── UIOverlay.shader              # UI transparency effects
│
├── Scenes/                           # Scene files organized by purpose
│   ├── Initialization.unity          # Bootstrap scene (DI setup)
│   ├── MainMenu.unity
│   ├── Lobby.unity
│   ├── GameScene_Blackjack.unity
│   ├── GameScene_Solitaire.unity
│   ├── GameScene_Poker.unity         # Future
│   └── Editor/
│       ├── DemoGameFlow.unity        # Editor testing scene
│       └── ComponentTestbed.unity
│
├── Data/                             # Configuration & game data
│   ├── GameConfig.json               # Master game balance data
│   ├── CardDefinitions.json          # Card metadata
│   ├── EconomyConfig.json            # Economy settings
│   ├── LootLockerConfig.json         # Backend config
│   └── Localization/
│       ├── en.json
│       └── es.json
│
├── Resources/                        # Runtime-loaded assets (use sparingly!)
│   ├── Prefabs/                      # Dynamically instantiated prefabs
│   │   └── DynamicCard.prefab
│   └── Data/
│       └── GameConfig.json           # Can also load from Resources
│
├── Editor/                           # Editor-only scripts & tools
│   ├── Editor.asmdef                 # Assembly (references all others)
│   │
│   ├── Tools/
│   │   ├── CardAssetValidator.cs     # Validate card assets
│   │   ├── BuildConfiguration.cs     # Build setup automation
│   │   └── PerformanceProfiler.cs    # Performance monitoring tools
│   │
│   └── Windows/
│       ├── GameConfigWindow.cs       # Edit game config in editor
│       └── TestHarness.cs            # Run game tests from editor
│
├── Config/                           # Project configuration files
│   ├── ProjectSettings/              # (Already in root, reference only)
│   └── README.md                     # Setup instructions
│
└── Documentation/                    # Markdown documentation (optional)
    ├── ARCHITECTURE.md               # Architecture decisions
    ├── SETUP.md                      # Project setup guide
    └── DEBUGGING.md                  # Debugging tips

```

---

## Key Improvements Over Your Structure

### 1. **Assembly Definitions (AsmDef) - CRITICAL**

```
Project.Core.asmdef
├── References: None (pure C#)
├── Includes: Core/, DependencyInjection/, Utilities/

Project.Presentation.asmdef
├── References: Project.Core
├── Includes: Presentation/, Animations, Materials, Prefabs

Project.Input.asmdef
├── References: Project.Core
├── Includes: Input/

Project.Cloud.asmdef
├── References: Project.Core, (LootLocker SDK)
├── Includes: Cloud/

Project.XR.asmdef (OPTIONAL - only if targeting VR)
├── References: Project.Core, Project.Presentation, (Meta SDK)
├── Includes: XR/

Project.Tests.asmdef
├── References: All above + test frameworks
├── Includes: Tests/
```

**Why?**

- Faster compilation (Core compiles independently)
- Prevents circular dependencies
- Enables parallel dev (one person on Core, another on Presentation)
- Clear separation of concerns
- Essential for solo dev to catch issues early

---

### 2. **XR as Optional Assembly**

```csharp
// In Presentation.asmdef
{
  "name": "Project.Presentation",
  "references": ["Project.Core"],
  // NO Meta SDK reference here
}

// Separately, Project.XR.asmdef
{
  "name": "Project.XR",
  "references": ["Project.Core", "Project.Presentation"],
  "versionDefines": [
    { "name": "com.meta.xr.sdk", "expression": "1.0" }
  ]
  // Only compile if Meta SDK is present
}
```

**Why?**

- WebGL/Mobile builds don't include unused VR code
- Faster builds for flat-screen platforms
- Can develop VR separately
- Easier to disable VR for non-VR testing

---

### 3. **Scripts Organized by LAYER, not FEATURE**

❌ **Bad (Feature-based)**:

```
Scripts/
├── Blackjack/
│   ├── BlackjackEngine.cs
│   ├── BlackjackController.cs
│   └── BlackjackView.cs
├── Solitaire/
│   ├── SolitaireEngine.cs
│   ├── SolitaireController.cs
│   └── SolitaireView.cs
```

✅ **Good (Layer-based - this structure)**:

```
Scripts/
├── Core/          # Pure logic
│   ├── Engines/
│   │   ├── BlackjackEngine.cs
│   │   └── SolitaireEngine.cs
├── Presentation/  # Views & Controllers
│   ├── Controllers/
│   │   ├── BlackjackController.cs
│   │   └── SolitaireController.cs
```

**Why?**

- Reflects MVC architecture
- Easier to test (Core isolated)
- Reusable engines across future projects
- Core layer never knows about Views

---

### 4. **Input as Separate Assembly**

```
Scripts/Input/ (Project.Input.asmdef)
├── PlatformAdapters/
│   ├── IPlatformInput.cs
│   ├── DesktopInputAdapter.cs
│   ├── MobileInputAdapter.cs
│   └── VRInputAdapter.cs (basic - for TASK-3.2 only)
```

**Why?**

- VR input is NOT in this assembly
- TASK-3.2 handles Mouse/Touch/Gamepad → separate from VR hand tracking
- Hand tracking (TASK-5.1) in Project.XR.asmdef
- Clean separation between flat-screen and VR input

---

### 5. **Dedicated XR Folder Structure**

```
Scripts/XR/
├── Input/
│   ├── MetaHandTrackingAdapter.cs    # TASK-5.1
│   └── OVRInputMapper.cs              # TASK-5.1
├── UI/
│   └── WristAnchorUIManager.cs        # TASK-5.1
├── Networking/
│   ├── MetaNetServicesAdapter.cs      # TASK-5.2
│   └── AvatarSyncController.cs        # TASK-5.2
└── Spatial/
    └── SpatialAnchorManager.cs        # TASK-5.3
```

**Why?**

- All VR-specific code in one place
- Easy to exclude from non-VR builds
- Clear what's Meta XR vs generic
- Matches EPIC-05 task organization

---

### 6. **Better Test Organization**

```
Tests/
├── EditMode/        # Fast, no scenes, run on CI
│   ├── Core/        # Engine tests
│   ├── Utilities/   # Math/algorithm tests
│   └── Economy/     # Economy logic tests
├── PlayMode/        # Slow, requires scenes, local dev only
│   ├── GameFlowTests.cs
│   └── MultiplayerSyncTests.cs
└── Mocks/           # Shared across both
    ├── MockNetworkService.cs
    └── MockEconomyService.cs
```

**Why?**

- EditMode runs on every commit (fast feedback)
- PlayMode only on local testing (slower)
- Mocks reusable for both types
- Clear separation of concerns

---

### 7. **Data Folder for Non-Code Assets**

```
Data/
├── GameConfig.json            # Main game settings
├── CardDefinitions.json       # Card metadata
├── EconomyConfig.json         # Chip values, rewards
├── LootLockerConfig.json      # API keys, endpoints
└── Localization/
    ├── en.json               # English strings
    └── es.json               # Spanish strings
```

**Why?**

- Version control friendly (JSON not binary)
- Designers can edit without opening Unity
- Easy CI/CD integration for config validation
- Separate from Resources/ (no runtime overhead)

---

## Pro Tips

- ✅ Use `#if ENABLE_VR_BUILD` or `#if UNITY_EDITOR` to guard platform-specific code
- ✅ Never put MonoBehaviour in Core/
- ✅ Test Core layer code independently (it's pure C#)
- ✅ Use DI for all service injection (no FindObjectOfType!)
- ✅ Keep Prefabs/Table/ organized by game mode
- ✅ Use `Resources/` sparingly (runtime loading cost)
- ✅ Version control JSON data files, not JSON as text in code

---
