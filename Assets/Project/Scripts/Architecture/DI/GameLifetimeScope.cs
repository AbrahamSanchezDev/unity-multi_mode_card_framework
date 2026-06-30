using UnityEngine;
using VContainer;
using VContainer.Unity;
using CardFramework.Core.Models;
using CardFramework.Core.Engines;
using CardFramework.Cloud.Interfaces;
using CardFramework.Cloud.PlayFab;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Controllers;
using CardFramework.Presentation.Views;

namespace CardFramework.Architecture.DI {
    /// <summary>
    /// Root Dependency Injection container for the Card Framework application using VContainer.
    /// Registers core engines and architectural services.
    /// </summary>
    public class GameLifetimeScope : LifetimeScope {
        [Header("UI Presentation Hierarchy References")]
        [SerializeField] private BlackjackView blackjackViewInstance;
        protected override void Configure(IContainerBuilder builder) {

            // Core Data Models & Decks (Transient so each engine gets a unique stack)
            builder.Register<Deck>(Lifetime.Transient);
            
            // Register Core Game Engines as Transients.
            // This ensures every time a new game scene or table is loaded, 
            // a fresh, isolated logic instance is provided without cross-contamination.
            builder.Register<BlackjackEngine>(Lifetime.Transient);
            builder.Register<TexasHoldemEngine>(Lifetime.Transient);

            // Note: If SolitaireEngine is implemented as a non-static engine, 
            // register it here matching the same lifecycle.
            builder.Register<SolitaireEngine>(Lifetime.Transient);

            // [Extension Point] Future Cloud & Network Services will be registered here.
            // e.g., builder.Register<IAuthenticationService, CloudAuthService>(Lifetime.Singleton);

            // Cloud Infrastructure Contracts (Singletons)
            // Binding contracts directly to PlayFab concrete services seamlessly
            builder.Register<IAuthenticationService, PlayFabAuthService>(Lifetime.Singleton);
            builder.Register<ICloudSaveService, PlayFabDataService>(Lifetime.Singleton);

            // ---- PRESENTATION LAYER REGISTRATIONS (TASK-3.3) ----

            // Registering the view instance present inside the active Unity Scene Hierarchy
            builder.RegisterInstance<IBlackjackView>(blackjackViewInstance);

            // Registering the POCO entry point controller to bind into the Unity Engine lifecycle automatically
            builder.RegisterEntryPoint<BlackjackTableController>(Lifetime.Singleton);
        }
    }
}