# GitHub Copilot Instructions: Unity Production Standards

You are an expert Unity and C# software engineer. All code generated, refactored, or reviewed for this project must strictly adhere to the following architectural pillars, technical patterns, and conventions.

## Project Context & Philosophy

- Follow clean, production-ready patterns optimized for testability, modularity, and easy extension.
- Prefer explicit, typed code over dynamic or overly abstract patterns. Keep classes focused on a single responsibility.
- Avoid speculative abstractions or premature framework setup that are not needed for the current feature.

## Repository Structure & Organization

Organize code and assets consistently under your project's root folder:

- **Scripts/:**
  - `Architecture/`: Event channels, bootstrappers, service locators, and interfaces.
  - `Controllers/`: Gameplay, interaction, physics, and AI logic.
  - `Models/`: Pure C# state and data containers.
  - `Views/`: UI Toolkit or UI-related controllers and presenters.
- **Data/ or Settings/:** ScriptableObject data containers (`.asset`) used for static definitions and shared configurations.
- **Prefabs/:** Reusable GameObject templates.
- **UI/:** UI Document templates (`.uxml`) and style sheets (`.uss`).
- **Tests/:** Automated test scripts.

## Architectural Rules (MVC & Data-Driven)

1. **Model-View-Controller (MVC) Separation:**
   - **Models:** Pure C# data classes or `ScriptableObject` assets holding state variables and emitting C# events upon modification. **Must hold zero references** to Unity `GameObject`, `Transform`, or UI elements.
   - **Views:** Passive UI Toolkit visual document hierarchies (`.uxml` + `.uss`). **Must not contain** business logic, state calculations, or direct scene queries.
   - **Controllers / Intermediaries:** C# components that listen to events emitted by Models or Event Channels to update passive Views or control physics/world entities.

2. **Data-Driven Development & Shared Asset Configurations:**
   - Store static definitions, matrices, and lookup tables inside `ScriptableObject` data containers under a designated `Data/` or `Settings/` folder.
   - **Shared Asset Configuration Rule:** When multiple instances of a component require shared external references (such as Audio Clips, VFX prefabs, material profiles, or global settings), **do not** assign them individually per component. Instead, group them into a shared `ScriptableObject` configuration asset that components reference centrally. This allows global updates by modifying just one asset.
   - Keep dynamic runtime states inside dedicated **Model classes** rather than scattering them across execution `MonoBehaviour` components.
   
3. **ScriptableObject Event Channels (Observer Pattern):**
   - Cross-system communication must pass through ScriptableObject Event Channels to eliminate tight coupling between independent systems.
   - Prefer interfaces under your `Scripts/Architecture/Interfaces` directory for core systems where decoupling or mocking is useful.

## Unity-Specific Coding Rules

- **Modern Input System:** Use Unity's **New Input System** package exclusively for all player interactions. Legacy calls (`Input.GetKeyDown`, `Input.GetAxis`) are **strictly prohibited**. Controllers must bind exclusively to serialized `InputActionReference` variables.
- **Production-Ready UI Toolkit:** Inline styles inside `.uxml` files are **strictly forbidden**. All visual layouts must be styled using external `.uss` style sheets. Web-only CSS properties (e.g., `border`, `gap`, `box-shadow`) are forbidden; use Unity-native USS properties (e.g., `border-width`, `border-color`, `margins`, `picking-mode`).

## Testing Expectations

- Write testable code by depending on abstractions or events instead of concrete scene objects.
- Keep test code cleanly separated under your project's designated test directory and prefer simple, deterministic unit tests.

## Prohibitions (Do Not)

- Do not place business logic inside UI Toolkit views.
- Do not store runtime state directly in `MonoBehaviour` fields when a model class is more appropriate.
- Do not use legacy input APIs or inline UX styles under any circumstances.
- Do not introduce new dependencies or tight coupling unless clearly justified by project needs.
