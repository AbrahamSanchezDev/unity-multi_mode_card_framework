using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using CardFramework.Core.Engines;
using CardFramework.Core.Interfaces;
using CardFramework.Core.Models;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;
using CardFramework.Presentation;

namespace CardFramework.Presentation.Controllers {
    public class SolitaireTableController : IStartable, IDisposable {
        private readonly SolitaireEngine _engine;
        private readonly ISolitaireView _uiView;
        private readonly IEconomyService _economyService;
        private readonly BettingModalView _bettingModalView;
        private readonly NavigationController _navigationController;
        private CurrencyDisplayHelper _currencyDisplayHelper;

        public const int MaxSolitaireWager = 50;
        private int _currentWager = 0;
        private bool _isSolitaireActive = false;

        public SolitaireTableController(
            SolitaireEngine engine,
            ISolitaireView solitaireView,
            IEconomyService economyService,
            BettingModalView bettingModalView,
            NavigationController navigationController) {
            _engine = engine;
            _uiView = solitaireView;
            _economyService = economyService;
            _bettingModalView = bettingModalView;
            _navigationController = navigationController;

            _currencyDisplayHelper?.Dispose();
            if (_economyService != null) {
                _currencyDisplayHelper = new CurrencyDisplayHelper(_economyService, HandleBalanceUpdated);
            }
            Debug.Log("[Solitaire Controller] Initialized with SolitaireEngine and ISolitaireView.");
        }

        public void Start() {
            UnsubscribeEvents();

            if (_bettingModalView != null) {
                _bettingModalView.OnBetConfirmed += HandleWagerConfirmed;
            }

            if (_uiView != null) {
                _uiView.OnStockTapped += HandleStockTapped;
                _uiView.OnRestartRequested += RequestNewGame;
                _uiView.OnTableauDropRequested += HandleTableauDrop;
                _uiView.OnFoundationDropRequested += HandleFoundationDrop;

                // Subscribe to the event for when the menu gets open
                _uiView.OnMenuRequested += HandleMenuToggleRequested;
            }

            if (_navigationController != null) {
                _navigationController.OnSwitchGameCompleted += HandleGameSwitchCompleted;
            }
        }

        public void Dispose() {
            UnsubscribeEvents();
        }

        private void UnsubscribeEvents() {
            if (_bettingModalView != null) {
                _bettingModalView.OnBetConfirmed -= HandleWagerConfirmed;
            }

            _currencyDisplayHelper?.Dispose();
            _currencyDisplayHelper = null;

            if (_uiView != null) {
                _uiView.OnStockTapped -= HandleStockTapped;
                _uiView.OnRestartRequested -= RequestNewGame;
                _uiView.OnTableauDropRequested -= HandleTableauDrop;
                _uiView.OnFoundationDropRequested -= HandleFoundationDrop;
            }

            if (_navigationController != null) {
                _navigationController.OnSwitchGameCompleted -= HandleGameSwitchCompleted;
            }
        }

        private void HandleGameSwitchCompleted(string targetGameKey) {
            _isSolitaireActive = targetGameKey.Equals("Solitaire", StringComparison.OrdinalIgnoreCase);

            _uiView.ShowUi(_isSolitaireActive);
            if (_isSolitaireActive) {
                Debug.Log("[Solitaire Controller] Solitaire mode activated.");
                RequestNewGame();
            }
            else {
                _uiView?.ClearTable();
            }
        }

        public void RequestNewGame() {
            if (!_isSolitaireActive) return;

            _currentWager = 0;
            _uiView?.ClearTable();
            _bettingModalView?.ShowModalWithCap(minBet: 0, maxBet: MaxSolitaireWager);
            HandleBalanceUpdated(_economyService.CurrentGold);
        }

        private void HandleWagerConfirmed(int selectedWager) {
            if (!_isSolitaireActive) return;

            _currentWager = Mathf.Clamp(selectedWager, 0, MaxSolitaireWager);

            if (_currentWager > 0 && _economyService != null) {
                Debug.Log($"[Solitaire] Starting game with a wager of {_currentWager} GD.");
                _economyService.DebitGold(_currentWager);
            }
            else {
                Debug.Log("[Solitaire] Starting casual game (0 GD bet).");
            }

            _engine.Initialize();

            // Render starting physical card positions
            RefreshTableLayout();
        }

        private void RefreshTableLayout() {
            _uiView?.RenderLayout(
                _engine.GetTableau(),
                _engine.GetFoundation(),
                _engine.GetStock(),
                _engine.GetWaste()
            );

            _uiView?.UpdateFoundationScore(GetFoundationCount(), 52);
            _uiView?.ClearOutcome();
        }

        private void HandleBalanceUpdated(int newBalance) {
            if (_isSolitaireActive) {
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

        private void HandleStockTapped() {
            if (!_isSolitaireActive) return;

            _engine.DrawCard();
            RefreshTableLayout();
        }

        private void HandleTableauDrop(List<CardData> cards, int sourceColumn, int startIndex, int targetColumn) {
            if (!_isSolitaireActive) return;
            if (!TryMoveToTableau(cards, sourceColumn, startIndex, targetColumn)) {
                RefreshTableLayout(); // Resets positions if move rejected by rules
            }
        }

        private void HandleFoundationDrop(List<CardData> cards, int suitIndex) {
            if (!_isSolitaireActive) return;
            if (cards == null || cards.Count == 0) {
                RefreshTableLayout();
                return;
            }

            if (!TryMoveToFoundation(cards, suitIndex)) {
                RefreshTableLayout(); // Resets positions if move rejected by rules
            }
        }

        public bool TryMoveToTableau(List<CardData> cards, int sourceColumn, int startIndex, int targetColumn) {
            if (cards == null || cards.Count == 0) return false;
            if (sourceColumn == targetColumn) return false;

            var landingCard = cards[0];
            if (_engine.CanPlaceOnTableau(landingCard, targetColumn)) {
                _engine.MoveCardsToTableau(cards, sourceColumn, startIndex, targetColumn);
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
                RefreshTableLayout();
                CheckVictoryCondition();
                return true;
            }
            return false;
        }

        private void CheckVictoryCondition() {
            if (_engine.HasWon()) {
                Debug.Log("[Solitaire] Game Won!");
                if (_currentWager > 0 && _economyService != null) {
                    int payout = _currentWager * 5;
                    _economyService.CreditGold(payout);
                }

                _uiView?.DisplayOutcome("SOLITAIRE CLEARED!");
            }
        }

        private void HandleMenuToggleRequested() {
            _navigationController.OpenMenu("PlayFab Synced Profile");
        }
    }
}