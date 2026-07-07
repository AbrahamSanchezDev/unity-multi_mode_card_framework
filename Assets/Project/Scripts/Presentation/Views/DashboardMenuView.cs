using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class DashboardMenuView : MonoBehaviour {
        public event Action OnCloseRequested;
        public event Action OnLinkAccountRequested;
        public event Action OnExitApplicationRequested;
        public event Action<string> OnGameSwitchRequested;

        private VisualElement _root;
        private Label _lblAccountStatus;

        private void OnEnable() {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;
            uiDocument.enabled = true;

            _root = uiDocument.rootVisualElement;
            _lblAccountStatus = _root.Q<Label>("lbl-account-status");

            // Wire UI Toolkit Interactions straight to architecture events
            _root.Q<Button>("btn-close-dash").clicked += () => OnCloseRequested?.Invoke();
            _root.Q<Button>("btn-open-linking").clicked += () => OnLinkAccountRequested?.Invoke();
            _root.Q<Button>("btn-exit-app").clicked += () => OnExitApplicationRequested?.Invoke();

            _root.Q<Button>("btn-game-blackjack").clicked += () => OnGameSwitchRequested?.Invoke("Blackjack");
            _root.Q<Button>("btn-game-solitaire").clicked += () => OnGameSwitchRequested?.Invoke("Solitaire");
            _root.Q<Button>("btn-game-texasholdem").clicked += () => OnGameSwitchRequested?.Invoke("TexasHoldem");

            _root.style.display = DisplayStyle.None;
        }

        public void ShowDashboard(string statusText) {
            if (_lblAccountStatus != null) _lblAccountStatus.text = statusText;
            if (_root != null) _root.style.display = DisplayStyle.Flex;
        }

        public void HideDashboard() {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }
    }
}