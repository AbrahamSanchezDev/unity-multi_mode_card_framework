using UnityEngine;
using CardFramework.Core.Interfaces;
using VContainer;
using VContainer.Unity;

namespace CardFramework.Presentation.Controllers {
    // IInitializable tells VContainer to invoke the 'Initialize' method automatically once injection finishes
    public class CloudInitializationController : IInitializable {
        private readonly ICloudService _cloudService;

        [Inject]
        public CloudInitializationController(ICloudService cloudService) {
            _cloudService = cloudService;
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

            // Here you can unlock your UI buttons, or trigger the next steps (e.g., fetch economy/currency balances)
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