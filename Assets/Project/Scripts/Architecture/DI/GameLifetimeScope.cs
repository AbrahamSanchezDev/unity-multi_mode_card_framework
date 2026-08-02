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
using CardFramework.Presentation;

namespace CardFramework.Architecture.DI {
    /// <summary>
    /// Root Dependency Injection container for the Card Framework application using VContainer.
    /// Registers core engines and architectural services.
    /// </summary>
    public class GameLifetimeScope : LifetimeScope {
        [Header("UI Presentation Hierarchy References")]
        [SerializeField] private BlackjackView blackjackViewInstance;
        [SerializeField] private SolitaireView solitaireView;
        [SerializeField] private TexasHoldemView texasHoldemViewInstance;

        [Header("Global Infrastructure UI References")]
        [SerializeField] private ModalServiceView modalServiceViewInstance;
        [SerializeField] private BettingModalView bettingModalView;

        [SerializeField] private InputActionReference menuActionReference;
        [SerializeField] private CardAudioService audioServiceInstance;
        [SerializeField] private DashboardMenuView dashboardMenuView;
        [SerializeField] private GameRoomIntroView gameRoomIntroView;

        [SerializeField] private GameTableManager tableManager;
        [SerializeField] private NotificationSidebarView notificationSidebarViewInstance;

        protected override void Configure(IContainerBuilder builder) {
            // Check if we already have the texture version stored in PlayerPrefs
            CardFaceGenerator.CheckTexturesVersion();

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

            // ---- AUDIO SERVICE ----
            if (audioServiceInstance != null) {
                builder.RegisterComponent(audioServiceInstance).AsSelf().As<IAudioService>();
            }
            else {
                var audioServiceObject = new GameObject("CardAudioService");
                audioServiceObject.transform.SetParent(transform, false);
                var audioServiceInstance = audioServiceObject.AddComponent<CardAudioService>();
                if (audioServiceInstance != null) {
                    builder.RegisterComponent(audioServiceInstance).AsSelf().As<IAudioService>();
                }
            }

            // ---- VIEWS / PRESENTATION LAYER REGISTRATIONS ----
            if (blackjackViewInstance != null) {
                builder.RegisterComponent(blackjackViewInstance).AsSelf().As<IBlackjackView>();
            }
            else {
                var blackjackViewInScene = Object.FindFirstObjectByType<BlackjackView>();
                if (blackjackViewInScene != null) {
                    builder.RegisterComponent(blackjackViewInScene).AsSelf().As<IBlackjackView>();
                }
            }

            if (modalServiceViewInstance != null) {
                builder.RegisterComponent(modalServiceViewInstance).As<IModalService>();
            }

            if (solitaireView != null) {
                builder.RegisterComponent(solitaireView).AsSelf().As<ISolitaireView>();
            }
            else {
                var solitaireViewInScene = Object.FindFirstObjectByType<SolitaireView>();
                if (solitaireViewInScene != null) {
                    builder.RegisterComponent(solitaireViewInScene).AsSelf().As<ISolitaireView>();
                }
            }

            if (texasHoldemViewInstance != null) {
                builder.RegisterComponent(texasHoldemViewInstance).AsSelf().As<ITexasHoldemView>();
            }
            else {
                var texasHoldemViewInScene = Object.FindFirstObjectByType<TexasHoldemView>();
                if (texasHoldemViewInScene != null) {
                    builder.RegisterComponent(texasHoldemViewInScene).AsSelf().As<ITexasHoldemView>();
                }
            }

            if (dashboardMenuView != null) {
                builder.RegisterComponent(dashboardMenuView).AsSelf();
            }
            else {
                var dashboardViewInScene = Object.FindFirstObjectByType<DashboardMenuView>();
                if (dashboardViewInScene != null) {
                    builder.RegisterComponent(dashboardViewInScene).AsSelf();
                }
            }

            if (gameRoomIntroView != null) {
                builder.RegisterComponent(gameRoomIntroView).AsSelf();
            }
            else {
                var introViewInScene = Object.FindFirstObjectByType<GameRoomIntroView>();
                if (introViewInScene != null) {
                    builder.RegisterComponent(introViewInScene).AsSelf();
                }
                else {
                    var introGameObject = new GameObject("GameRoomIntroView");
                    introGameObject.transform.SetParent(transform, false);
                    var introViewInstance = introGameObject.AddComponent<GameRoomIntroView>();
                    if (introViewInstance != null) {
                        introViewInstance.SetData(ScriptableObject.CreateInstance<CardGamesRoomIntroData>());
                        builder.RegisterComponent(introViewInstance).AsSelf();
                    }
                }
            }

            if (notificationSidebarViewInstance != null) {
                builder.RegisterComponent(notificationSidebarViewInstance);
            }

            if (bettingModalView != null) {
                builder.RegisterComponent(bettingModalView);
            }

            if (tableManager != null) {
                builder.RegisterComponent(tableManager).AsSelf();
            }
            else {
                var tableManagerInScene = Object.FindFirstObjectByType<GameTableManager>();
                if (tableManagerInScene != null) {
                    builder.RegisterComponent(tableManagerInScene).AsSelf();
                }
                else {
                    var tableManagerObject = new GameObject("GameTableManager");
                    tableManagerObject.transform.SetParent(transform, false);
                    var tableManagerInstance = tableManagerObject.AddComponent<GameTableManager>();
                    if (tableManagerInstance != null) {
                        builder.RegisterComponent(tableManagerInstance).AsSelf();
                    }
                }
            }

            // ---- ENTRY POINT CONTROLLERS ----
            builder.RegisterEntryPoint<CloudInitializationController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<BlackjackTableController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<SolitaireTableController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<TexasHoldemTableController>(Lifetime.Scoped);
            builder.RegisterEntryPoint<CardGamesRoomIntroController>(Lifetime.Scoped);

            // Navigation Controller registered as EntryPoint for ITickable loops
            builder.RegisterEntryPoint<NavigationController>()
                .WithParameter(menuActionReference)
                .WithParameter(tableManager)
                .AsSelf();
        }
    }
}