using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using CardFramework.Presentation.Views;
using VContainer;

namespace CardFramework.Presentation.Controllers {
    /// <summary>
    /// Pure C# Navigation Controller managing canvas routing, menu visibility events, 
    /// application shutdown hooks, and high-performance cross-platform input processing.
    /// </summary>
    public class NavigationController : IStartable, IDisposable, ITickable {

        public Action<string> OnSwitchGameRequested;
        public Action<string> OnSwitchGameCompleted;
        private DashboardMenuView _dashboardView;
        private GameTableManager _tableManager;

        private readonly InputActionReference _toggleMenuAction;

        private bool _isMenuOpen = false;
        // Test hook: allow forcing the toggle branch from tests when input simulation is unreliable
        private bool _forceToggleThisFrame = false;

        // Reactive events to let other controllers toggle their interaction visibility states cleanly
        public event Action OnMenuOpened;
        public event Action OnMenuClosed;

        /// <summary>
        /// Constructor injected via VContainer containing localized view architecture and mapped action assets.
        /// </summary>
        public NavigationController(DashboardMenuView dashboardView, GameTableManager tableManager, InputActionReference toggleMenuAction = null) {
            _dashboardView = dashboardView;
            _toggleMenuAction = toggleMenuAction;
            _tableManager = tableManager;
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
            if (_toggleMenuAction.action.WasPressedThisFrame() || _forceToggleThisFrame) {
                ToggleMenuState();
                _forceToggleThisFrame = false;
            }
        }

        /// <summary>
        /// Test helper to force the toggle branch inside Tick() on the next frame.
        /// Use only in tests where InputSystem simulation is unreliable.
        /// </summary>
        public void ForceNextTickToggle() {
            _forceToggleThisFrame = true;
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

            OnSwitchGameRequested?.Invoke(targetGameKey);
            HandleCloseDashboard();
            
            // Move the player spatially to the requested table setup
            _tableManager?.SwitchTable(targetGameKey);
            _dashboardView.UpdateActiveGameVisuals(targetGameKey);

            // Hide the dashboard menu overlay once the switch execution concludes
            _dashboardView.HideDashboard();


            OnSwitchGameCompleted?.Invoke(targetGameKey);
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