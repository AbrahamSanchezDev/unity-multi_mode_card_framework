using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using CardFramework.Core.Models;
using CardFramework.Presentation.Interfaces;
using CardFramework.Core.Engines;
using VContainer;

namespace CardFramework.Presentation.Views {
    [RequireComponent(typeof(UIDocument))]
    public class TexasHoldemView : CardsGameBaseView, ITexasHoldemView {
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
        [SerializeField] private Transform communitySpawnAnchor;
        [SerializeField] private Transform houseSpawnAnchor;

        [Header("Poker Table Layout")]
        public bool HouseHandActive = true;
        public bool AnimateOnlyNewCards = true;
        [SerializeField] private float playerCardSpread = 0.11f;
        [SerializeField] private float communityCardSpread = 0.09f;
        [SerializeField] private float communityCardDepth = 0.003f;

        [Header("Deck Setup & Motion Polish")]
        [SerializeField] private Transform deckSpawnAnchor;
        [SerializeField] private float dealDuration = 0.45f;
        [SerializeField] private Ease dealEase = Ease.OutQuad;

        private const float PlayerCardDepth = 0.002f;
        private readonly List<Transform> _playerCardTransforms = new();
        private readonly List<Transform> _communityCardTransforms = new();
        private readonly List<Transform> _houseCardTransforms = new();
        private readonly Dictionary<Transform, CardData> _cardDataByTransform = new();
        private IGameSettingsService _gameSettingsService;

        public event Action OnDealRequested;
        public event Action OnRestartRequested;
        public event Action OnFoldRequested;


        [Inject]
        public void Construct(IAudioService audioService, IGameSettingsService gameSettingsService, INotificationsView notificationsView) {
            _audioService = audioService;
            _gameSettingsService = gameSettingsService;
            _notificationsView = notificationsView;
            if (_gameSettingsService != null) {
                _gameSettingsService.OnCardDisplayTypeChanged += HandleCardDisplayTypeChanged;
            }
        }

        private void OnEnable() {

            _boxCollider = GetComponent<BoxCollider>();
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
                btnHamburger.clicked += HandleMenuClicked;
            }

            if (_btnDeal != null) _btnDeal.clicked += HandleDealClicked;
            if (_btnFold != null) _btnFold.clicked += HandleFoldClicked;
            if (_btnRestart != null) _btnRestart.clicked += HandleRestartClicked;

            UpdateWalletBalance(0);
            ClearOutcome();

            var handData = _root.Q<VisualElement>("hand_data");
            var communityData = _root.Q<VisualElement>("community_data");
            if (handData != null) {
                handData.style.display = DisplayStyle.None;
            }
            else {
                Debug.LogWarning($"[{name}]: Could not find hand data section on Texas Hold'em view.");
            }

            if (communityData != null) {
                communityData.style.display = DisplayStyle.None;
            }
            else {
                Debug.LogWarning($"[{name}]: Could not find community data section on Texas Hold'em view.");
            }
            Setup3DView();
        }

        private void OnDisable() {
            if (_btnDeal != null) _btnDeal.clicked -= HandleDealClicked;
            if (_btnFold != null) _btnFold.clicked -= HandleFoldClicked;
            if (_btnRestart != null) _btnRestart.clicked -= HandleRestartClicked;
            if (_gameSettingsService != null) {
                _gameSettingsService.OnCardDisplayTypeChanged -= HandleCardDisplayTypeChanged;
            }
        }


        #region 3D View Controls

        protected override void HandleMainButtonClicked() {
            HandleDealClicked();
        }
        protected override void HandleGiveUpClicked() {
            HandleFoldClicked();
        }

        protected override void HandleNewGameClicked() {
            HandleRestartClicked();
        }


        #endregion
        private void HandleDealClicked() {
            PlayButtonClickSound();
            OnDealRequested?.Invoke();
        }

        private void HandleFoldClicked() {
            PlayButtonClickSound();
            OnFoldRequested?.Invoke();
        }

        private void HandleRestartClicked() {
            PlayButtonClickSound();
            OnRestartRequested?.Invoke();
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

            foreach (var t in _houseCardTransforms) {
                if (t != null) {
                    t.DOKill();
                    Destroy(t.gameObject);
                }
            }
            _houseCardTransforms.Clear();
            _cardDataByTransform.Clear();

            foreach (var t in _communityCardTransforms) {
                if (t != null) {
                    t.DOKill();
                    Destroy(t.gameObject);
                }
            }
            _communityCardTransforms.Clear();

            foreach (var t in _houseCardTransforms) {
                if (t != null) {
                    t.DOKill();
                    Destroy(t.gameObject);
                }
            }
            _houseCardTransforms.Clear();

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
            SpawnPhysicalCard(card, isPlayer, false);
        }

        public void SpawnHousePlaceholders(int count) {
            if (houseSpawnAnchor == null || cardPrefab == null) {
                Debug.LogWarning($"[{name}]: Missing house spawn anchor or card prefab for Texas Hold'em dealer placeholder rendering.");
                return;
            }
            for (int i = 0; i < count; i++) {
                GameObject placeholderCard = Instantiate(cardPrefab, houseSpawnAnchor.position, houseSpawnAnchor.rotation, houseSpawnAnchor);
                placeholderCard.transform.localScale = Vector3.one;
                placeholderCard.transform.localPosition = new Vector3(i * playerCardSpread - ((count - 1) * playerCardSpread / 2f), 0f, i * PlayerCardDepth);
                placeholderCard.transform.localRotation = Quaternion.identity;
                placeholderCard.transform.localEulerAngles = new Vector3(0f, 180f, 0f); // Face down
                _houseCardTransforms.Add(placeholderCard.transform);
            }
        }

        public void RevealHouseHand(List<CardData> houseCards) {
            if (houseCards == null || houseCards.Count == 0) return;

            for (int i = 0; i < _houseCardTransforms.Count && i < houseCards.Count; i++) {
                var cardTransform = _houseCardTransforms[i];
                if (cardTransform == null) continue;

                cardTransform.localScale = Vector3.one;
                var faceGenerator = cardTransform.GetComponent<CardFaceGenerator>();
                if (faceGenerator != null) {
                    faceGenerator.GenerateCard(houseCards[i], cardsGraphics);
                    _cardDataByTransform[cardTransform] = houseCards[i];
                }

                cardTransform.DOKill();
                cardTransform.DOLocalMove(new Vector3(i * playerCardSpread - ((_houseCardTransforms.Count - 1) * playerCardSpread / 2f), 0f, i * PlayerCardDepth), 0.25f).SetEase(Ease.OutCubic);
                cardTransform.DOLocalRotate(Vector3.zero, 0.25f);
            }
        }

        public void SpawnPhysicalCard(CardData card, bool isPlayer, bool isHouseHand) {
            bool useHouseLane = isHouseHand && HouseHandActive;
            Transform targetAnchor = isPlayer ? playerSpawnAnchor : (useHouseLane && houseSpawnAnchor != null ? houseSpawnAnchor : communitySpawnAnchor);
            List<Transform> activeList = isPlayer ? _playerCardTransforms : (useHouseLane ? _houseCardTransforms : _communityCardTransforms);
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
            _cardDataByTransform[spawnedCard.transform] = card;

            int totalCards = activeList.Count;
            int cardIndex = totalCards - 1;
            float spread = isPlayer || useHouseLane ? playerCardSpread : communityCardSpread;
            float depthOffset = isPlayer || useHouseLane ? (cardIndex * PlayerCardDepth) : (cardIndex * communityCardDepth);
            float newCardTargetX = cardIndex * spread - ((totalCards - 1) * spread / 2f);
            Vector3 flightTargetPos = new Vector3(newCardTargetX, 0f, depthOffset);

            spawnedCard.transform.DOKill();
            if (!AnimateOnlyNewCards) {
                spawnedCard.transform.localPosition = flightTargetPos;
                spawnedCard.transform.localRotation = Quaternion.identity;
                ReCenterHand(activeList, isPlayer || useHouseLane);
                return;
            }

            Sequence dealSequence = DOTween.Sequence();
            dealSequence.Join(spawnedCard.transform.DOLocalMove(flightTargetPos, dealDuration).SetEase(dealEase));
            dealSequence.Join(spawnedCard.transform.DOLocalRotate(Vector3.zero, dealDuration).SetEase(dealEase));
            dealSequence.OnComplete(() => {
                ReCenterHand(activeList, isPlayer || useHouseLane);
            });
        }

        private void ReCenterHand(List<Transform> handTransforms, bool isPlayerLikeHand) {
            int count = handTransforms.Count;
            if (count == 0) return;

            float spread = isPlayerLikeHand ? playerCardSpread : communityCardSpread;
            float totalWidth = (count - 1) * spread;
            float startX = -totalWidth / 2f;
            Sequence centerSequence = DOTween.Sequence();

            for (int i = 0; i < count; i++) {
                Transform cardTransform = handTransforms[i];
                if (cardTransform == null) continue;

                float targetLocalX = startX + (i * spread);
                float targetLocalZ = isPlayerLikeHand ? (i * PlayerCardDepth) : (i * communityCardDepth);
                Vector3 finalPos = new Vector3(targetLocalX, 0f, targetLocalZ);

                cardTransform.DOKill();
                centerSequence.Join(cardTransform.DOLocalMove(finalPos, 0.25f).SetEase(Ease.OutCubic));
                centerSequence.Join(cardTransform.DOLocalRotate(Vector3.zero, 0.25f));
            }
        }

        public void UpdateWalletBalance(int balance) {
            if (_lblWalletBalance != null) _lblWalletBalance.text = $"Balance: {balance} GD";

            UpdateBalanceDisplay($"Balance: {balance} GD");
        }

        private void HandleCardDisplayTypeChanged(CardDisplayType newDisplayType) {
            RefreshRevealedCardsDisplay(newDisplayType);
        }

        private void RefreshRevealedCardsDisplay(CardDisplayType newDisplayType) {
            foreach (var kvp in _cardDataByTransform) {
                if (kvp.Key == null) continue;
                CardData cardData = kvp.Value;
                if (!cardData.HasBeenRevealed || !cardData.IsFaceUp) continue;
                var faceGenerator = kvp.Key.GetComponent<CardFaceGenerator>();
                if (faceGenerator == null || faceGenerator.DisplayType == newDisplayType) continue;
                faceGenerator.GenerateCard(cardData, cardsGraphics);
                faceGenerator.SetFaceUpMaterial(true);
            }
        }

        public void DisplayOutcome(string message) {
            if (_outcomeMessageVisualElement != null) {
                _outcomeMessageVisualElement.style.display = DisplayStyle.Flex;
            }

            if (_lblOutcome != null) {
                _lblOutcome.text = message;
            }
            UpdateFinalResultText(message);
            ShowFinalResultDisplay(true);
        }

        public void ClearOutcome() {
            if (_outcomeMessageVisualElement != null) {
                _outcomeMessageVisualElement.style.display = DisplayStyle.None;
            }

            if (_lblOutcome != null) {
                _lblOutcome.text = string.Empty;
            }

            UpdateFinalResultText(string.Empty);
            ShowFinalResultDisplay(false);
        }

        public void SetInteractionState(bool canInteract) {
            if (_btnDeal != null) _btnDeal.SetEnabled(canInteract);
            if (_btnFold != null) _btnFold.SetEnabled(canInteract);
            AllowResetButton(canInteract);
            Show3DView(canInteract);
        }

        public void AllowResetButton(bool allow) {
            if (_btnRestart != null) {
                _btnRestart.SetEnabled(allow);
            }
            ShowNewGameButton(allow);
        }

        public void SetRestartButtonEnabled(bool enabled) {
            if (_btnRestart != null) {
                _btnRestart.SetEnabled(enabled);
            }
            ShowNewGameButton(enabled);
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
