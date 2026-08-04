# Multi-Mode Card Framework

**A production-grade, modular card game engine demonstrating senior-level software architecture, decoupled MVC design patterns, and cross-platform deployment.**

<!-- [![Build Status](https://github.com/AbrahamSanchezDev/multi-mode-card-framework/workflows/CI/badge.svg)](https://github.com/AbrahamSanchezDev/multi-mode-card-framework/actions) -->

[![Code Coverage](https://img.shields.io/badge/coverage-85%25-brightgreen)](./docs/COVERAGE.md)
[![Unity Version](https://img.shields.io/badge/unity-6.3%20LTS-blue)](https://unity.com/download)

---

## 👀 EPIC 3 State Overview

![Epic 3 Preview Image](/ReadMePreviewImages/Epic_3.png)
![Epic 3 Preview](/ReadMePreviewImages/Epic_3.gif)

## 👀 EPIC 4 Current State Overview

![Epic 4 Preview](/ReadMePreviewImages/Epic_4.gif)
<video src="/ReadMePreviewImages/Epic_4.mp4" width="800" height="450" controls>
  Your browser does not support the video tag.
</video>

---

## 🎯 Project Overview

**Multi-Mode Card Framework** is a comprehensive, decoupled card game engine built with Unity that demonstrates professional game development practices. The core game logic is completely isolated from the presentation layer, enabling 100% reusability across platforms and future projects.

### What Makes This Different

- **Pure C# Core**: All game logic lives in POCO (Plain Old C# Objects) with zero Unity dependencies—enabling clean unit testing and reusability
- **True Modularity**: Core engine can be extracted and used in any C# project (RPGs, puzzle games, etc.)
- **Multi-Platform**: Single codebase deploys to WebGL, PC, Mobile, and Meta Quest 3 with adaptive UI
- **Production Architecture**: MVC pattern, dependency injection, event-driven systems, and assembly definitions
- **Portfolio Quality**: Comprehensive documentation, 85%+ test coverage, and CI/CD automation

---

## 🎮 Features

#### Current Implementation (EPIC-04 ✅ | EPIC-05 🔄)

#### Game Engines

- ✅ **Blackjack (21)**: Full rules with Ace mechanics and dealer AI (stand on soft 17).
- ✅ **Solitaire**: Klondike variant with spatial 3D drag/drop tableau, foundation rules, and auto-sweep victory detection.
- ✅ **Texas Hold'em**: Poker rules engine with betting round progression, pot tracking, and community card flow.

#### Architecture & Polish

- ✅ **Decoupled MVC & VContainer DI**: Pure C# Models (POCOs) injected cleanly across Controllers and Views.
- ✅ **Unity New Input System**: Unified spatial raycasting support across Mouse, Touch, and XR Interactors.
- ✅ **UI Toolkit Integration**: Dual-layer architecture with non-blocking `PickingMode.Ignore` 3D raycast pass-through.
- ✅ **Juice & Polish Engine**: Centralized `CardAudioService` (grab/drop/shuffle/click SFX), DOTween spatial card animation, and 3D particle burst victory effects.
- ✅ **Unit Testing**: NUnit EditMode test suite validating engine edge cases and wager loops.

#### Platform Support

- ✅ **WebGL**: Responsive browser-based gameplay with adaptive portrait/landscape USS layout.
- ✅ **PC Standalone**: High-framerate desktop build profile.
- 🔄 **Meta Quest 3**: Native spatial VR gameplay (In Progress - EPIC-05).

#### Backend & Cloud Persistence

- ✅ **PlayFab Cloud Infrastructure**: Silent hardware ID authentication, remote currency (`GD`) sync, debit/credit transaction safety, and PlayFab server-time verification.
- ✅ **Cross-Platform State Sync**: Asynchronous 6-character PIN account linking between WebGL and Meta Quest 3 sessions.

---

## 🏗️ Architecture

### Layer Separation

```
┌─────────────────────────────────────────┐
│        Presentation Layer (Views)       │ (MonoBehaviour)
│  CardView, PlayerStatusView, UIManager  │
└────────────────────┬────────────────────┘
                     │ UnityEvents
                     ▼
┌─────────────────────────────────────────┐
│      Controller Layer (Orchestration)   │ (MonoBehaviour)
│ BlackjackController, TableController    │
└────────────────────┬────────────────────┘
                     │ Direct access
                     ▼
┌─────────────────────────────────────────┐
│      Model Layer (Pure C#, NO Unity)    │ (POCO)
│ CardData, Deck, BlackjackEngine, etc.   │
└─────────────────────────────────────────┘
```

### Assembly Definitions (AsmDef)

- **Project.Core**: Pure C# game logic (zero dependencies)
- **Project.Presentation**: Views & Controllers (references Core)
- **Project.Input**: Input handling (references Core)
- **Project.Cloud**: Backend services (references Core)
- **Project.XR**: VR/Meta Quest support (optional, references Core & Presentation)
- **Project.Tests**: Unit & integration tests
- **Project.Editor**: Editor tools and utilities

### Key Design Patterns

| Pattern                    | Usage                   | Benefit                              |
| -------------------------- | ----------------------- | ------------------------------------ |
| **MVC**                    | Separation of concerns  | Testable, reusable                   |
| **Dependency Injection**   | Service registration    | No singletons, testable              |
| **Strategy**               | Game variant engines    | Swappable implementations            |
| **Observer (UnityEvents)** | Event propagation       | Loose coupling                       |
| **Adapter**                | Platform-specific input | Single interface, multiple platforms |
| **Finite State Machine**   | Game phase management   | Clear state transitions              |

---

## 🚀 Quick Start

### Requirements

- **Unity**: 6.3 LTS
- **Git**: Latest version
- **IDE**: Visual Studio Code
- **RAM**: 8GB minimum, 16GB recommended
- **Disk**: 50GB+ free space

### Setup (15 minutes)

```bash
# 1. Clone repository
git clone https://github.com/yourusername/multi-mode-card-framework.git
cd multi-mode-card-framework

# 2. Open in Unity (2022 LTS or later)
# File > Open Project > select folder

# 3. Wait for initial import and compilation (~2-3 minutes)

# 4. Verify setup
# Window > Test Runner > EditMode > Run All
# Expected: ~104 tests passing, 87% coverage

# 5. Open demo scene
# Scenes/Initialization.unity (then GameScene_Blackjack.unity)
```

### First Build (WebGL Example)

```bash
# File > Build Settings
# - Select WebGL platform
# - Click "Build"
# - Output directory: Builds/WebGL
# - Build time: ~2-3 minutes
```

---

## 📖 Documentation

| Document                 | Purpose                                                   |
| ------------------------ | --------------------------------------------------------- |
| **BACKLOG.md**           | Detailed task breakdown, dependencies, and tracking       |
| **PROJECT_STRUCTURE.md** | Folder hierarchy and assembly definition strategy         |
| **ARCHITECTURE.md**      | Architecture decisions and design rationale (coming soon) |

---

## 🧪 Testing

### Run Tests Locally

```bash
# EditMode tests (fast, no scenes)
Window > Test Runner > EditMode > Run All

# Or from command line:
~/Unity/Editor/Unity -projectPath . \
  -runTests -testPlatform editmode \
  -logFile test-results.log
```

### Test Coverage

- **Target**: 85%+
- **Current**: 87% (EPIC-01)
- **Tools**: NUnit + OpenCover

### Coverage Report

```bash
# Generate coverage report (CI/CD automation)
# Results available in: test-results/coverage.html
```

---

## 🔄 Development Workflow

### Branching Strategy

```
main (stable releases only)
  ↓
develop (integration branch)
  ↓
feature/EPIC-XX-description (individual work)
  ↓
feature/TASK-X.X-description (granular tasks)
```

### Commit Convention

```bash
# Format:
git commit -m "TASK-X.X: Brief description

Details:
- What was implemented
- Why this approach was chosen
- Any blockers or considerations"

# Examples:
git commit -m "TASK-1.1: Implement CardData and Deck with Fisher-Yates shuffle"
git commit -m "TASK-2.1: Setup LootLocker integration with guest authentication"
```

### Creating a PR

1. Create feature branch: `feature/TASK-X.X-description`
2. Commit frequently with clear messages
3. Push to GitHub: `git push origin feature/TASK-X.X-description`
4. Create Pull Request
5. Verify CI/CD tests pass (GitHub Actions)
6. Request code review
7. Merge to `develop` after approval

---

## 📊 Project Status

### EPIC Progress

| Epic | Tasks | Status | Target Completion |
| :--- | :---: | :---: | :---: |
| **EPIC-01: Core Pure C# Game Logic** | 4/4 | ✅ COMPLETED | Week 4 |
| **EPIC-02: Architecture & DI Scoping** | 3/3 | ✅ COMPLETED | Week 8 |
| **EPIC-03: UI/UX Presentation Layer** | 3/3 | ✅ COMPLETED | Week 12 |
| **EPIC-04: Cloud Infrastructure & Metagame** | 5/5 | ✅ COMPLETED | Week 16 |
| **EPIC-05: XR Integration & Meta Quest 3** | 0/4 | 🔄 IN PROGRESS | Week 20 |

### Granular Phase Updates (EPIC-04 ✅)

- **TASK-4.1 & 4.2:** PlayFab SDK integration with silent guest login, `IEconomyService` wallet data binding to UI Toolkit HUD, and transaction safety validation.
- **TASK-4.3:** Asynchronous 6-character PIN cross-platform account linking for WebGL and Meta Quest 3.
- **TASK-4.5:** 3D physical spatial card interaction handling for Mouse, Touch, and XR Interactors with zero UI blockages.
- **TASK-4.5.1:** Centralized tactile audio feedback (`CardAudioService`), spatial DOTween motion, and victory particle burst effects.

### Timeline

- **Current Phase**: Starting EPIC-05 (Meta Quest 3 & Deployment Pipelines) | 🔄

## 🚀 AI-Accelerated Velocity & Sprint Metrics

This project demonstrates hyper-efficient development velocity by integrating advanced AI-assisted engineering workflows (GitHub Copilot, Cursor, Ollama, and LLM orchestration) to compress traditional enterprise timelines.

### Sprint 1 Summary (Duration: 2 Weeks)

- **Traditional Baseline Estimation**: 12 Weeks (To research, scaffold core architectures, and implement fully responsive multi-platform UI layouts from scratch).
- **Actual AI-Augmented Timeline**: 2 Weeks (Completed Sprint 1 today).
- **Velocity Multiplier**: ~6x development speed optimization.

### Timeline Compression Breakdown

| Phase / Milestone                    | Estimated (Manual) | Actual (AI-Driven) |   Status   | Efficiency Gain |
| :----------------------------------- | :----------------: | :----------------: | :--------: | :-------------: |
| **EPIC-01: Pure C# Domain Engine**   |    Weeks 1 - 4     |     Days 1 - 4     | Decoupled  | 85% Time Reduc. |
| **EPIC-02: Architecture & DI Layer** |    Weeks 5 - 8     |     Days 5 - 8     | Solidified | 80% Time Reduc. |
| **EPIC-03: Responsive UI Toolkit**   |    Weeks 9 - 12    |    Days 9 - 14     | Production | 88% Time Reduc. |
| **EPIC-04: Cloud, Polish & Input**   |   Weeks 13 - 16    |    Days 15 - 20    | Production | 86% Time Reduc. |
> 💡 **Architectural Note**: This acceleration wasn't achieved by cutting corners. The code maintains a strict Decoupled MVC design pattern, uses VContainer for dependency injection, features adaptive spatial calculation algorithms for portrait mobile viewports, and maintains a stable **90% min Unit Test coverage**.

---

## 🌍 Backend Services

### PlayFab Cloud Infrastructure (Primary System)

- **Free tier**: Up to 100,000 user accounts
- **API**: REST endpoints (similar to LootLocker)
- **Docs**: https://docs.microsoft.com/gaming/playfab/

**Status**: Free Tier Ecosystem (Sandbox Mode)  
**Authentication Vector**: Silent Device ID authentication for friction-free modern player profile creation.  
**Features Used**:

- Guest Device Lifecycle Management
- Cloud Save JSON State Synchronization
- Cross-progression Data Synchronization

**Configuration Workflow**:

```bash
# Securely loaded inside CI/CD runner secrets—NEVER commit raw data variants to repo
PLAYFAB_TITLE_ID=your_playfab_title_id
```

### LootLocker (Alternative)

**Status**: Free tier (no monetization)  
**License**: Applied for enterprise free tier (waiting approval)  
**Features Used**:

- Guest authentication
- Cloud save persistence
- Player profiles
- Cross-progression (PIN linking)

**Configuration**:

```bash
# Store in CI/CD secrets, NOT in repository
LOOTLOCKER_GAME_ID=your_game_id
LOOTLOCKER_API_KEY=your_api_key
```

**Migration Path**:

1. Implement `ICloudService` interface abstraction
2. Create `PlayFabCloudService` implementation
3. Update DI container bindings
4. Update configuration

---

## 🛠️ Development Tools

### Required

- **Unity 6.3 LTS+**: Game engine
- **Visual Studio Code** or **Rider**: C# IDE with debugger
- **Git**: Version control
- **GitHub CLI**: Optional, for PR management

### Optional (Recommended)

- **Git Desktop**: GUI for Git
- **Unity Profiler**: Built-in performance analysis
- **Visual Studio**: Advanced C# debugging
- **Meta XR All-in-One SDK**: For Quest 3 development

### Setup VS Code Extensions

```
Extensions to install:
- C# (powered by OmniSharp)
- Debugger for Unity
- GitLens
- Thunder Client (REST testing)
```

---

## 🚢 Deployment

### Platform Targets

| Platform | Status | Build Time | Output Size |
| :--- | :--- | :--- | :--- |
| WebGL | ✅ Ready | ~2-3 min | <50MB |
| Windows PC | ✅ Ready | ~3-5 min | ~150MB |
| macOS | ✅ Ready | ~5-7 min | ~150MB |
| Mobile (iOS / Android) | ✅ Ready | ~8-10 min | ~200MB |
| Meta Quest 3 | 🔄 EPIC-05 | ~5-7 min | ~250MB |

### Build Commands (CLI)

```bash
# WebGL
~/Unity/Editor/Unity -projectPath . -buildTarget WebGL \
  -executeMethod BuildScript.BuildWebGL

# Windows
~/Unity/Editor/Unity -projectPath . -buildTarget StandaloneWindows64 \
  -executeMethod BuildScript.BuildWindows

# Android
~/Unity/Editor/Unity -projectPath . -buildTarget Android \
  -executeMethod BuildScript.BuildAndroid
```

---

## 🤝 Contributing

This is a **solo development portfolio project**.

**For feedback or suggestions**:

- Create GitHub Issue
- Include: description, reproduction steps, expected behavior

---

## 📚 References & Resources

### Documentation

- [BACKLOG.md](BACKLOG.md) - Detailed task tracking
- [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) - Folder organization

### External Resources

- [Unity Documentation](https://docs.unity3d.com/)
- [LootLocker Docs](https://docs.lootlocker.io/)
- [PlayFab Docs](https://docs.microsoft.com/gaming/playfab/)
- [Meta XR SDK](https://developers.meta.com/horizon/documentation/)
- [C# Best Practices](https://docs.microsoft.com/dotnet/csharp/)

### Useful Tools

- [GitHub Actions](https://github.com/features/actions) - CI/CD automation
- [Game-CI](https://game.ci/) - Unity CI/CD workflows
- [ShaderGraph](https://unity.com/features/shader-graph) - Visual shader creation

---

## 🎓 Learning Resources

### Architecture & Design Patterns

- MVC Pattern: https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller
- Dependency Injection: https://en.wikipedia.org/wiki/Dependency_injection
- Unity Best Practices: https://learn.unity.com/

### Game Development

- Brackeys Game Development: https://www.youtube.com/c/Brackeys
- Game Programming Patterns: https://gameprogrammingpatterns.com/

---

## 🐛 Troubleshooting

### Build Issues

**Problem**: "Assembly not found"

- **Solution**: Verify .asmdef files exist and references are correct
- **Command**: `Assets > Reimport All`

**Problem**: "Input System not responding"

- **Solution**: Verify `GameplayActions.inputactions` asset exists
- **Path**: `Assets/_Project/Input/GameplayActions.inputactions`

### Performance Issues

**Problem**: "Game dropping below 60fps"

- **Solution**: Open `Window > Analysis > Profiler` and identify bottleneck
- **Common Fixes**:
  - Reduce draw calls (batch static geometry)
  - Lower shadow distance (20-30 meters)
  - Disable MSAA on mobile

---

## 📞 Support

### Getting Help

1. **GitHub Issues**: Create an issue if you find a bug or have a feature request
2. **Search Existing Issues**: Your question may already be answered

### Reporting Bugs

Please include:

- Description of the issue
- Reproduction steps
- Expected vs actual behavior
- Environment (Unity version, platform, etc.)

---

## 🎉 Acknowledgments

This project demonstrates professional game development practices through:

- **Clean Architecture**: Decoupled MVC pattern
- **Quality Assurance**: 85%+ test coverage with NUnit
- **Documentation**: Comprehensive guides and code examples
- **Cross-Platform Support**: WebGL, PC, Mobile, and VR
- **CI/CD Integration**: Automated testing and builds

Built as a **senior-level portfolio project** showcasing production-ready code quality and architectural best practices.

---

## 📊 Project Metrics

- **Code Coverage**: 90% (target: 85%+)
- **Documentation**: (BACKLOG, STRUCTURE)
- **Assembly Definitions**: 7 (clean separation)
- **CI/CD Pipeline**: GitHub Actions (automated)
- **Test Suite**: 102 unit tests (EditMode)
- **Target Platforms**: (WebGL, PC, Mobil, Quest 3)

---

**Last Updated**: August 2026  
**Maintenance Status**: Active Development  
**Project Type**: Solo Portfolio | Production-Grade Game Engine

---

### Quick Links

- 🌐 **Play Online**: (Coming soon - WebGL build)
- 📦 **Releases**: [GitHub Releases](https://github.com/yourusername/multi-mode-card-framework/releases)
- 📋 **Issues**: [GitHub Issues](https://github.com/yourusername/multi-mode-card-framework/issues)
- 📖 **Wiki**: [GitHub Wiki](https://github.com/yourusername/multi-mode-card-framework/wiki)
