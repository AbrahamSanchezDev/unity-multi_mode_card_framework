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

        private int _playerCardsSpawned;
        private int _dealerCardsSpawned;
        private List<GameObject> _spawnedCards = new List<GameObject>();
        // 6cm card width + 1cm margin gap
        private const float CardOffsetHorizontal = 0.07f;

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
            _playerCardsSpawned = 0;
            _dealerCardsSpawned = 0;
            foreach (var card in _spawnedCards) {
                Destroy(card);
            }
            _spawnedCards.Clear();
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
            int cardIndex = isPlayer ? _playerCardsSpawned : _dealerCardsSpawned;

            // Calculate the procedural local coordinate shift
            Vector3 localOffset = new Vector3(cardIndex * CardOffsetHorizontal, 0, 0);
            Vector3 targetPosition = anchor.TransformPoint(localOffset);
            Quaternion targetRotation = anchor.rotation;

            // Instantiate and inject your procedural parameters
            GameObject spawnedCard = Instantiate(cardPrefab, targetPosition, targetRotation);

            // Invoke your custom runtime card shader/atlas binder
            var faceGenerator = spawnedCard.GetComponent<CardFaceGenerator>();
            if (faceGenerator != null) {
                // Mapping: CardFramework Core Enum Ranks to your byte parameters
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

            if (isPlayer) _playerCardsSpawned++;
            else _dealerCardsSpawned++;
            _spawnedCards.Add(spawnedCard);
        }
    }
}