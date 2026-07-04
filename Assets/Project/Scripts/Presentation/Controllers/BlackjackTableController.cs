using System;
using VContainer.Unity;
using CardFramework.Core.Engines;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Controllers {
    /// <summary>
    /// Pure C# Architecture Controller driving Blackjack UI events into the Core Simulation Engine.
    /// </summary>
    public class BlackjackTableController : IStartable, IDisposable {
        private readonly BlackjackEngine _gameEngine;
        private readonly IBlackjackView _uiView;

        public BlackjackTableController(BlackjackEngine gameEngine, IBlackjackView uiView) {
            _gameEngine = gameEngine;
            _uiView = uiView;
        }

        public void Start() {
            // Bind UI User Interactions to Controller Logic
            _uiView.OnHitRequested += HandleHit;
            _uiView.OnStandRequested += HandleStand;
            _uiView.OnRestartRequested += HandleRestart;

            // Trigger the initial match setup
            HandleRestart();
        }

        public void Dispose() {
            // Unsubscribe to mitigate memory leaks upon lifecycle destruction
            _uiView.OnHitRequested -= HandleHit;
            _uiView.OnStandRequested -= HandleStand;
            _uiView.OnRestartRequested -= HandleRestart;
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
            _uiView.SetInteractionState(true);
            // Check if initial hands instantly yielded a natural Blackjack
            if (_gameEngine.CurrentState == BlackjackEngine.GameState.Showdown) {
                EvaluateMatchOutcome();
            }
        }

        private void HandleHit() {
            _gameEngine.PlayerHit();

            // Get the last drawn card and spawn its physical 3D counterpart
            var playerHand = _gameEngine.GetPlayerHand();
            if (playerHand.Cards.Count > 0) {
                _uiView.SpawnPhysicalCard(playerHand.Cards[^1], true); // Using C# hat operator for last item
            }

            _uiView.UpdatePlayerScore(_gameEngine.GetPlayerValue());

            if (_gameEngine.CurrentState == BlackjackEngine.GameState.PlayerBust) {
                _uiView.SetInteractionState(false);
                _uiView.DisplayWinner("Dealer (Player Busted)");
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

        private void HandleRestart() {
            InitializeTable();
        }

        private void EvaluateMatchOutcome() {
            int pValue = _gameEngine.GetPlayerValue();
            int dValue = _gameEngine.GetDealerValue();

            if (_gameEngine.CurrentState == BlackjackEngine.GameState.DealerBust) {
                _uiView.DisplayWinner("Player (Dealer Busted)");
            }
            else if (pValue > dValue) {
                _uiView.DisplayWinner("Player Wins!");
            }
            else if (dValue > pValue) {
                _uiView.DisplayWinner("Dealer Wins!");
            }
            else {
                _uiView.DisplayWinner("Push (Tie Game)");
            }
        }
    }
}