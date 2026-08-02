using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using CardFramework.Core.Engines;
using CardFramework.Core.Interfaces;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;
using CardFramework.Presentation;

namespace CardFramework.Presentation.Controllers {
    /// <summary>
    /// Pure C# Architecture Controller driving Blackjack UI events, cloud economy wagers, 
    /// and modal state transitions into the Core Simulation Engine.
    /// </summary>
    public class BlackjackTableController : BaseTableController {
        private readonly BlackjackEngine _gameEngine;
        private readonly IBlackjackView _uiView;
        private readonly Queue<int> _pendingGoldCredits = new Queue<int>();
        private bool _isGoldCreditQueueProcessing;

        public override int MaxWager => MaxBlackjackWager;
        public const int MaxBlackjackWager = 0; // Unlimited for Blackjack, but can be capped by PlayFab Economy Service

        private bool _canInteractHitOrStand;

        public BlackjackTableController(
            BlackjackEngine gameEngine,
            IBlackjackView uiView,
            IEconomyService economyService,
            IModalService modalService,
            BettingModalView bettingModalView,
            NavigationController navigationController,
            IAudioService audioService) {
            _gameEngine = gameEngine;
            _uiView = uiView;
            _economyService = economyService;
            _modalService = modalService;
            _bettingModalView = bettingModalView;
            _navigationController = navigationController;
            _audioService = audioService;
        }

        public override void Start() {
            base.Start();
            // Bind UI User Interactions to Controller Logic
            _uiView.OnHitRequested += HandleHit;
            _uiView.OnStandRequested += HandleStand;
            _uiView.OnRestartRequested += RequestNewGame;

            // Initialize the UI with the cached starting balance right away
            _uiView.UpdateWalletBalance(_economyService.CurrentGold);

            // Subscribe to the event for when the menu gets open
            _uiView.OnMenuRequested += HandleMenuToggleRequested;

            // Trigger the initial match setup by requesting a wager first
            //HandleRestart();
        }

        public override void Dispose() {
            base.Dispose();
            // Unsubscribe to mitigate memory leaks upon lifecycle destruction
            _uiView.OnHitRequested -= HandleHit;
            _uiView.OnStandRequested -= HandleStand;
            _uiView.OnRestartRequested -= RequestNewGame;

            _bettingModalView.OnBetConfirmed -= HandleWagerConfirmed;

            _currencyDisplayHelper?.Dispose();
            _currencyDisplayHelper = null;

            // Clean up navigation listeners
            _navigationController.OnMenuOpened -= HandleMenuOpened;
            _navigationController.OnMenuClosed -= HandleMenuClosed;
            _navigationController.OnSwitchGameCompleted -= HandleGameSwitchCompleted;
        }

        protected override string GetDescriptionOnNewGameText() {
            return "This will discard the current Blackjack hand. Start a new round?";
        }

        protected override void BeginNewGame() {
            base.BeginNewGame();
            SetInteractionState(false);
            _uiView.ClearTable();
            _canInteractHitOrStand = true;
        }

        protected override void HandleMenuOpened() {
            SetInteractionState(false);
        }

        protected override void HandleMenuClosed() {
            // Only restore interaction state if the game is currently active and not awaiting a wager confirmation
            switch (_gameEngine.CurrentState) {
                case BlackjackEngine.GameState.PlayerTurn:
                case BlackjackEngine.GameState.DealerTurn:
                    SetInteractionState(true);
                    break;
                case BlackjackEngine.GameState.PlayerBust:
                case BlackjackEngine.GameState.DealerBust:
                case BlackjackEngine.GameState.Showdown:
                case BlackjackEngine.GameState.GameOver:
                    SetInteractionState(false);
                    break;
            }
        }

        private void SetInteractionState(bool isEnabled) {
            _uiView.SetInteractionState(isEnabled);
            _canInteractHitOrStand = isEnabled;
        }

        protected override void UpdateWalletBalance(int freshBalance) {
            _uiView.UpdateWalletBalance(freshBalance);
        }

        protected override void InitializeEngine() {
            base.InitializeEngine();
            // Once economy state is locked on the cloud, proceed to spawn physical game assets
            InitializeTable();
        }

        override protected string GetGameModeKey() {
            return "Blackjack";
        }
        override protected void OnGameModeActivated() {
            base.OnGameModeActivated();
            _uiView.ShowUi(true);
        }

        protected override void OnGameModeDeactivated() {
            base.OnGameModeDeactivated();
            _uiView.ClearTable();
            SetInteractionState(false);
        }

        override public void ShowUI(bool show) {
            _uiView?.ShowUi(show);
        }

        #region Blackjack Specific Handlers
        private void InitializeTable() {
            _gameEngine.ResetEngineState();
            _gameEngine.DealInitialHands();

            _uiView.ClearTable();

            // Spawn initial 3D cards in standard casino order (Player, Dealer, Player, Dealer)
            var playerHand = _gameEngine.GetPlayerHand();
            var dealerHand = _gameEngine.GetDealerHand();

            if (playerHand.Cards.Count >= 2 && dealerHand.Cards.Count >= 2) {
                _uiView.SpawnPhysicalCard(playerHand.Cards[0], true);
                PlayCardDrop();
                _uiView.SpawnPhysicalCard(dealerHand.Cards[0], false);
                PlayCardDrop();
                _uiView.SpawnPhysicalCard(playerHand.Cards[1], true);
                PlayCardDrop();
                _uiView.SpawnPhysicalCard(dealerHand.Cards[1], false);
                PlayCardDrop();
            }

            _uiView.UpdatePlayerScore(_gameEngine.GetPlayerValue());
            _uiView.UpdateDealerScore(_gameEngine.GetDealerValue());

            // Check if initial hands instantly yielded a natural Blackjack
            if (_gameEngine.CurrentState == BlackjackEngine.GameState.Showdown) {
                SetInteractionState(false);
                EvaluateMatchOutcome(true);
            }
            else {
                SetInteractionState(true);
            }
        }

        private void HandleHit() {
            _gameEngine.PlayerHit();

            // Get the last drawn card and spawn its physical 3D counterpart
            var playerHand = _gameEngine.GetPlayerHand();
            if (playerHand.Cards.Count > 0) {
                _uiView.SpawnPhysicalCard(playerHand.Cards[^1], true);
                PlayCardDrop();
            }

            _uiView.UpdatePlayerScore(_gameEngine.GetPlayerValue());

            if (_gameEngine.CurrentState == BlackjackEngine.GameState.PlayerBust) {
                PlayInvalidMove();
                SetInteractionState(false);
                _uiView.DisplayWinner("Dealer (Player Busted)");

                // Player busted: Wager is permanently lost, no cloud credits needed
                Debug.Log($"[Match Flow] Player busted. Lost: {_currentWager} GD.");
            }
        }

        private void HandleStand() {
            SetInteractionState(false);

            // Cache the current dealer card count before dealer AI acts
            int existingDealerCards = _gameEngine.GetDealerHand().Cards.Count;

            _gameEngine.PlayerStand();

            // Spawn any new cards the Dealer AI drew during its execution loop
            var dealerHand = _gameEngine.GetDealerHand();
            for (int i = existingDealerCards; i < dealerHand.Cards.Count; i++) {
                _uiView.SpawnPhysicalCard(dealerHand.Cards[i], false);
                PlayCardDrop();
            }

            _uiView.UpdateDealerScore(_gameEngine.GetDealerValue());
            EvaluateMatchOutcome();
        }

        private void EvaluateMatchOutcome(bool firstEvaluation = false) {
            int pValue = _gameEngine.GetPlayerValue();
            int dValue = _gameEngine.GetDealerValue();

            // Check architectural states to reward or forfeit gold back to Microsoft PlayFab
            if (_gameEngine.CurrentState == BlackjackEngine.GameState.DealerBust) {
                _uiView.DisplayWinner("Player Wins! (Dealer Busted)");

                int payout = _currentWager * 2;
                Debug.Log($"[Match Flow] Dealer busted. Standard payout credited: {payout} GD");
                CreditGold(payout, firstEvaluation);
            }
            else if (pValue > dValue) {
                // Determine if win was achieved via a Natural 2-card 21 Blackjack (Pays 3:2 -> 2.5x total payout)
                var playerHand = _gameEngine.GetPlayerHand();
                if (pValue == 21 && playerHand.Cards.Count == 2) {
                    _uiView.DisplayWinner("Natural Blackjack!");
                    int payout = Mathf.FloorToInt(_currentWager * 2.5f);
                    Debug.Log($"[Match Flow] Natural Blackjack! Premium payout credited: {payout} GD");

                    CreditGold(payout, firstEvaluation);
                }
                else {
                    _uiView.DisplayWinner("Player Wins!");
                    int payout = _currentWager * 2;
                    Debug.Log($"[Match Flow] Standard Win. Payout credited: {payout} GD");
                    CreditGold(payout, firstEvaluation);
                }
            }
            else if (dValue > pValue) {
                _uiView.DisplayWinner("Dealer Wins!");
                Debug.Log($"[Match Flow] Dealer won. Lost: {_currentWager} GD.");
            }
            else {
                _uiView.DisplayWinner("Push (Tie Game)");
                // Tie Game: Return original wager straight back to cloud inventory balance
                Debug.Log($"[Match Flow] Tie match detected. Returning original wager: {_currentWager} GD");
                CreditGold(_currentWager, firstEvaluation);
            }
        }

        private void QueueDelayedGoldCredit(int amount) {
            _pendingGoldCredits.Enqueue(amount);
            if (_isGoldCreditQueueProcessing) {
                return;
            }

            _ = ProcessQueuedGoldCreditsAsync();
        }

        private async Task ProcessQueuedGoldCreditsAsync() {
            _isGoldCreditQueueProcessing = true;

            try {
                while (_pendingGoldCredits.Count > 0) {
                    int nextPayout = _pendingGoldCredits.Dequeue();
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    CreditGold(nextPayout);
                }
            }
            finally {
                _isGoldCreditQueueProcessing = false;
            }
        }

        private void CreditGold(int amount, bool delay = false) {
            if (delay) {
                QueueDelayedGoldCredit(amount);
                return;
            }
            else
                _economyService.CreditGold(amount);
        }
        #endregion

    }
}