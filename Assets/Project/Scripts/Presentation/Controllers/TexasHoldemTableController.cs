using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;
using CardFramework.Core.Engines;
using CardFramework.Core.Interfaces;
using CardFramework.Core.Models;
using CardFramework.Core.Utils;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;
using CardFramework.Presentation;

namespace CardFramework.Presentation.Controllers {
    public class TexasHoldemTableController : IStartable, IDisposable {
        private readonly TexasHoldemEngine _engine;
        private readonly ITexasHoldemView _uiView;
        private readonly IEconomyService _economyService;
        private readonly IModalService _modalService;
        private readonly BettingModalView _bettingModalView;
        private readonly NavigationController _navigationController;
        private CurrencyDisplayHelper _currencyDisplayHelper;

        private bool _isTexasHoldemActive;
        private int _currentWager;

        public TexasHoldemTableController(
            TexasHoldemEngine engine,
            ITexasHoldemView uiView,
            IEconomyService economyService,
            IModalService modalService,
            BettingModalView bettingModalView,
            NavigationController navigationController) {
            _engine = engine;
            _uiView = uiView;
            _economyService = economyService;
            _modalService = modalService;
            _bettingModalView = bettingModalView;
            _navigationController = navigationController;
        }

        public void Start() {
            UnsubscribeEvents();

            if (_bettingModalView != null) {
                _bettingModalView.OnBetConfirmed += HandleWagerConfirmed;
            }

            if (_uiView != null) {
                _uiView.OnDealRequested += HandleDealRequested;
                _uiView.OnRestartRequested += RequestNewHand;
                _uiView.OnFoldRequested += HandleFoldRequested;
                _uiView.OnMenuRequested += HandleMenuToggleRequested;
            }

            if (_navigationController != null) {
                _navigationController.OnSwitchGameCompleted += HandleGameSwitchCompleted;
            }

            _currencyDisplayHelper?.Dispose();
            _currencyDisplayHelper = new CurrencyDisplayHelper(_economyService, HandleWalletBalanceChanged);

            _uiView?.UpdateWalletBalance(_economyService?.CurrentGold ?? 0);
            _uiView?.SetInteractionState(false);
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
                _uiView.OnDealRequested -= HandleDealRequested;
                _uiView.OnRestartRequested -= RequestNewHand;
                _uiView.OnFoldRequested -= HandleFoldRequested;
                _uiView.OnMenuRequested -= HandleMenuToggleRequested;
            }

            if (_navigationController != null) {
                _navigationController.OnSwitchGameCompleted -= HandleGameSwitchCompleted;
            }
        }

        private void HandleGameSwitchCompleted(string targetGameKey) {
            _isTexasHoldemActive = targetGameKey.Equals("TexasHoldem", StringComparison.OrdinalIgnoreCase);
            _uiView?.ShowUi(_isTexasHoldemActive);

            if (_isTexasHoldemActive) {
                BeginNewHandSequence();
            }
            else {
                _uiView?.ClearTable();
                _uiView?.SetInteractionState(false);
            }
        }

        private void BeginNewHandSequence() {
            _uiView?.ClearTable();
            _uiView?.SetInteractionState(false);
            _bettingModalView?.ShowModal();
        }

        private void HandleMenuToggleRequested() {
            _navigationController?.OpenMenu("PlayFab Synced Profile");
        }

        private void RequestNewHand() {
            if (!_isTexasHoldemActive) return;
            BeginNewHandSequence();
        }

        private void HandleWagerConfirmed(int confirmedWager) {
            if (!_isTexasHoldemActive) return;

            _currentWager = confirmedWager;
            _economyService?.DebitGold(_currentWager);

            _engine.StartNewHand();
            RefreshView();
            _uiView?.SetInteractionState(true);
        }

        private void HandleDealRequested() {
            if (!_isTexasHoldemActive) return;
            if (_engine.CurrentRound == TexasHoldemEngine.RoundState.Showdown) {
                _engine.StartNewHand();
                RefreshView();
                _uiView?.SetInteractionState(true);
                return;
            }

            _engine.AdvanceRound();
            RefreshView();

            if (_engine.CurrentRound == TexasHoldemEngine.RoundState.Showdown) {
                EvaluateAndReportBestHand();
                _uiView?.SetInteractionState(false);
            }
        }

        private void HandleFoldRequested() {
            if (!_isTexasHoldemActive) return;
            _uiView?.DisplayOutcome("Folded. New hand ready.");
            _uiView?.SetInteractionState(false);
        }

        private void HandleWalletBalanceChanged(int newBalance) {
            _uiView?.UpdateWalletBalance(newBalance);
        }

        private void RefreshView() {
            _uiView?.ClearTable();
            _uiView?.RenderRoundState(_engine.CurrentRound, _engine.PlayerHand, _engine.CommunityCards);

            foreach (var card in _engine.PlayerHand) {
                _uiView?.SpawnPhysicalCard(card, true);
            }

            foreach (var card in _engine.CommunityCards) {
                _uiView?.SpawnPhysicalCard(card, false);
            }
        }

        private void EvaluateAndReportBestHand() {
            var combinedCards = new List<CardData>(_engine.PlayerHand);
            combinedCards.AddRange(_engine.CommunityCards);

            var bestHand = new List<CardData>();
            var bestRank = HandRank.HighCard;

            for (int i = 0; i < combinedCards.Count - 4; i++) {
                for (int j = i + 1; j < combinedCards.Count - 3; j++) {
                    for (int k = j + 1; k < combinedCards.Count - 2; k++) {
                        for (int l = k + 1; l < combinedCards.Count - 1; l++) {
                            for (int m = l + 1; m < combinedCards.Count; m++) {
                                var candidate = new List<CardData> {
                                    combinedCards[i], combinedCards[j], combinedCards[k], combinedCards[l], combinedCards[m]
                                };

                                var candidateRank = HandEvaluator.EvaluateFiveCardHand(candidate);
                                if ((int)candidateRank > (int)bestRank) {
                                    bestRank = candidateRank;
                                    bestHand = candidate;
                                }
                            }
                        }
                    }
                }
            }

            if (_currentWager > 0 && _economyService != null) {
                int payout = Mathf.FloorToInt(_currentWager * 1.5f);
                _economyService.CreditGold(payout);
            }

            _uiView?.DisplayOutcome($"Best Hand: {FormatRank(bestRank)}");
        }

        private static string FormatRank(HandRank handRank) {
            return handRank switch {
                HandRank.RoyalFlush => "Royal Flush",
                HandRank.StraightFlush => "Straight Flush",
                HandRank.FourOfAKind => "Four of a Kind",
                HandRank.FullHouse => "Full House",
                HandRank.Flush => "Flush",
                HandRank.Straight => "Straight",
                HandRank.ThreeOfAKind => "Three of a Kind",
                HandRank.TwoPair => "Two Pair",
                HandRank.OnePair => "One Pair",
                _ => "High Card"
            };
        }
    }
}
