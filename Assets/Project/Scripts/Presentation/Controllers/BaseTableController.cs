using System;
using UnityEngine;
using VContainer.Unity;
using CardFramework.Core.Interfaces;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Controllers {

    public class BaseTableController : IStartable, IDisposable {
        protected IEconomyService _economyService;
        protected IModalService _modalService;
        protected BettingModalView _bettingModalView;
        protected NavigationController _navigationController;
        protected IAudioService _audioService;
        protected CurrencyDisplayHelper _currencyDisplayHelper;

        // Need to override this in inheriting classes for specific game rules
        public virtual int MaxWager => 50;
        protected virtual int MinWager => 0;

        protected int _currentWager = 0;
        protected bool _isGameModeActive = false;

        protected bool playButtonsAreActive = false;


        public virtual void Start() {
            UnsubscribeEvents();

            if (_bettingModalView != null) {
                _bettingModalView.OnBetConfirmed += HandleWagerConfirmed;
            }

            if (_navigationController != null) {
                _navigationController.OnSwitchGameCompleted += HandleGameSwitchCompleted;

                // Listen to Navigation lifecycle changes to freeze/unfreeze interactions
                _navigationController.OnMenuOpened += HandleMenuOpened;
                _navigationController.OnMenuClosed += HandleMenuClosed;
            }


            _currencyDisplayHelper?.Dispose();
            _currencyDisplayHelper = new CurrencyDisplayHelper(_economyService, HandleBalanceUpdated);
        }

        public virtual void Dispose() {
            UnsubscribeEvents();
        }

        protected virtual void UnsubscribeEvents() {
            if (_bettingModalView != null) {
                _bettingModalView.OnBetConfirmed -= HandleWagerConfirmed;
            }

            _currencyDisplayHelper?.Dispose();
            _currencyDisplayHelper = null;

            if (_navigationController != null) {
                _navigationController.OnSwitchGameCompleted -= HandleGameSwitchCompleted;
            }
        }


        protected virtual string GetGameModeKey() {
            return "BaseGame";
        }
        protected void HandleGameSwitchCompleted(string targetGameKey) {
            _isGameModeActive = targetGameKey.Equals(GetGameModeKey(), StringComparison.OrdinalIgnoreCase);
            ShowUI(_isGameModeActive);
            if (_isGameModeActive) {
                // Debug.Log($"[{GetGameModeKey()} Controller] {GetGameModeKey()} mode activated.");
                OnGameModeActivated();
            }
            else {
                OnGameModeDeactivated();
            }
        }

        protected virtual void HandleMenuOpened() {
        }

        protected virtual void HandleMenuClosed() {
        }

        protected virtual void OnGameModeActivated() {
            playButtonsAreActive = true;
            _isGameModeActive = true;
            BeginNewGame();
        }

        protected virtual void OnGameModeDeactivated() {
            playButtonsAreActive = false;
            _isGameModeActive = false;
        }

        public virtual void ShowUI(bool show) {
            // _uiView?.ShowUi(show);
        }

        protected virtual string GetStartOnNewGameText() {
            return "Start New Game?";
        }

        protected virtual string GetDescriptionOnNewGameText() {
            return "This will discard your current progress. Start a new round?";
        }

        public virtual void RequestNewGame() {
            if (!_isGameModeActive) return;
            if (playButtonsAreActive) {
                _modalService?.ShowConfirmation(
                GetStartOnNewGameText(),
                GetDescriptionOnNewGameText(),
                BeginNewGame,
                OnCancelNewGame
                );
            }
            else {

                BeginNewGame();
            }

        }

        protected virtual void OnCancelNewGame() {
            // Debug.Log("[Solitaire Controller] New game request canceled by user.");
        }

        protected virtual void BeginNewGame() {
            _currentWager = 0;
            PlayOnNewGameSound();
            if (MaxWager <= 0) {
                _bettingModalView?.ShowModal();
            }
            else {
                _bettingModalView?.ShowModalWithCap(minBet: MinWager, maxBet: MaxWager);
            }
            HandleBalanceUpdated(_economyService?.CurrentGold ?? 0);
        }

        protected virtual void HandleWagerConfirmed(int selectedWager) {
            if (!_isGameModeActive) return;
            var theFinalMax = MaxWager;
            if (theFinalMax <= 0) {
                theFinalMax = int.MaxValue;
            }
            _currentWager = Mathf.Clamp(selectedWager, MinWager, theFinalMax);
            if (_currentWager > 0 && _economyService != null) {
                Debug.Log($"Starting game with a wager of {_currentWager} GD.");
                _economyService.DebitGold(_currentWager);
            }
            else {
                Debug.Log("Starting casual game (0 GD bet).");
            }
            InitializeEngine();
            PlayOnBetConfirmedSound();
            // Render starting physical card positions
            RefreshTableLayout();
        }
        protected virtual void InitializeEngine() {
            // This method should be overridden in derived classes to set up the specific game engine.
            // _engine.Initialize();
        }

        protected virtual void RefreshTableLayout() {
        }

        protected virtual void HandleBalanceUpdated(int newBalance) {
            if (_isGameModeActive) {
                UpdateWalletBalance(newBalance);
            }
        }

        protected virtual void UpdateWalletBalance(int newBalance) {
            // _uiView?.UpdateWalletBalance(newBalance);
        }


        protected virtual void HandleMenuToggleRequested() {
            _navigationController.OpenMenu("PlayFab Synced Profile");
        }

        protected virtual void PlayCardDrop() {
            _audioService?.PlayCardDrop();
        }

        protected virtual void PlayCardGrab() {
            _audioService?.PlayCardGrab();
        }

        protected virtual void PlayShuffle() {
            _audioService?.PlayShuffle();
        }

        protected virtual void PlayInvalidMove() {
            _audioService?.PlayInvalidMove();
        }

        protected virtual void PlayVictorySound() {
            _audioService?.PlayVictory();
            _navigationController?.PlayWinVfx(GetGameModeKey());
        }

        protected virtual void PlayOnBetConfirmedSound() {
            _audioService?.PlayGameStart();
        }

        protected virtual void PlayOnNewGameSound() {
            // _audioService?.PlayButtonClick();
            _navigationController?.StopWinVfx();
        }

    }
}