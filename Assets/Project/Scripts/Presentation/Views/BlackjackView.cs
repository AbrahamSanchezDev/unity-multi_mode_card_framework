using System;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Presentation.Interfaces;
using CardFramework.Core.Models;
using System.Collections.Generic;

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

        private Label _lblWalletBalance;

        // Implementation of the architectural view contract events
        public event Action OnHitRequested;
        public event Action OnStandRequested;
        public event Action OnRestartRequested;

        #region 3D Spawning

        [Header("3D Spawning Architecture")]
        [SerializeField] private CardsGraphics cardsGraphics;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform playerSpawnAnchor;
        [SerializeField] private Transform dealerSpawnAnchor;

        // 6cm card width + 1cm margin gap
        private const float CardOffsetHorizontal = 0.07f;

        // Track active runtime card instances to dynamically recalculate hand centers
        private readonly List<Transform> _playerCardTransforms = new();
        private readonly List<Transform> _dealerCardTransforms = new();

        #endregion

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
            // Fetch the new wallet element text label
            _lblWalletBalance = _root.Q<Label>("lbl-wallet-balance");

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

            // Set an initial placeholder text safely text forced white for contrast
            if (_lblWalletBalance != null) {
                _lblWalletBalance.text = "Balance: -- GD";
                _lblWalletBalance.style.color = Color.white;
            }
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
        
        public void UpdateWalletBalance(int freshBalance) {
            if (_lblWalletBalance != null) {
                _lblWalletBalance.text = $"Balance: {freshBalance} GD";
            }
        }

        public void DisplayWinner(string winnerName) {
            _outcomeMessageVisualElement.style.display = DisplayStyle.Flex;
            _outcomeMessageLabel.text = winnerName;
        }

        public void ClearTable() {
            _outcomeMessageLabel.text = string.Empty;
            _outcomeMessageVisualElement.style.display = DisplayStyle.None;

            // Clear Player visual objects
            foreach (var t in _playerCardTransforms) { if (t != null) Destroy(t.gameObject); }
            _playerCardTransforms.Clear();

            // Clear Dealer visual objects
            foreach (var t in _dealerCardTransforms) { if (t != null) Destroy(t.gameObject); }
            _dealerCardTransforms.Clear();
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

        public void SpawnPhysicalCard(CardData card, bool isPlayer) {
            Transform anchor = isPlayer ? playerSpawnAnchor : dealerSpawnAnchor;
            System.Collections.Generic.List<Transform> activeList = isPlayer ? _playerCardTransforms : _dealerCardTransforms;

            // Instantiate the physical asset as a direct child of its target spatial anchor
            GameObject spawnedCard = Instantiate(cardPrefab, anchor);


            // Invoke your custom runtime card shader/atlas binder parameters
            var faceGenerator = spawnedCard.GetComponent<CardFaceGenerator>();
            if (faceGenerator != null) {
                CardData.Rank rank = card.CardRank;
                bool isBlack = card.CardSuit == CardData.Suit.Spades || card.CardSuit == CardData.Suit.Clubs;
                // Fetch your Suit icon from your asset database/cache as needed
                Sprite suitIcon = cardsGraphics != null ? cardsGraphics.GetSuitIcon(card.CardSuit) : null;
                Sprite faceSprite = null;
                if (card.CardRank == CardData.Rank.Jack || card.CardRank == CardData.Rank.Queen || card.CardRank == CardData.Rank.King) {
                    // Fetch the face card sprite for Jack, Queen, King
                    faceSprite = cardsGraphics != null ? cardsGraphics.GetFaceCardSprite(card) : null;
                }

                faceGenerator.GenerateCard(suitIcon, rank, faceSprite, isBlack);
            }


            activeList.Add(spawnedCard.transform);

            // Dynamic auto-centering calculation for the entire hand layout
            int totalCards = activeList.Count;
            float totalWidth = (totalCards - 1) * CardOffsetHorizontal;
            float startX = -totalWidth / 2f;

            // Reposition all existing cards in this hand relative to the local origin
            for (int i = 0; i < totalCards; i++) {
                float localX = startX + (i * CardOffsetHorizontal);

                // Keep local Y and Z at 0 so they respect the Anchor's native transform layout orientation
                activeList[i].localPosition = new Vector3(localX, 0f, 0f);
                activeList[i].localRotation = Quaternion.identity;
            }


        }
    }
}