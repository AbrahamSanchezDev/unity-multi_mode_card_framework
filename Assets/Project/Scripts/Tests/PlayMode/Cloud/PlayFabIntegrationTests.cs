using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using CardFramework.Cloud.Interfaces;
using CardFramework.Cloud.PlayFab;
using PlayFab;

namespace CardFramework.Tests.PlayMode.Cloud {
    [TestFixture]
    public class PlayFabIntegrationTests {
        private IAuthenticationService _authService;
        private ICloudSaveService _saveService;

        [SetUp]
        public void Setup() {
            // Instantiating concrete implementations for end-to-end integration testing
            _authService = new PlayFabAuthService();
            _saveService = new PlayFabDataService();
        }

        [TearDown]
        public void TearDown() {
            if (_authService != null && _authService.IsLoggedIn) {
                _authService.LogoutAsync().GetAwaiter().GetResult();
            }
        }

        [UnityTest]
        public IEnumerator PlayFab_AnonymousLogin_ReturnsSuccessAndSetsPlayerId() {
            // 1. Trigger the asynchronous network task
            Task<bool> loginTask = _authService.LoginAnonymousAsync();

            // 2. Yield control back to Unity until the server responds
            while (!loginTask.IsCompleted) {
                yield return null;
            }

            // 3. Evaluation criteria
            Assert.IsTrue(loginTask.Result, "PlayFab authentication pipeline must complete with a success status.");
            Assert.IsTrue(_authService.IsLoggedIn, "IsLoggedIn state must accurately reflect active session.");
            Assert.IsFalse(string.IsNullOrEmpty(_authService.PlayerId), "PlayerId must be successfully retrieved from PlayFab responses.");
        }

        [UnityTest]
        public IEnumerator PlayFab_SaveAndLoadUserData_RetainsDataIntegrity() {
            // 1. Prerequisite: Session authentication must be active to process user storage
            Task<bool> loginTask = _authService.LoginAnonymousAsync();
            while (!loginTask.IsCompleted) yield return null;

            Assert.IsTrue(loginTask.Result, "Prerequisite setup failed: Cloud authentication aborted.");

            // 2. Prepare mock payload structural schema
            var testPayload = new TestSaveData { MockScore = 4200, MockName = "SeniorDev_Abraham" };
            const string targetKey = "integration_test_key";

            // 3. Save payload to PlayFab Player Data
            Task<bool> saveTask = _saveService.SaveDataAsync(targetKey, testPayload);
            while (!saveTask.IsCompleted) yield return null;

            Assert.IsTrue(saveTask.Result, "Cloud storage transmission pipeline failed execution.");

            // 4. Retrieve payload from PlayFab Player Data
            Task<TestSaveData> loadTask = _saveService.LoadDataAsync<TestSaveData>(targetKey);
            while (!loadTask.IsCompleted) yield return null;

            // 5. Verification evaluation
            TestSaveData dynamicResult = loadTask.Result;
            Assert.IsNotNull(dynamicResult, "Deserialized cloud data must not return null handles.");
            Assert.AreEqual(testPayload.MockScore, dynamicResult.MockScore, "Numerical balance integrity corrupted over network transport.");
            Assert.AreEqual(testPayload.MockName, dynamicResult.MockName, "String validation failed matching historical snapshot.");
        }

        [UnityTest]
        public IEnumerator PlayFab_LoginWithDeviceAsync_ExecutesSuccessfully() {
            // Act: Call the device-specific fallback implementation explicitly
            Task<bool> deviceLoginTask = _authService.LoginWithDeviceAsync();

            while (!deviceLoginTask.IsCompleted) {
                yield return null;
            }

            // Assert: Verify it correctly re-routes to the underlying token logic
            Assert.IsTrue(deviceLoginTask.Result, "LoginWithDeviceAsync must execute the validation loop successfully.");
            Assert.IsTrue(_authService.IsLoggedIn);
        }

        [UnityTest]
        public IEnumerator PlayFab_Z_LoginWithError_TriggersErrorCallbackAndLogsFailure() {
            // 1. Setup: Hacemos el Regex ultra flexible para capturar CUALQUIER texto de error que arroje el callback
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex(@"^\[PlayFab Auth Error\].*"));

            // 2. Rompemos la configuración para forzar al servidor a rechazar la solicitud
            string originalTitleId = PlayFabSettings.staticSettings.TitleId;
            PlayFabSettings.staticSettings.TitleId = "INVALID_ID_404";

            // 3. Act: Disparamos el login
            Task<bool> brokenLoginTask = _authService.LoginAnonymousAsync();

            while (!brokenLoginTask.IsCompleted) {
                yield return null;
            }

            // 4. Restauramos el entorno inmediatamente
            PlayFabSettings.staticSettings.TitleId = originalTitleId;

            // 5. Assert: El sistema debe resolver en falso limpiamente
            Assert.IsFalse(brokenLoginTask.Result, "The auth service must resolve with a false result on network/config errors.");
        }

        /// <summary>
        /// Highly predictable structured serializable mock model for integration testing data pipeline boundaries.
        /// </summary>
        [System.Serializable]
        private class TestSaveData {
            public int MockScore;
            public string MockName;
        }
    }
}