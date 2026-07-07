using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Controllers {
    /// <summary>
    /// Pure C# Navigation Controller managing canvas routing, menu visibility events, 
    /// application shutdown hooks, and high-performance cross-platform input processing.
    /// </summary>
    public class NavigationController : IStartable, IDisposable, ITickable {
        private readonly DashboardMenuView _dashboardView;
        private readonly InputActionReference _toggleMenuAction;

        private bool _isMenuOpen = false;

        // Reactive events to let other controllers toggle their interaction visibility states cleanly
        public event Action OnMenuOpened;
        public event Action OnMenuClosed;

        /// <summary>
        /// Constructor injected via VContainer containing localized view architecture and mapped action assets.
        /// </summary>
        public NavigationController(DashboardMenuView dashboardView, InputActionReference toggleMenuAction = null) {
            _dashboardView = dashboardView;
            _toggleMenuAction = toggleMenuAction;
        }

        public void Start() {
            _dashboardView.OnCloseRequested += HandleCloseDashboard;
            _dashboardView.OnLinkAccountRequested += HandleLinkAccountTriggered;
            _dashboardView.OnExitApplicationRequested += HandleExitApplicationTriggered;
            _dashboardView.OnGameSwitchRequested += HandleGameSwitchTriggered;

            // Secure the InputAction is safely active across the platform lifecycle
            if (_toggleMenuAction != null && _toggleMenuAction.action != null) {
                _toggleMenuAction.action.Enable();
            }
        }

        public void Dispose() {
            if (_dashboardView != null) {
                _dashboardView.OnCloseRequested -= HandleCloseDashboard;
                _dashboardView.OnLinkAccountRequested -= HandleLinkAccountTriggered;
                _dashboardView.OnExitApplicationRequested -= HandleExitApplicationTriggered;
                _dashboardView.OnGameSwitchRequested -= HandleGameSwitchTriggered;
            }

            if (_toggleMenuAction != null && _toggleMenuAction.action != null) {
                _toggleMenuAction.action.Disable();
            }
        }

        /// <summary>
        /// Frame update execution hook driven by VContainer pipeline loop.
        /// </summary>
        public void Tick() {
            if (_toggleMenuAction == null || _toggleMenuAction.action == null) return;

            // Triggered checks handling frame-perfect user inputs safely
            if (_toggleMenuAction.action.WasPressedThisFrame()) {
                ToggleMenuState();
            }
        }

        private void ToggleMenuState() {
            if (_isMenuOpen) {
                HandleCloseDashboard();
            }
            else {
                // Future PlayFab state can pass explicit user credential naming strings here
                OpenMenu("PlayFab Guest Profile");
            }
        }

        public void OpenMenu(string currentAccountStatus) {
            Debug.Log("[Navigation] Intercepting execution context to mount global dashboard.");
            _isMenuOpen = true;
            _dashboardView.ShowDashboard(currentAccountStatus);
            OnMenuOpened?.Invoke();
        }

        private void HandleCloseDashboard() {
            Debug.Log("[Navigation] Dismounting global dashboard view container.");
            _isMenuOpen = false;
            _dashboardView.HideDashboard();
            OnMenuClosed?.Invoke();
        }

        private void HandleLinkAccountTriggered() {
            Debug.Log("[Navigation] Redirecting execution to the Account Linking subsystem.");
        }

        private void HandleGameSwitchTriggered(string targetGameKey) {
            Debug.Log($"[Navigation] Context Switch requested! Target Game Engine Signature: {targetGameKey}");
            if (targetGameKey != "Blackjack") {
                Debug.LogWarning($"[Navigation] Engine for {targetGameKey} is currently stubbed out inside TASK-4.5.");
            }
        }

        private void HandleExitApplicationTriggered() {
            Debug.Log("[Navigation] Shutting down application session state pipeline...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}