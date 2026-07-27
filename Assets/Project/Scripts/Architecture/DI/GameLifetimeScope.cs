using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using CardFramework.Core.Models;
using CardFramework.Core.Engines;
using CardFramework.Cloud.Interfaces;
using CardFramework.Cloud.PlayFab;
using CardFramework.Cloud;
using CardFramework.Core.Interfaces;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Controllers;
using CardFramework.Presentation.Views;
using CardFramework.Presentation.Input;
using CardFramework.Core.Managers;

namespace CardFramework.Architecture.DI {
    /// <summary>
    /// Root Dependency Injection container for the Card Framework application using VContainer.
    /// Registers core engines and architectural services.
    /// </summary>
    public class GameLifetimeScope : LifetimeScope {
        [Header("UI Presentation Hierarchy References")]
        [SerializeField] private BlackjackView blackjackViewInstance;
        [SerializeField] private SolitaireView solitaireView;

        [Header("Global Infrastructure UI References")]
        [SerializeField] private ModalServiceView modalServiceViewInstance;
        [SerializeField] private BettingModalView bettingModalView;

        [SerializeField] private InputActionReference menuActionReference;
        [SerializeField] private DashboardMenuView dashboardMenuView;

        [SerializeField] private GameTableManager tableManager;
        [SerializeField] private NotificationSidebarView notificationSidebarViewInstance;

        protected override void Configure(IContainerBuilder builder) {

            // ---- CORE DATA MODELS & ENGINES ----
            // Decks & Game Engines registered as Transient/Scoped for isolated state
            builder.Register<Deck>(Lifetime.Transient);
            builder.Register<BlackjackEngine>(Lifetime.Transient);
            builder.Register<TexasHoldemEngine>(Lifetime.Transient);
            builder.Register<SolitaireEngine>(Lifetime.Scoped);

            // ---- CLOUD & INFRASTRUCTURE SERVICES (SINGLETONS) ----
            builder.Register<ICloudService, PlayFabCloudService>(Lifetime.Singleton);
            builder.Register<IAuthenticationService, PlayFabAuthService>(Lifetime.Singleton);
            builder.Register<ICloudSaveService, PlayFabDataService>(Lifetime.Singleton);
            builder.Register<IEconomyService, PlayFabEconomyService>(Lifetime.Singleton);
            builder.Register<IInputContext, StandaloneInputAdapter>(Lifetime.Singleton);
            builder.Register<ITimeService, PlayFabTimeService>(Lifetime.Singleton);
            builder.Register<CloudMailboxManager>(Lifetime.Singleton);

            // ---- VIEWS / PRESENTATION LAYER REGISTRATIONS ----
            if (blackjackViewInstance != null) {
                builder.RegisterComponent(blackjackViewInstance).As<IBlackjackView>();
            }

            if (modalServiceViewInstance != null) {
                builder.RegisterComponent(modalServiceViewInstance).As<IModalService>();
            }

            if (solitaireView != null) {
                builder.RegisterComponent(solitaireView).As<ISolitaireView>();
            }

            if (dashboardMenuView != null) {
                builder.RegisterComponent(dashboardMenuView);
            }

            if (notificationSidebarViewInstance != null) {
                builder.RegisterComponent(notificationSidebarViewInstance);
            }

            if (bettingModalView != null) {
                builder.RegisterComponent(bettingModalView);
            }

            // ---- ENTRY POINT CONTROLLERS ----
            builder.RegisterEntryPoint<CloudInitializationController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<BlackjackTableController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<SolitaireTableController>(Lifetime.Scoped);

            // Navigation Controller registered as EntryPoint for ITickable loops
            builder.RegisterEntryPoint<NavigationController>()
                .WithParameter(menuActionReference)
                .WithParameter(tableManager)
                .AsSelf();
        }
    }
}