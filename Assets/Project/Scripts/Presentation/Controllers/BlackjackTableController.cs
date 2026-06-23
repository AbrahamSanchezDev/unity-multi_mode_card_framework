using System;
using VContainer.Unity;
using CardFramework.Core.Engines;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Presentation.Controllers
{
    /// <summary>
    /// Pure C# Architecture Controller driving Blackjack UI events into the Core Simulation Engine.
    /// </summary>
    public class BlackjackTableController : IStartable, IDisposable
    {
        private readonly BlackjackEngine _gameEngine;
        private readonly IBlackjackView _uiView;

        public BlackjackTableController(BlackjackEngine gameEngine, IBlackjackView uiView)
        {
            _gameEngine = gameEngine;
            _uiView = uiView;
        }

        public void Start()
        {
            // Bind UI User Interactions to Controller Logic
            _uiView.OnHitRequested += HandleHit;
            _uiView.OnStandRequested += HandleStand;
            _uiView.OnRestartRequested += HandleRestart;

            // Trigger the initial match setup
            HandleRestart();
        }

        public void Dispose()
        {
            // Unsubscribe to mitigate memory leaks upon lifecycle destruction
            _uiView.OnHitRequested -= HandleHit;
            _uiView.OnStandRequested -= HandleStand;
            _uiView.OnRestartRequested -= HandleRestart;
        }

        private void InitializeTable()
        {
            _gameEngine.ResetEngineState();
            _gameEngine.DealInitialHands();

            _uiView.ClearTable();
            _uiView.UpdatePlayerScore(_gameEngine.GetPlayerValue());
            _uiView.UpdateDealerScore(_gameEngine.GetDealerValue());
            _uiView.SetInteractionState(true);

            // Check if initial hands instantly yielded a natural Blackjack
            if (_gameEngine.CurrentState == BlackjackEngine.GameState.Showdown)
            {
                EvaluateMatchOutcome();
            }
        }

        private void HandleHit()
        {
            _gameEngine.PlayerHit();
            _uiView.UpdatePlayerScore(_gameEngine.GetPlayerValue());

            if (_gameEngine.CurrentState == BlackjackEngine.GameState.PlayerBust)
            {
                _uiView.SetInteractionState(false);
                _uiView.DisplayWinner("Dealer (Player Busted)");
            }
        }

        private void HandleStand()
        {
            _uiView.SetInteractionState(false);
            _gameEngine.PlayerStand();
            
            _uiView.UpdateDealerScore(_gameEngine.GetDealerValue());
            EvaluateMatchOutcome();
        }

        private void HandleRestart()
        {
            InitializeTable();
        }

        private void EvaluateMatchOutcome()
        {
            int pValue = _gameEngine.GetPlayerValue();
            int dValue = _gameEngine.GetDealerValue();

            if (_gameEngine.CurrentState == BlackjackEngine.GameState.DealerBust)
            {
                _uiView.DisplayWinner("Player (Dealer Busted)");
            }
            else if (pValue > dValue)
            {
                _uiView.DisplayWinner("Player Wins!");
            }
            else if (dValue > pValue)
            {
                _uiView.DisplayWinner("Dealer Wins!");
            }
            else
            {
                _uiView.DisplayWinner("Push (Tie Game)");
            }
        }
    }
}