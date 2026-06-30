using System;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Views {
    /// <summary>
    /// UI Toolkit implementation mapping UXML VisualElements to the IBlackjackView architectural boundary.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class BlackjackView : MonoBehaviour, IBlackjackView {

        public bool HasAll;
        private VisualElement _root;
        private Button _hitButton;
        private Button _standButton;
        private Button _restartButton;
        private Label _playerScoreLabel;
        private Label _dealerScoreLabel;
        private Label _outcomeMessageLabel;
        private VisualElement _outcomeMessageVisualElement;

        // Implementation of the architectural view contract events
        public event Action OnHitRequested;
        public event Action OnStandRequested;
        public event Action OnRestartRequested;

        private void OnEnable() {
            // Acquire the root visual element from the native UIDocument component
            var uiDocument = GetComponent<UIDocument>();
            _root = uiDocument.rootVisualElement;

            // Query elements using standard UXML naming conventions
            _hitButton = _root.Q<Button>("hit-button");
            _standButton = _root.Q<Button>("stand-button");
            _restartButton = _root.Q<Button>("restart-button");
            _playerScoreLabel = _root.Q<Label>("player-score-label");
            _dealerScoreLabel = _root.Q<Label>("dealer-score-label");
            _outcomeMessageVisualElement = _root.Q<VisualElement>("outcome-message-label");
            if (_outcomeMessageVisualElement != null) {
                _outcomeMessageLabel = _outcomeMessageVisualElement.Q<Label>();
            }
            if (_hitButton != null && _standButton != null && _restartButton != null &&
               _playerScoreLabel != null && _dealerScoreLabel != null && _outcomeMessageLabel != null) {
                HasAll = true;
            }
            else {
                HasAll = false;
            }
            // Sanity check for UI Toolkit bindings
            ValidateVisualTreeBindings();

            // Register callbacks into the UI Toolkit architecture loop
            _hitButton.clicked += () => OnHitRequested?.Invoke();
            _standButton.clicked += () => OnStandRequested?.Invoke();
            _restartButton.clicked += () => OnRestartRequested?.Invoke();
        }

        private void OnDisable() {
            // Clean up callbacks to prevent memory fragmentation
            if (_hitButton != null) _hitButton.clicked -= () => OnHitRequested?.Invoke();
            if (_standButton != null) _standButton.clicked -= () => OnStandRequested?.Invoke();
            if (_restartButton != null) _restartButton.clicked -= () => OnRestartRequested?.Invoke();
        }

        public void UpdatePlayerScore(int score) {
            _playerScoreLabel.text = $"Player: {score}";
        }

        public void UpdateDealerScore(int score) {
            _dealerScoreLabel.text = $"Dealer: {score}";
        }

        public void DisplayWinner(string winnerName) {
            _outcomeMessageVisualElement.style.display = DisplayStyle.Flex;
            _outcomeMessageLabel.text = winnerName;
        }

        public void ClearTable() {
            _outcomeMessageLabel.text = string.Empty;
            _outcomeMessageVisualElement.style.display = DisplayStyle.None;
        }

        public void SetInteractionState(bool canInteract) {
            _hitButton.SetEnabled(canInteract);
            _standButton.SetEnabled(canInteract);
        }

        private void ValidateVisualTreeBindings() {
            if (_hitButton == null || _standButton == null || _restartButton == null ||
                _playerScoreLabel == null || _dealerScoreLabel == null || _outcomeMessageLabel == null) {
                Debug.LogError($"[{name}]: Missing critical VisualElements inside the UXML tree hierarchy. Verify element Names.");
            }
            if (_hitButton == null) Debug.LogError($"[{name}]: Missing 'hit-button' VisualElement.");
            if (_standButton == null) Debug.LogError($"[{name}]: Missing 'stand-button' VisualElement.");
            if (_restartButton == null) Debug.LogError($"[{name}]: Missing 'restart-button' VisualElement.");
            if (_playerScoreLabel == null) Debug.LogError($"[{name}]: Missing 'player-score-label' VisualElement.");
            if (_dealerScoreLabel == null) Debug.LogError($"[{name}]: Missing 'dealer-score-label' VisualElement.");
            if (_outcomeMessageLabel == null) Debug.LogError($"[{name}]: Missing 'outcome-message-label' VisualElement.");
        }
    }
}