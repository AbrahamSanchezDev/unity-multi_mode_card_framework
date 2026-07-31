using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using CardFramework.Core.Models;
using CardFramework.Presentation.Interfaces;
using CardFramework.Core.Engines;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class TexasHoldemView : MonoBehaviour, ITexasHoldemView {
        private VisualElement _root;
        private Label _lblWalletBalance;
        private Label _lblRoundState;
        private Label _lblPlayerHand;
        private Label _lblCommunityCards;
        private Label _lblOutcome;
        private Button _btnDeal;
        private Button _btnFold;
        private Button _btnRestart;
        private VisualElement _outcomeMessageVisualElement;

        [Header("3D Spawning Architecture")]
        [SerializeField] private CardsGraphics cardsGraphics;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform playerSpawnAnchor;
        [SerializeField] private Transform dealerSpawnAnchor;

        [Header("Deck Setup & Motion Polish")]
        [SerializeField] private Transform deckSpawnAnchor;
        [SerializeField] private float dealDuration = 0.45f;
        [SerializeField] private Ease dealEase = Ease.OutQuad;

        private const float CardOffsetHorizontal = 0.075f;
        private const float CardOffsetDepth = 0.002f;

        private readonly List<Transform> _playerCardTransforms = new();
        private readonly List<Transform> _communityCardTransforms = new();

        public event Action OnDealRequested;
        public event Action OnRestartRequested;
        public event Action OnFoldRequested;
        public event Action OnMenuRequested;

        private void OnEnable() {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) {
                Debug.LogWarning($"[{name}]: Missing UIDocument component.");
                return;
            }

            uiDocument.enabled = true;
            _root = uiDocument.rootVisualElement;
            if (_root == null) return;

            _lblWalletBalance = _root.Q<Label>("lbl-wallet-balance");
            _lblRoundState = _root.Q<Label>("lbl-round-state");
            _lblPlayerHand = _root.Q<Label>("lbl-player-hand");
            _lblCommunityCards = _root.Q<Label>("lbl-community-cards");
            _outcomeMessageVisualElement = _root.Q<VisualElement>("outcome-message-label");
            _lblOutcome = _root.Q<Label>("lbl-outcome");
            _btnDeal = _root.Q<Button>("btn-deal");
            _btnFold = _root.Q<Button>("btn-fold");
            _btnRestart = _root.Q<Button>("btn-restart");

            var btnHamburger = _root.Q<Button>("btn-hamburger-menu");
            if (btnHamburger != null) {
                btnHamburger.clicked += () => OnMenuRequested?.Invoke();
            }

            if (_btnDeal != null) _btnDeal.clicked += () => OnDealRequested?.Invoke();
            if (_btnFold != null) _btnFold.clicked += () => OnFoldRequested?.Invoke();
            if (_btnRestart != null) _btnRestart.clicked += () => OnRestartRequested?.Invoke();

            UpdateWalletBalance(0);
            ClearOutcome();
        }

        private void OnDisable() {
            if (_btnDeal != null) _btnDeal.clicked -= () => OnDealRequested?.Invoke();
            if (_btnFold != null) _btnFold.clicked -= () => OnFoldRequested?.Invoke();
            if (_btnRestart != null) _btnRestart.clicked -= () => OnRestartRequested?.Invoke();
        }

        public void ClearTable() {
            foreach (var t in _playerCardTransforms) {
                if (t != null) {
                    t.DOKill();
                    Destroy(t.gameObject);
                }
            }
            _playerCardTransforms.Clear();

            foreach (var t in _communityCardTransforms) {
                if (t != null) {
                    t.DOKill();
                    Destroy(t.gameObject);
                }
            }
            _communityCardTransforms.Clear();

            if (_lblPlayerHand != null) _lblPlayerHand.text = "No hand yet";
            if (_lblCommunityCards != null) _lblCommunityCards.text = "No community cards yet";
            if (_lblRoundState != null) _lblRoundState.text = "Round: Pre-Flop";
            ClearOutcome();
        }

        public void RenderRoundState(TexasHoldemEngine.RoundState roundState, List<CardData> playerHand, List<CardData> communityCards) {
            if (_lblRoundState != null) _lblRoundState.text = $"Round: {roundState}";

            if (_lblPlayerHand != null) {
                _lblPlayerHand.text = FormatCardList(playerHand, "No hole cards yet");
            }

            if (_lblCommunityCards != null) {
                _lblCommunityCards.text = FormatCardList(communityCards, "No community cards yet");
            }
        }

        public void SpawnPhysicalCard(CardData card, bool isPlayer) {
            Transform targetAnchor = isPlayer ? playerSpawnAnchor : dealerSpawnAnchor;
            List<Transform> activeList = isPlayer ? _playerCardTransforms : _communityCardTransforms;
            Transform startPoint = deckSpawnAnchor != null ? deckSpawnAnchor : targetAnchor;

            if (cardPrefab == null || targetAnchor == null) {
                Debug.LogWarning($"[{name}]: Missing cardPrefab or spawn anchor for Texas Hold'em physical card rendering.");
                return;
            }

            GameObject spawnedCard = Instantiate(cardPrefab, startPoint.position, startPoint.rotation, targetAnchor);

            var faceGenerator = spawnedCard.GetComponent<CardFaceGenerator>();
            if (faceGenerator != null) {
                faceGenerator.GenerateCard(card, cardsGraphics);
            }

            activeList.Add(spawnedCard.transform);

            int totalCards = activeList.Count;
            int cardIndex = totalCards - 1;
            float depthOffset = isPlayer ? (cardIndex * CardOffsetDepth) : 0f;
            float newCardTargetX = cardIndex * CardOffsetHorizontal - ((totalCards - 1) * CardOffsetHorizontal / 2f);
            Vector3 flightTargetPos = new Vector3(newCardTargetX, 0f, depthOffset);

            spawnedCard.transform.DOKill();
            Sequence dealSequence = DOTween.Sequence();
            dealSequence.Join(spawnedCard.transform.DOLocalMove(flightTargetPos, dealDuration).SetEase(dealEase));
            dealSequence.Join(spawnedCard.transform.DOLocalRotate(Vector3.zero, dealDuration).SetEase(dealEase));
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

        public void UpdateWalletBalance(int balance) {
            if (_lblWalletBalance != null) _lblWalletBalance.text = $"Balance: {balance} GD";
        }

        public void DisplayOutcome(string message) {
            if (_outcomeMessageVisualElement != null) {
                _outcomeMessageVisualElement.style.display = DisplayStyle.Flex;
            }

            if (_lblOutcome != null) {
                _lblOutcome.text = message;
            }
        }

        public void ClearOutcome() {
            if (_outcomeMessageVisualElement != null) {
                _outcomeMessageVisualElement.style.display = DisplayStyle.None;
            }

            if (_lblOutcome != null) {
                _lblOutcome.text = string.Empty;
            }
        }

        public void SetInteractionState(bool canInteract) {
            if (_btnDeal != null) _btnDeal.SetEnabled(canInteract);
            if (_btnFold != null) _btnFold.SetEnabled(canInteract);
            if (_btnRestart != null) _btnRestart.SetEnabled(canInteract);
        }

        public void ShowUi(bool show) {
            if (_root != null) {
                _root.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static string FormatCardList(List<CardData> cards, string emptyText) {
            if (cards == null || cards.Count == 0) return emptyText;

            var formatted = new List<string>();
            foreach (var card in cards) {
                formatted.Add($"{card.CardRank} of {card.CardSuit}");
            }

            return string.Join(", ", formatted);
        }
    }
}
