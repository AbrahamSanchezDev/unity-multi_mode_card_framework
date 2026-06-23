using VContainer;
using VContainer.Unity;
using CardFramework.Core.Engines;

namespace CardFramework.Architecture.DI
{
    /// <summary>
    /// Root Dependency Injection container for the Card Framework application using VContainer.
    /// Registers core engines and architectural services.
    /// </summary>
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 1. Register Core Game Engines as Transients.
            // This ensures every time a new game scene or table is loaded, 
            // a fresh, isolated logic instance is provided without cross-contamination.
            builder.Register<BlackjackEngine>(Lifetime.Transient);
            builder.Register<TexasHoldemEngine>(Lifetime.Transient);
            
            // Note: If SolitaireEngine is implemented as a non-static engine, 
            // register it here matching the same lifecycle.
            builder.Register<SolitaireEngine>(Lifetime.Transient);

            // 2. [Extension Point] Future Cloud & Network Services will be registered here.
            // e.g., builder.Register<IAuthenticationService, CloudAuthService>(Lifetime.Singleton);

            // Cloud Infrastructure Contracts (Singletons)
            // Once concrete classes like FirebaseAuthService are built, register them here:
            // builder.Register<IAuthenticationService, FirebaseAuthService>(Lifetime.Singleton);
            // builder.Register<ICloudSaveService, FirebaseSaveService>(Lifetime.Singleton);
        }
    }
}