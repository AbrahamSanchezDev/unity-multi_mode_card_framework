# Multi-Mode Card Framework

A Unity 6.3 LTS card-game framework built to demonstrate clean architecture, modular gameplay systems, and cross-platform delivery across WebGL and Meta Quest 3.

[![Unity Version](https://img.shields.io/badge/unity-6.3%20LTS-blue)](https://unity.com/download)

## ⭐ Why this project matters

This project was designed to show how game systems can be structured for scalability, maintainability, and real production-style delivery. It centers on a pure C# gameplay domain, explicit architectural boundaries, dependency injection, platform adapters, and testable service interfaces.

The goal is to demonstrate a reusable game framework that keeps rules, presentation, and platform-specific behavior cleanly separated while still shipping on multiple surfaces.

## 🧰 Tech Stack

- **Unity 6.3 LTS** for engine and cross-platform delivery
- **C#** for gameplay logic, service layers, and architecture
- **UI Toolkit** for responsive presentation and UI flow
- **VContainer** for dependency injection and runtime composition
- **PlayFab** for authentication, cloud data, economy, and time validation
- **XR Interaction Toolkit** for Meta Quest 3 interaction and spatial input
- **NUnit** for deterministic testing and validation

---

## 🎮 Play the MVP

- **WebGL Demo:** [Open in browser](https://abrahamsanchezdev.github.io/unity-multi_mode_card_framework/)
- **Meta Quest 3 VR Build:** [Download release](https://github.com/AbrahamSanchezDev/unity-multi_mode_card_framework/releases/tag/VR_v1)

---

## 👀 Project Preview

![Epic 3 Preview Image](/ReadMePreviewImages/Epic_3.png)

![Epic 3 Preview](/ReadMePreviewImages/Epic_3.gif)

![Epic 4 Preview](/ReadMePreviewImages/Epic_4.gif)

<video src="/ReadMePreviewImages/Epic_4.mp4" width="800" height="450" controls>
  Your browser does not support the video tag.
</video>

<div align="center">
  <a href="https://youtube.com/shorts/9fTiyTmDf-M">
    <img src="https://img.youtube.com/vi/9fTiyTmDf-M/hqdefault.jpg" alt="VR testing video" width="350" style="border-radius: 10px;" />
  </a>
</div>

---

## 🎯 Project Overview

This project is a modular card-game framework built around a strict architecture boundary: gameplay rules live in pure C# logic, while Unity-specific presentation, input, and platform integrations remain separated behind interfaces and composition roots.

The result is a codebase that is easier to test, easier to extend, and more representative of how production-ready systems are structured in real game projects.

### What this demonstrates

- **Pure C# gameplay logic** with no Unity dependency in the domain layer
- **Layered architecture** with clearly separated Core, Presentation, Cloud, Input, and XR responsibilities
- **Dependency injection** using VContainer instead of scene-based singleton access
- **Cross-platform delivery** for browser-based WebGL and Meta Quest 3 experiences
- **Production-minded patterns** for testability, modularity, and long-term maintainability

---

## 🃏 Games and Systems Implemented

### Current game modes

- ✅ **Blackjack** with standard hand evaluation and dealer logic
- ✅ **Solitaire** with tableau, foundation, and drag-based interaction flows
- ✅ **Texas Hold'em** with betting and community-card progression logic

### Core systems

- ✅ **Decoupled presentation flow** between controllers and views
- ✅ **VContainer-based dependency composition** through a central lifetime scope
- ✅ **UI Toolkit presentation** with responsive layouts and platform-adaptive behavior
- ✅ **Spatial interaction for XR** with card grabbing, haptics, and interaction feedback
- ✅ **PlayFab integration** for authentication, cloud data, economy, and server time validation

---

## 🏗️ Architecture

The project follows a layered structure designed to keep game rules reusable and platform code isolated.

```text
+------------------------------------------------------+
| Presentation                                          |
| UI Toolkit, controllers, table interaction, views    |
+-------------------------------+----------------------+
                                | injected contracts
+-------------------------------v----------------------+
| Platform and infrastructure adapters                  |
| Cloud / PlayFab, Input, XR, Unity-facing services     |
+-------------------------------+----------------------+
                                | interfaces
+-------------------------------v----------------------+
| Core                                                  |
| CardData, Deck, game engines, rules, models           |
| Pure C#; no UnityEngine or MonoBehaviour             |
+------------------------------------------------------+
```

### Architectural focus

- **Core** contains the rules, models, and deterministic game logic.
- **Presentation** handles UI, scene behavior, and interaction orchestration.
- **Cloud** contains PlayFab-backed services and their adapter boundaries.
- **Input and XR** isolate platform-specific behavior rather than leaking it into the domain.

This pattern keeps the rule layer portable while allowing the project to ship in multiple environments with different interaction models.

---

## 🧭 Project Documentation

The repository includes deeper technical documentation for architecture, roadmap, and project structure:

- [Assets/Project/Documentation/PROJECT_ARCHITECTURE.md](Assets/Project/Documentation/PROJECT_ARCHITECTURE.md) — system design and dependency boundaries
- [Assets/Project/Documentation/PROJECT_ROADMAP.md](Assets/Project/Documentation/PROJECT_ROADMAP.md) — milestone history and delivery status
- [Assets/Project/Documentation/PROJECT_STRUCTURE.md](Assets/Project/Documentation/PROJECT_STRUCTURE.md) — folder and assembly layout
- [Assets/Project/Documentation/BACKLOG.md](Assets/Project/Documentation/BACKLOG.md) — future work and backlog priorities

---

## 📊 Current Status

**MVP complete** and available across both delivery channels:

- Browser demo: live and playable
- Meta Quest 3 build: published through GitHub Releases

The codebase is now positioned for continued expansion in polish, quality, and gameplay depth rather than foundational architecture work.

---

## 🧪 Testing and Engineering Practices

The project is organized around deterministic testing and replaceable infrastructure boundaries.

- **EditMode tests** validate domain logic and service behavior
- **PlayMode coverage** supports scene and integration validation
- **Infrastructure services** are wrapped behind interfaces to avoid hard dependency coupling
- **Runtime dependencies** are registered through a central dependency injection composition root

---

## 🤝 Notes

This is a solo portfolio project focused on demonstrating practical engineering judgment, system design, and delivery across multiple platforms.

For project details, roadmap updates, and future backlog items, see the documentation links above.

---

## 🔗 Quick Links

- 🌐 **Play online**: [WebGL Demo](https://abrahamsanchezdev.github.io/unity-multi_mode_card_framework/)
- 🥽 **VR release**: [Meta Quest 3 build](https://github.com/AbrahamSanchezDev/unity-multi_mode_card_framework/releases/tag/VR_v1)
- 📦 **GitHub releases**: [Releases](https://github.com/AbrahamSanchezDev/unity-multi_mode_card_framework/releases)
- 🐛 **Issues**: [GitHub Issues](https://github.com/AbrahamSanchezDev/unity-multi_mode_card_framework/issues)

---

Built as a portfolio project centered on clean architecture, scalable systems thinking, and production-minded Unity development.
