using System;
using UnityEngine;
using VContainer.Unity;
using CardFramework.Core.Engines;
using CardFramework.Core.Interfaces;
using CardFramework.Core.Models;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Controllers {
    public class SolitaireTableController : IStartable, IDisposable {
        private readonly SolitaireEngine _engine;
        private readonly ISolitaireView _solitaireView;
        private readonly IEconomyService _economyService;
        private readonly BettingModalView _bettingModalView;
        private readonly NavigationController _navigationController;

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
            _solitaireView = solitaireView;
            _economyService = economyService;
            _bettingModalView = bettingModalView;
            _navigationController = navigationController;
        }

        public void Start() {
            UnsubscribeEvents();

            if (_bettingModalView != null) {
                _bettingModalView.OnBetConfirmed += HandleWagerConfirmed;
            }

            if (_economyService != null) {
                _economyService.OnBalanceUpdated += HandleBalanceUpdated;
                _solitaireView?.UpdateWalletBalance(_economyService.CurrentGold);
            }

            if (_solitaireView != null) {
                _solitaireView.OnStockTapped += HandleStockTapped;
                _solitaireView.OnRestartRequested += RequestNewGame;
                _solitaireView.OnTableauDropRequested += HandleTableauDrop;
                _solitaireView.OnFoundationDropRequested += HandleFoundationDrop;
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

            if (_economyService != null) {
                _economyService.OnBalanceUpdated -= HandleBalanceUpdated;
            }

            if (_solitaireView != null) {
                _solitaireView.OnStockTapped -= HandleStockTapped;
                _solitaireView.OnRestartRequested -= RequestNewGame;
                _solitaireView.OnTableauDropRequested -= HandleTableauDrop;
                _solitaireView.OnFoundationDropRequested -= HandleFoundationDrop;
            }

            if (_navigationController != null) {
                _navigationController.OnSwitchGameCompleted -= HandleGameSwitchCompleted;
            }
        }

        private void HandleGameSwitchCompleted(string targetGameKey) {
            _isSolitaireActive = targetGameKey.Equals("Solitaire", StringComparison.OrdinalIgnoreCase);

            if (_isSolitaireActive) {
                Debug.Log("[Solitaire Controller] Solitaire mode activated.");
                RequestNewGame();
            }
            else {
                _solitaireView?.ClearTable();
            }
        }

        public void RequestNewGame() {
            if (!_isSolitaireActive) return;

            _currentWager = 0;
            _solitaireView?.ClearTable();
            _bettingModalView?.ShowModalWithCap(minBet: 0, maxBet: MaxSolitaireWager);
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
            _solitaireView?.RenderLayout(
                _engine.GetTableau(),
                _engine.GetFoundation(),
                // Note: Expose stock and waste getters in SolitaireEngine if not already available
                _engine.GetStock(),
                _engine.GetWaste()
            );
        }

        private void HandleBalanceUpdated(int newBalance) {
            if (_isSolitaireActive) {
                _solitaireView?.UpdateWalletBalance(newBalance);
            }
        }

        private void HandleStockTapped() {
            if (!_isSolitaireActive) return;

            _engine.DrawCard();
            RefreshTableLayout();
        }

        private void HandleTableauDrop(CardData card, int targetColumn) {
            if (!_isSolitaireActive) return;
            TryMoveToTableau(card, targetColumn);
        }

        private void HandleFoundationDrop(CardData card, int suitIndex) {
            if (!_isSolitaireActive) return;
            TryMoveToFoundation(card, suitIndex);
        }

        public bool TryMoveToTableau(CardData card, int targetColumn) {
            if (_engine.CanPlaceOnTableau(card, targetColumn)) {
                _engine.GetTableau()[targetColumn].Add(card);
                CheckVictoryCondition();
                return true;
            }
            return false;
        }

        public bool TryMoveToFoundation(CardData card, int suitIndex) {
            if (_engine.CanPlaceOnFoundation(card, suitIndex)) {
                _engine.GetFoundation()[suitIndex].Add(card);
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
            }
        }
    }
}