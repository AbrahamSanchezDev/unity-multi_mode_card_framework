using System;
using UnityEngine;
using VContainer.Unity;
using CardFramework.Core.Engines;
using CardFramework.Core.Interfaces;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Controllers {
    /// <summary>
    /// Pure C# Architecture Controller driving Blackjack UI events, cloud economy wagers, 
    /// and modal state transitions into the Core Simulation Engine.
    /// </summary>
    public class BlackjackTableController : IStartable, IDisposable {
        private readonly BlackjackEngine _gameEngine;
        private readonly IBlackjackView _uiView;
        private readonly IEconomyService _economyService;
        private readonly BettingModalView _bettingModalView;

        private int _currentActiveWager = 0;

        public BlackjackTableController(
            BlackjackEngine gameEngine,
            IBlackjackView uiView,
            IEconomyService economyService,
            BettingModalView bettingModalView) {
            _gameEngine = gameEngine;
            _uiView = uiView;
            _economyService = economyService;
            _bettingModalView = bettingModalView;
        }

        public void Start() {
            // Bind UI User Interactions to Controller Logic
            _uiView.OnHitRequested += HandleHit;
            _uiView.OnStandRequested += HandleStand;
            _uiView.OnRestartRequested += HandleRestart;

            // Bind Cloud Betting Modal Confirmation
            _bettingModalView.OnBetConfirmed += HandleWagerConfirmed;

            // Subscribe to server balance changes to update the table HUD immediately
            _economyService.OnBalanceUpdated += HandleWalletBalanceChanged;

            // Initialize the UI with the cached starting balance right away
            _uiView.UpdateWalletBalance(_economyService.CurrentGold);

            // Trigger the initial match setup by requesting a wager first
            HandleRestart();
        }

        public void Dispose() {
            // Unsubscribe to mitigate memory leaks upon lifecycle destruction
            _uiView.OnHitRequested -= HandleHit;
            _uiView.OnStandRequested -= HandleStand;
            _uiView.OnRestartRequested -= HandleRestart;

            _bettingModalView.OnBetConfirmed -= HandleWagerConfirmed;

            // Clean up the event hook to protect memory management pipelines
            if (_economyService != null) {
                _economyService.OnBalanceUpdated -= HandleWalletBalanceChanged;
            }
        }

        private void HandleRestart() {
            // Intercept standard auto-deal layout loops to request a server-validated bet first
            _currentActiveWager = 0;
            _uiView.SetInteractionState(false);
            _uiView.ClearTable();

            _bettingModalView.ShowModal();
        }

        private void HandleWalletBalanceChanged(int freshBalance) {
            Debug.Log($"[Wallet Sync] Pushing updated balance to main HUD layout: {freshBalance} GD");
            _uiView.UpdateWalletBalance(freshBalance);
        }

        private void HandleWagerConfirmed(int confirmedBet) {
            _currentActiveWager = confirmedBet;
            Debug.Log($"[Match Flow] Wager verified: {_currentActiveWager} GD. Processing cloud debit transaction...");

            // Authoritative server side balance debit via PlayFab pipeline
            _economyService.DebitGold(_currentActiveWager);

            // Once economy state is locked on the cloud, proceed to spawn physical game assets
            InitializeTable();
        }

        private void InitializeTable() {
            _gameEngine.ResetEngineState();
            _gameEngine.DealInitialHands();

            _uiView.ClearTable();

            // Spawn initial 3D cards in standard casino order (Player, Dealer, Player, Dealer)
            var playerHand = _gameEngine.GetPlayerHand();
            var dealerHand = _gameEngine.GetDealerHand();

            if (playerHand.Cards.Count >= 2 && dealerHand.Cards.Count >= 2) {
                _uiView.SpawnPhysicalCard(playerHand.Cards[0], true);
                _uiView.SpawnPhysicalCard(dealerHand.Cards[0], false);
                _uiView.SpawnPhysicalCard(playerHand.Cards[1], true);
                _uiView.SpawnPhysicalCard(dealerHand.Cards[1], false);
            }

            _uiView.UpdatePlayerScore(_gameEngine.GetPlayerValue());
            _uiView.UpdateDealerScore(_gameEngine.GetDealerValue());

            // Check if initial hands instantly yielded a natural Blackjack
            if (_gameEngine.CurrentState == BlackjackEngine.GameState.Showdown) {
                _uiView.SetInteractionState(false);
                EvaluateMatchOutcome();
            }
            else {
                _uiView.SetInteractionState(true);
            }
        }

        private void HandleHit() {
            _gameEngine.PlayerHit();

            // Get the last drawn card and spawn its physical 3D counterpart
            var playerHand = _gameEngine.GetPlayerHand();
            if (playerHand.Cards.Count > 0) {
                _uiView.SpawnPhysicalCard(playerHand.Cards[^1], true);
            }

            _uiView.UpdatePlayerScore(_gameEngine.GetPlayerValue());

            if (_gameEngine.CurrentState == BlackjackEngine.GameState.PlayerBust) {
                _uiView.SetInteractionState(false);
                _uiView.DisplayWinner("Dealer (Player Busted)");

                // Player busted: Wager is permanently lost, no cloud credits needed
                Debug.Log($"[Match Flow] Player busted. Lost: {_currentActiveWager} GD.");
            }
        }

        private void HandleStand() {
            _uiView.SetInteractionState(false);

            // Cache the current dealer card count before dealer AI acts
            int existingDealerCards = _gameEngine.GetDealerHand().Cards.Count;

            _gameEngine.PlayerStand();

            // Spawn any new cards the Dealer AI drew during its execution loop
            var dealerHand = _gameEngine.GetDealerHand();
            for (int i = existingDealerCards; i < dealerHand.Cards.Count; i++) {
                _uiView.SpawnPhysicalCard(dealerHand.Cards[i], false);
            }

            _uiView.UpdateDealerScore(_gameEngine.GetDealerValue());
            EvaluateMatchOutcome();
        }

        private void EvaluateMatchOutcome() {
            int pValue = _gameEngine.GetPlayerValue();
            int dValue = _gameEngine.GetDealerValue();

            // Check architectural states to reward or forfeit gold back to Microsoft PlayFab
            if (_gameEngine.CurrentState == BlackjackEngine.GameState.DealerBust) {
                _uiView.DisplayWinner("Player Wins! (Dealer Busted)");

                int payout = _currentActiveWager * 2;
                Debug.Log($"[Match Flow] Dealer busted. Standard payout credited: {payout} GD");
                _economyService.CreditGold(payout);
            }
            else if (pValue > dValue) {
                // Determine if win was achieved via a Natural 2-card 21 Blackjack (Pays 3:2 -> 2.5x total payout)
                var playerHand = _gameEngine.GetPlayerHand();
                if (pValue == 21 && playerHand.Cards.Count == 2) {
                    _uiView.DisplayWinner("Natural Blackjack!");
                    int payout = Mathf.FloorToInt(_currentActiveWager * 2.5f);
                    Debug.Log($"[Match Flow] Natural Blackjack! Premium payout credited: {payout} GD");
                    _economyService.CreditGold(payout);
                }
                else {
                    _uiView.DisplayWinner("Player Wins!");
                    int payout = _currentActiveWager * 2;
                    Debug.Log($"[Match Flow] Standard Win. Payout credited: {payout} GD");
                    _economyService.CreditGold(payout);
                }
            }
            else if (dValue > pValue) {
                _uiView.DisplayWinner("Dealer Wins!");
                Debug.Log($"[Match Flow] Dealer won. Lost: {_currentActiveWager} GD.");
            }
            else {
                _uiView.DisplayWinner("Push (Tie Game)");
                // Tie Game: Return original wager straight back to cloud inventory balance
                Debug.Log($"[Match Flow] Tie match detected. Returning original wager: {_currentActiveWager} GD");
                _economyService.CreditGold(_currentActiveWager);
            }
        }
    }
}