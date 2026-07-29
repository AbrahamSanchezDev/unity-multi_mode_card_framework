using System;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Presentation.Interfaces;
using CardFramework.Core.Models;
using System.Collections.Generic;
using DG.Tweening; // Import DOTween namespace

namespace CardFramework.Presentation.Views {
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
        private VisualElement _screenContainer;

        private Label _lblWalletBalance;

        public event Action OnHitRequested;
        public event Action OnStandRequested;
        public event Action OnRestartRequested;
        public event Action OnMenuRequested;

        #region 3D Spawning & Animation Architecture

        [Header("3D Spawning Architecture")]
        [SerializeField] private CardsGraphics cardsGraphics;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform playerSpawnAnchor;
        [SerializeField] private Transform dealerSpawnAnchor;

        [Header("Deck Setup & Motion Polish")]
        [SerializeField] private Transform deckSpawnAnchor; // Assign your physical shoe/deck location in Editor
        [SerializeField] private float dealDuration = 0.45f;
        [SerializeField] private Ease dealEase = Ease.OutQuad;

        // 6cm card width + 1cm margin gap
        private const float CardOffsetHorizontal = 0.075f;
        // Define a tiny depth step between cards (e.g., 3mm or 0.003f)
        private const float CardOffsetDepth = 0.002f;

        private readonly List<Transform> _playerCardTransforms = new();
        private readonly List<Transform> _dealerCardTransforms = new();

        #endregion

        private void OnEnable() {
            var uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            _root = uiDocument.rootVisualElement;
            _screenContainer = _root.Q<VisualElement>(className: "screen-container");
            _hitButton = _root.Q<Button>("hit-button");
            _standButton = _root.Q<Button>("stand-button");
            _restartButton = _root.Q<Button>("restart-button");
            _playerScoreLabel = _root.Q<Label>("player-score-label");
            _dealerScoreLabel = _root.Q<Label>("dealer-score-label");
            _outcomeMessageVisualElement = _root.Q<VisualElement>("outcome-message-label");
            _lblWalletBalance = _root.Q<Label>("lbl-wallet-balance");

            var btnHamburger = _root.Q<Button>("btn-hamburger-menu");
            if (btnHamburger != null) {
                btnHamburger.clicked += () => OnMenuRequested?.Invoke();
            }

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

            ValidateVisualTreeBindings();

            _hitButton.clicked += () => OnHitRequested?.Invoke();
            _standButton.clicked += () => OnStandRequested?.Invoke();
            _restartButton.clicked += () => OnRestartRequested?.Invoke();

            if (_lblWalletBalance != null) {
                _lblWalletBalance.text = "Balance: -- GD";
                _lblWalletBalance.style.color = Color.white;
            }
        }

        private void OnDisable() {
            if (_hitButton != null) _hitButton.clicked -= () => OnHitRequested?.Invoke();
            if (_standButton != null) _standButton.clicked -= () => OnStandRequested?.Invoke();
            if (_restartButton != null) _restartButton.clicked -= () => OnRestartRequested?.Invoke();
        }

        public void UpdatePlayerScore(int score) => _playerScoreLabel.text = $"Player: {score}";
        public void UpdateDealerScore(int score) => _dealerScoreLabel.text = $"Dealer: {score}";

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

            // Kill active tweens on cards before destroying them to avoid memory warnings
            foreach (var t in _playerCardTransforms) {
                if (t != null) {
                    t.DOKill();
                    Destroy(t.gameObject);
                }
            }
            _playerCardTransforms.Clear();

            foreach (var t in _dealerCardTransforms) {
                if (t != null) {
                    t.DOKill();
                    Destroy(t.gameObject);
                }
            }
            _dealerCardTransforms.Clear();
        }

        public void SetInteractionState(bool canInteract) {
            _hitButton.SetEnabled(canInteract);
            _standButton.SetEnabled(canInteract);
            if (_screenContainer == null) {
                Debug.LogWarning($"[{name}]: _screenContainer is null. Cannot set interaction state.");
                return;
            }

            _screenContainer.pickingMode = canInteract ? PickingMode.Position : PickingMode.Ignore;
        }

        private void ValidateVisualTreeBindings() {
            if (_hitButton == null || _standButton == null || _restartButton == null ||
                _playerScoreLabel == null || _dealerScoreLabel == null || _outcomeMessageLabel == null) {
                Debug.LogError($"[{name}]: Missing critical VisualElements inside the UXML tree hierarchy. Verify element Names.");
            }
        }

        public void SpawnPhysicalCard(CardData card, bool isPlayer) {
            Transform targetAnchor = isPlayer ? playerSpawnAnchor : dealerSpawnAnchor;
            List<Transform> activeList = isPlayer ? _playerCardTransforms : _dealerCardTransforms;

            Transform startPoint = deckSpawnAnchor != null ? deckSpawnAnchor : targetAnchor;

            // 1. Instantiate new card at deck position
            GameObject spawnedCard = Instantiate(cardPrefab, startPoint.position, startPoint.rotation, targetAnchor);

            // 2. Configure card face graphics
            var faceGenerator = spawnedCard.GetComponent<CardFaceGenerator>();
            if (faceGenerator != null) {
                faceGenerator.GenerateCard(card, cardsGraphics);
            }

            activeList.Add(spawnedCard.transform);

            int totalCards = activeList.Count;
            int cardIndex = totalCards - 1;

            // 3. Apply Z depth offset ONLY for player cards; dealer cards remain flat at Z = 0f
            float depthOffset = isPlayer ? (cardIndex * CardOffsetDepth) : 0f;

            float newCardTargetX = cardIndex * CardOffsetHorizontal - ((totalCards - 1) * CardOffsetHorizontal / 2f);
            Vector3 flightTargetPos = new Vector3(newCardTargetX, 0f, depthOffset);

            // 4. Phase 1: Animate ONLY the newly spawned card from deck to target anchor
            spawnedCard.transform.DOKill();

            Sequence dealSequence = DOTween.Sequence();
            dealSequence.Join(spawnedCard.transform.DOLocalMove(flightTargetPos, dealDuration).SetEase(dealEase));
            dealSequence.Join(spawnedCard.transform.DOLocalRotate(Vector3.zero, dealDuration).SetEase(dealEase));

            // 5. Phase 2: Re-center hand on complete, passing isPlayer flag
            dealSequence.OnComplete(() => {
                ReCenterHand(activeList, isPlayer);
            });
        }

        private void ReCenterHand(List<Transform> handTransforms, bool isPlayer) {
            int count = handTransforms.Count;
            if (count == 0) return;

            float totalWidth = (count - 1) * CardOffsetHorizontal;
            float startX = -totalWidth / 2f;

            Sequence centerSequence = DOTween.Sequence();

            for (int i = 0; i < count; i++) {
                Transform cardTransform = handTransforms[i];
                if (cardTransform == null) continue;

                float targetLocalX = startX + (i * CardOffsetHorizontal);
                float targetLocalZ = isPlayer ? (i * CardOffsetDepth) : 0f;

                Vector3 finalPos = new Vector3(targetLocalX, 0f, targetLocalZ);

                cardTransform.DOKill();
                centerSequence.Join(cardTransform.DOLocalMove(finalPos, 0.25f).SetEase(Ease.OutCubic));
                centerSequence.Join(cardTransform.DOLocalRotate(Vector3.zero, 0.25f));
            }
        }

        public void ShowUi(bool show) {
            if (_root != null) {
                _root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

    }
}