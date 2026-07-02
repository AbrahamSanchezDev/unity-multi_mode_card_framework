# Graph Report - Assets/Project  (2026-06-29)

## Corpus Check
- Corpus is ~10,463 words - fits in a single context window. You may not need a graph.

## Summary
- 327 nodes · 520 edges · 17 communities detected
- Extraction: 62% EXTRACTED · 38% INFERRED · 0% AMBIGUOUS · INFERRED: 195 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Blackjack Engine Logic|Blackjack Engine Logic]]
- [[_COMMUNITY_Solitaire Engine|Solitaire Engine]]
- [[_COMMUNITY_Blackjack Controller & Tests|Blackjack Controller & Tests]]
- [[_COMMUNITY_TexasHoldem & Deck|TexasHoldem & Deck]]
- [[_COMMUNITY_PlayFab Data Service|PlayFab Data Service]]
- [[_COMMUNITY_Domain God Nodes|Domain God Nodes]]
- [[_COMMUNITY_Engine Test Fixtures|Engine Test Fixtures]]
- [[_COMMUNITY_Hand Evaluator & Poker|Hand Evaluator & Poker]]
- [[_COMMUNITY_PlayFab Auth Service|PlayFab Auth Service]]
- [[_COMMUNITY_Card Data Model|Card Data Model]]
- [[_COMMUNITY_Shuffle Algorithm|Shuffle Algorithm]]
- [[_COMMUNITY_Blackjack View|Blackjack View]]
- [[_COMMUNITY_View Interface|View Interface]]
- [[_COMMUNITY_Auth Service Interface|Auth Service Interface]]
- [[_COMMUNITY_DI Lifetime Scope|DI Lifetime Scope]]
- [[_COMMUNITY_Cloud Save Interface|Cloud Save Interface]]
- [[_COMMUNITY_Mock Deck Test Helpers|Mock Deck Test Helpers]]

## God Nodes (most connected - your core abstractions)
1. `BlackjackEngineTests` - 35 edges
2. `SolitaireEngineTests` - 26 edges
3. `TexasHoldemTests` - 14 edges
4. `BlackjackView` - 11 edges
5. `BlackjackEngine` - 10 edges
6. `BlackjackTableController` - 10 edges
7. `DeckTests` - 10 edges
8. `MockBlackjackView` - 10 edges
9. `GameLifetimeScope (VContainer DI Root)` - 10 edges
10. `CardDataTests` - 9 edges

## Surprising Connections (you probably didn't know these)
- `Assembly Definitions Design Rationale` --rationale_for--> `GameLifetimeScope (VContainer DI Root)`  [INFERRED]
  Assets/Project/Documentation/PROJECT_ARCHITECTURE.md → Assets/Project/Scripts/Architecture/DI/GameLifetimeScope.cs
- `Layer-Based Script Organization Rationale` --rationale_for--> `BlackjackTableController`  [INFERRED]
  Assets/Project/Documentation/PROJECT_ARCHITECTURE.md → Assets/Project/Scripts/Presentation/Controllers/BlackjackTableController.cs
- `Deck` --semantically_similar_to--> `ShuffleAlgorithm (Fisher-Yates)`  [INFERRED] [semantically similar]
  Assets/Project/Scripts/Core/Models/Deck.cs → Assets/Project/Scripts/Core/Utils/ShuffleAlgorithm.cs
- `BlackjackEngine` --semantically_similar_to--> `SolitaireEngine`  [INFERRED] [semantically similar]
  Assets/Project/Scripts/Core/Engines/BlackjackEngine.cs → Assets/Project/Scripts/Core/Engines/SolitaireEngine.cs
- `BlackjackEngine` --semantically_similar_to--> `TexasHoldemEngine`  [INFERRED] [semantically similar]
  Assets/Project/Scripts/Core/Engines/BlackjackEngine.cs → Assets/Project/Scripts/Core/Engines/TexasHoldemEngine.cs

## Hyperedges (group relationships)
- **Blackjack MVC Flow** — BlackjackView_BlackjackView, IBlackjackView_IBlackjackView, BlackjackTableController_BlackjackTableController, BlackjackEngine_BlackjackEngine [EXTRACTED 0.95]
- **Cloud Service Interface Implementations** — PlayFabAuthService_PlayFabAuthService, IAuthenticationService_IAuthenticationService, PlayFabDataService_PlayFabDataService, ICloudSaveService_ICloudSaveService [EXTRACTED 0.95]
- **Core Engines Sharing Deck & CardData** — BlackjackEngine_BlackjackEngine, SolitaireEngine_SolitaireEngine, TexasHoldemEngine_TexasHoldemEngine, Deck_Deck, CardData_CardData [EXTRACTED 0.90]
- **Mock Test Double Pattern (Interface Mocking)** — MockPlayFabDataWrapper, MockBlackjackView, MockDeck [INFERRED 0.90]
- **CardData Domain Entity Consumers** — BlackjackEngineTests, SolitaireEngineTests, TexasHoldemTests, DeckTests, ShuffleAlgorithmTests, CardDataTests [INFERRED 0.95]
- **PlayFab Cloud Test Layer** — PlayFabDataServiceTests, PlayFabIntegrationTests, MockPlayFabDataWrapper, PlayFabAuthService, PlayFabDataService [INFERRED 0.90]

## Communities

### Community 0 - "Blackjack Engine Logic"

Cohesion: 0.08
Nodes (4): BlackjackEngine, CardFramework.Core.Engines, Hand, BlackjackEngineTests

### Community 1 - "Solitaire Engine"

Cohesion: 0.11
Nodes (4): CardFramework.Core.Engines, SolitaireEngine, CardFramework.Tests.EditMode.Core, SolitaireEngineTests

### Community 2 - "Blackjack Controller & Tests"

Cohesion: 0.11
Nodes (7): BlackjackTableController, CardFramework.Presentation.Controllers, BlackjackTableControllerTests, CardFramework.Tests.EditMode.Presentation, MockBlackjackView, IDisposable, IStartable

### Community 3 - "TexasHoldem & Deck"

Cohesion: 0.12
Nodes (6): CardFramework.Core.Models, Deck, CardFramework.Tests.EditMode.Core, DeckTests, CardFramework.Core.Engines, TexasHoldemEngine

### Community 4 - "PlayFab Data Service"

Cohesion: 0.09
Nodes (10): ICloudSaveService, IPlayFabDataWrapper, CardFramework.Cloud.PlayFab, DefaultPlayFabDataWrapper, IPlayFabDataWrapper, PlayFabDataService, CardFramework.Tests.EditMode.Cloud, MockPlayFabDataWrapper (+2 more)

### Community 5 - "Domain God Nodes"

Cohesion: 0.13
Nodes (24): BlackjackEngine, BlackjackEngine.GameState enum, BlackjackTableController, BlackjackView (MonoBehaviour), CardData (POCO struct), Deck, GameLifetimeScope (VContainer DI Root), HandEvaluator (+16 more)

### Community 6 - "Engine Test Fixtures"

Cohesion: 0.1
Nodes (23): Ace Soft/Hard Value Adjustment Logic, BlackjackEngineTests Test Fixture, BlackjackEngine.Hand (nested), BlackjackTableControllerTests Test Fixture, CardDataTests Test Fixture, DeckTests Test Fixture, HandRank Enum (Poker Hand Categories), MockBlackjackView (Mock IBlackjackView) (+15 more)

### Community 7 - "Hand Evaluator & Poker"

Cohesion: 0.17
Nodes (4): CardFramework.Core.Utils, HandEvaluator, CardFramework.Tests.EditMode.Core, TexasHoldemTests

### Community 8 - "PlayFab Auth Service"

Cohesion: 0.15
Nodes (6): IAuthenticationService, CardFramework.Cloud.PlayFab, PlayFabAuthService, CardFramework.Tests.PlayMode.Cloud, PlayFabIntegrationTests, TestSaveData

### Community 9 - "Card Data Model"

Cohesion: 0.17
Nodes (6): CardFramework.Core.Models, Equals(), GetHashCode(), ToString(), CardDataTests, CardFramework.Tests.EditMode.Core

### Community 10 - "Shuffle Algorithm"

Cohesion: 0.21
Nodes (4): CardFramework.Core.Utils, ShuffleAlgorithm, CardFramework.Tests.EditMode.Core, ShuffleAlgorithmTests

### Community 11 - "Blackjack View"

Cohesion: 0.17
Nodes (4): BlackjackView, CardFramework.Presentation.Views, IBlackjackView, MonoBehaviour

### Community 12 - "View Interface"

Cohesion: 0.25
Nodes (2): CardFramework.Presentation.Interfaces, IBlackjackView

### Community 13 - "Auth Service Interface"

Cohesion: 0.33
Nodes (2): CardFramework.Cloud.Interfaces, IAuthenticationService

### Community 14 - "DI Lifetime Scope"

Cohesion: 0.4
Nodes (3): CardFramework.Architecture.DI, GameLifetimeScope, LifetimeScope

### Community 15 - "Cloud Save Interface"

Cohesion: 0.4
Nodes (2): CardFramework.Cloud.Interfaces, ICloudSaveService

### Community 16 - "Mock Deck Test Helpers"

Cohesion: 0.4
Nodes (3): CardFramework.Tests.EditMode.Core, MockDeck, Deck

## Knowledge Gaps
- **39 isolated node(s):** `CardFramework.Architecture.DI`, `CardFramework.Cloud.Interfaces`, `CardFramework.Cloud.Interfaces`, `CardFramework.Cloud.PlayFab`, `CardFramework.Cloud.PlayFab` (+34 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `MockBlackjackView` connect `Blackjack Controller & Tests` to `Blackjack View`?**
  _High betweenness centrality (0.069) - this node is a cross-community bridge._
- **Why does `TexasHoldemTests` connect `Hand Evaluator & Poker` to `TexasHoldem & Deck`?**
  _High betweenness centrality (0.067) - this node is a cross-community bridge._
- **Why does `BlackjackEngineTests` connect `Blackjack Engine Logic` to `Mock Deck Test Helpers`, `Blackjack Controller & Tests`?**
  _High betweenness centrality (0.058) - this node is a cross-community bridge._
- **What connects `CardFramework.Architecture.DI`, `CardFramework.Cloud.Interfaces`, `CardFramework.Cloud.Interfaces` to the rest of the system?**
  _39 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Blackjack Engine Logic` be split into smaller, more focused modules?**
  _Cohesion score 0.08 - nodes in this community are weakly interconnected._
- **Should `Solitaire Engine` be split into smaller, more focused modules?**
  _Cohesion score 0.11 - nodes in this community are weakly interconnected._
- **Should `Blackjack Controller & Tests` be split into smaller, more focused modules?**
  _Cohesion score 0.11 - nodes in this community are weakly interconnected._