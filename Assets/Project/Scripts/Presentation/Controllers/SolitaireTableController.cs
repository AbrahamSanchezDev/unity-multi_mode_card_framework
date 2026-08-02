using System.Collections.Generic;
using UnityEngine;
using CardFramework.Core.Engines;
using CardFramework.Core.Interfaces;
using CardFramework.Core.Models;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Controllers {
    public class SolitaireTableController : BaseTableController {
        private readonly SolitaireEngine _engine;
        private readonly ISolitaireView _uiView;

        override public int MaxWager => MaxSolitaireWager;
        public const int MaxSolitaireWager = 50;

        public SolitaireTableController(
            SolitaireEngine engine,
            ISolitaireView solitaireView,
            IEconomyService economyService,
            IModalService modalService,
            BettingModalView bettingModalView,
            NavigationController navigationController,
            IAudioService audioService) {
            _engine = engine;
            _uiView = solitaireView;
            _economyService = economyService;
            _modalService = modalService;
            _bettingModalView = bettingModalView;
            _navigationController = navigationController;
            _audioService = audioService;

            _currencyDisplayHelper?.Dispose();
            if (_economyService != null) {
                _currencyDisplayHelper = new CurrencyDisplayHelper(_economyService, HandleBalanceUpdated);
            }
            Debug.Log("[Solitaire Controller] Initialized with SolitaireEngine and ISolitaireView.");
        }

        public override void Start() {
            base.Start();

            if (_uiView != null) {
                _uiView.OnStockTapped += HandleStockTapped;
                _uiView.OnRestartRequested += RequestNewGame;
                _uiView.OnTableauDropRequested += HandleTableauDrop;
                _uiView.OnFoundationDropRequested += HandleFoundationDrop;

                // Subscribe to the event for when the menu gets open
                _uiView.OnMenuRequested += HandleMenuToggleRequested;
            }
        }

        protected override void UnsubscribeEvents() {
            base.UnsubscribeEvents();

            if (_uiView != null) {
                _uiView.OnStockTapped -= HandleStockTapped;
                _uiView.OnRestartRequested -= RequestNewGame;
                _uiView.OnTableauDropRequested -= HandleTableauDrop;
                _uiView.OnFoundationDropRequested -= HandleFoundationDrop;
            }
        }

        protected override string GetGameModeKey() {
            return "Solitaire";
        }

        protected override void OnGameModeDeactivated() {
            base.OnGameModeDeactivated();
            _uiView?.ClearTable();
        }
        override public void ShowUI(bool show) {
            _uiView?.ShowUi(show);
        }

        protected override string GetDescriptionOnNewGameText() {
            return "This will discard your current Solitaire progress. Start a new round?";
        }

        protected override void BeginNewGame() {
            base.BeginNewGame();
            _uiView?.ClearTable();
        }

        protected override void InitializeEngine() {
            _engine.Initialize();
        }

        protected override void RefreshTableLayout() {
            _uiView?.RenderLayout(
                _engine.GetTableau(),
                _engine.GetFoundation(),
                _engine.GetStock(),
                _engine.GetWaste()
            );

            _uiView?.UpdateFoundationScore(GetFoundationCount(), 52);
            _uiView?.ClearOutcome();
        }

        protected override void UpdateWalletBalance(int newBalance) {
            if (_isGameModeActive) {
                _uiView?.UpdateWalletBalance(newBalance);
            }
        }

        private int GetFoundationCount() {
            int total = 0;
            var foundation = _engine.GetFoundation();
            if (foundation == null) return total;

            for (int i = 0; i < foundation.Length; i++) {
                if (foundation[i] != null) total += foundation[i].Count;
            }
            return total;
        }

        #region  Solitaire Specific Handlers

        private void HandleStockTapped() {
            if (!_isGameModeActive) return;

            _engine.DrawCard();
            PlayCardGrab();
            RefreshTableLayout();
        }

        private void HandleTableauDrop(List<CardData> cards, int sourceColumn, int startIndex, int targetColumn) {
            if (!_isGameModeActive) return;
            if (!TryMoveToTableau(cards, sourceColumn, startIndex, targetColumn)) {
                PlayInvalidMove();
                RefreshTableLayout(); // Resets positions if move rejected by rules
            }
        }

        private void HandleFoundationDrop(List<CardData> cards, int suitIndex) {
            if (!_isGameModeActive) return;
            if (cards == null || cards.Count == 0) {
                PlayInvalidMove();
                RefreshTableLayout();
                return;
            }

            if (!TryMoveToFoundation(cards, suitIndex)) {
                PlayInvalidMove();
                RefreshTableLayout(); // Resets positions if move rejected by rules
            }
        }

        public bool TryMoveToTableau(List<CardData> cards, int sourceColumn, int startIndex, int targetColumn) {
            if (cards == null || cards.Count == 0) return false;
            if (sourceColumn == targetColumn) return false;

            var landingCard = cards[0];
            if (_engine.CanPlaceOnTableau(landingCard, targetColumn)) {
                _engine.MoveCardsToTableau(cards, sourceColumn, startIndex, targetColumn);
                PlayCardDrop();
                RefreshTableLayout();
                CheckVictoryCondition();
                return true;
            }
            return false;
        }

        public bool TryMoveToFoundation(List<CardData> cards, int suitIndex) {
            if (cards == null || cards.Count == 0) return false;

            var topCard = cards[^1];
            if (_engine.CanPlaceOnFoundation(topCard, suitIndex)) {
                _engine.MoveCardsToFoundation(cards, suitIndex);
                PlayCardDrop();
                RefreshTableLayout();
                CheckVictoryCondition();
                return true;
            }
            return false;
        }

        private void CheckVictoryCondition() {
            if (_engine.HasWon()) {
                PlayVictorySound();
                Debug.Log("[Solitaire] Game Won!");
                if (_currentWager > 0 && _economyService != null) {
                    int payout = _currentWager * 5;
                    _economyService.CreditGold(payout);
                }

                _uiView?.DisplayOutcome("SOLITAIRE CLEARED!");
            }
        }

        #endregion

    }
}