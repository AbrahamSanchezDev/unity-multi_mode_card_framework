using UnityEngine;
using CardFramework.Core.Interfaces;
using VContainer;
using VContainer.Unity;

namespace CardFramework.Presentation.Controllers {
    // IInitializable tells VContainer to invoke the 'Initialize' method automatically once injection finishes
    public class CloudInitializationController : IInitializable {
        private readonly ICloudService _cloudService;
        private readonly IEconomyService _economyService;

        [Inject]
        public CloudInitializationController(ICloudService cloudService, IEconomyService economyService) {
            _cloudService = cloudService;
            _economyService = economyService;
        }

        public void Initialize() {
            Debug.Log("[Boot] Order Received. Hooking cloud connectivity dependencies...");

            // Register to authentication loops
            _cloudService.OnAuthenticationSuccess += OnCloudReady;
            _cloudService.OnAuthenticationFailed += OnCloudBootError;

            // Trigger silent cloud login process immediately on start
            _cloudService.AuthenticateSilently();
        }

        private void OnCloudReady() {
            Debug.Log($"[Boot] Cloud connection established. Player authenticated as ID: {_cloudService.PlayerId}");

            // Once authenticated, immediately fetch the player's currency balance and recharge ticks
            Debug.Log("[Boot] Synchronizing server economy balances...");
            _economyService.RefreshBalance();

            _cloudService.OnAuthenticationSuccess -= OnCloudReady;
            _cloudService.OnAuthenticationFailed -= OnCloudBootError;
        }

        private void OnCloudBootError(string reason) {
            Debug.LogError($"[Boot] Critical Cloud Connection Failure: {reason}");

            _cloudService.OnAuthenticationSuccess -= OnCloudReady;
            _cloudService.OnAuthenticationFailed -= OnCloudBootError;
        }
    }
}