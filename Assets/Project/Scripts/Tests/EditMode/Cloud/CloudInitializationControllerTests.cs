using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;
using CardFramework.Core.Interfaces;
using CardFramework.Presentation.Controllers;
using CardFramework.Cloud.Interfaces;
using System.Threading.Tasks;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class CloudInitializationControllerTests {
        private MockCloudService _mockCloudService;
        private MockEconomyService _mockEconomyService;
        private CloudInitializationController _controller;

        [SetUp]
        public void Setup() {
            _mockCloudService = new MockCloudService();
            _mockEconomyService = new MockEconomyService();

            // Instantiate the system under test passing our isolated mock dependencies
            _controller = new CloudInitializationController(_mockCloudService, _mockEconomyService);
        }

        [Test]
        public void Controller_OnInitialize_TriggersSilentAuthenticationImmediately() {
            // Act
            _controller.Initialize();

            // Assert
            Assert.IsTrue(_mockCloudService.AuthenticateSilentlyCalled,
                "The controller must invoke silent authentication immediately upon initialization startup.");
        }

        [Test]
        public void Controller_OnAuthSuccess_SynchronizesServerEconomyBalances() {
            // Arrange
            _controller.Initialize();

            // Act - Simulate a successful callback from the cloud server
            _mockCloudService.SimulateAuthSuccess("PlayFab_User_87A1");

            // Assert
            Assert.IsTrue(_mockEconomyService.RefreshBalanceCalled,
                "Once authenticated, the system must immediately synchronize the player's wallet balance with the cloud.");
        }

        [Test]
        public void Controller_OnAuthSuccess_UnsubscribesToPreventMemoryLeaks() {
            // Arrange
            _controller.Initialize();
            _mockCloudService.SimulateAuthSuccess("PlayFab_User_87A1");

            // Act - Attempt to fire a second success event to verify unsubscription logic
            _mockEconomyService.ResetTracker();
            _mockCloudService.SimulateAuthSuccess("PlayFab_User_87A1");

            // Assert
            Assert.IsFalse(_mockEconomyService.RefreshBalanceCalled,
                "The controller must unsubscribe from authentication events to safely mitigate memory leaks.");
        }

        [Test]
        public void Controller_OnAuthFailed_LogsErrorAndUnsubscribesClean() {
            // Arrange
            _controller.Initialize();

            // Tell Unity's Test Runner to intercept and validate the expected error log pattern
            LogAssert.Expect(LogType.Error, "[Boot] Critical Cloud Connection Failure: Network Timeout");

            // Act - Simulate a network connection failure scenario
            _mockCloudService.SimulateAuthFailure("Network Timeout");

            // Assert
            _mockEconomyService.ResetTracker();
            _mockCloudService.SimulateAuthSuccess("PlayFab_User_87A1");

            Assert.IsFalse(_mockEconomyService.RefreshBalanceCalled,
                "The controller must clean up its event hooks even after a critical boot routine failure.");
        }

        /// <summary>
        /// Controlled test stub simulating cloud network behaviors and PlayFab ID processing instantly.
        /// </summary>
        private class MockCloudService : ICloudService {
            public event Action OnAuthenticationSuccess;
            public event Action<string> OnAuthenticationFailed;

            public string PlayerId { get; private set; }
            public bool AuthenticateSilentlyCalled { get; private set; }

            public void AuthenticateSilently() {
                AuthenticateSilentlyCalled = true;
            }

            public void SimulateAuthSuccess(string targetPlayerId) {
                PlayerId = targetPlayerId;
                OnAuthenticationSuccess?.Invoke();
            }

            public void SimulateAuthFailure(string reason) {
                OnAuthenticationFailed?.Invoke(reason);
            }

            public bool IsAuthenticated => !string.IsNullOrEmpty(PlayerId);

            public Task<string> GenerateLinkingPINAsync() {
                throw new NotImplementedException();
            }
            public Task<bool> LinkAccountWithPINAsync(string pinCode) {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Economy service test stub tracking balance synchronization requests.
        /// </summary>
        private class MockEconomyService : IEconomyService {
            public event Action<int> OnBalanceUpdated;
#pragma warning disable CS0067
            public event Action<string> OnEconomyError;
#pragma warning restore CS0067

            public int CurrentGold { get; set; } = 1000;
            public bool RefreshBalanceCalled { get; private set; }

            public void RefreshBalance() {
                RefreshBalanceCalled = true;
                OnBalanceUpdated?.Invoke(CurrentGold);
            }

            public void CreditGold(int amount) => CurrentGold += amount;
            public void DebitGold(int amount) => CurrentGold -= amount;

            public void ResetTracker() {
                RefreshBalanceCalled = false;
            }
        }
    }
}