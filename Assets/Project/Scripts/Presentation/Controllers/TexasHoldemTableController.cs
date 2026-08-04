using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardFramework.Core.Engines;
using CardFramework.Core.Interfaces;
using CardFramework.Core.Models;
using CardFramework.Core.Utils;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;

namespace CardFramework.Presentation.Controllers {
    public class TexasHoldemTableController : BaseTableController {
        private readonly TexasHoldemEngine _engine;
        private readonly ITexasHoldemView _uiView;

        private int _spawnedPlayerCount;
        private int _spawnedHouseCount;
        private int _spawnedCommunityCount;
        private bool _housePlaceholdersSpawned;

        public override int MaxWager => 50;

        protected override string GetGameModeKey() {
            return "TexasHoldem";
        }

        public TexasHoldemTableController(
            TexasHoldemEngine engine,
            ITexasHoldemView uiView,
            IEconomyService economyService,
            IModalService modalService,
            BettingModalView bettingModalView,
            NavigationController navigationController,
            IAudioService audioService) {
            _engine = engine;
            _uiView = uiView;
            _economyService = economyService;
            _modalService = modalService;
            _bettingModalView = bettingModalView;
            _navigationController = navigationController;
            _audioService = audioService;
        }

        public override void Start() {
            base.Start();

            if (_uiView != null) {
                _uiView.OnDealRequested += HandleDealRequested;
                _uiView.OnRestartRequested += RequestNewGame;
                _uiView.OnFoldRequested += HandleFoldRequested;
                _uiView.OnMenuRequested += HandleMenuToggleRequested;

                _uiView.UpdateWalletBalance(_economyService?.CurrentGold ?? 0);
                _uiView.SetInteractionState(false);
            }
        }

        protected override void UnsubscribeEvents() {
            base.UnsubscribeEvents();
            if (_uiView != null) {
                _uiView.OnDealRequested -= HandleDealRequested;
                _uiView.OnRestartRequested -= RequestNewGame;
                _uiView.OnFoldRequested -= HandleFoldRequested;
                _uiView.OnMenuRequested -= HandleMenuToggleRequested;
            }
        }


        protected override void OnGameModeDeactivated() {
            base.OnGameModeDeactivated();
            _uiView?.ClearTable();
            SetInteractionState(false);
        }
        public override void ShowUI(bool show) {
            _uiView?.ShowUi(show);
        }

        protected override void BeginNewGame() {
            base.BeginNewGame();
            _uiView?.ClearTable();
            SetInteractionState(false);
        }

        protected override void InitializeEngine() {
            _spawnedPlayerCount = 0;
            _spawnedHouseCount = 0;
            _spawnedCommunityCount = 0;
            _housePlaceholdersSpawned = false;
            _housePlaceholdersSpawned = true;
            _engine.StartNewHand();
            _uiView?.ClearTable();
            _uiView?.SpawnHousePlaceholders(_engine.HouseHand.Count);
            _uiView?.SetRestartButtonEnabled(true);
            SetInteractionState(true);
        }

        protected override void RefreshTableLayout() {
            base.RefreshTableLayout();
            RefreshView();
        }
        protected override void UpdateWalletBalance(int newBalance) {
            base.UpdateWalletBalance(newBalance);
            _uiView?.UpdateWalletBalance(newBalance);
        }

        #region Texas Hold'em Specific Handlers
        private void HandleDealRequested() {
            if (!_isGameModeActive) return;
            if (_engine.CurrentRound == TexasHoldemEngine.RoundState.Showdown) {
                _audioService?.PlayShuffle();
                _engine.StartNewHand();
                _spawnedPlayerCount = 0;
                _spawnedHouseCount = 0;
                _spawnedCommunityCount = 0;
                _housePlaceholdersSpawned = false;
                _uiView?.ClearTable();
                _uiView?.SpawnHousePlaceholders(_engine.HouseHand.Count);
                _housePlaceholdersSpawned = true;
                RefreshView();
                _uiView?.SetRestartButtonEnabled(true);
                SetInteractionState(true);
                return;
            }

            _engine.AdvanceRound();
            RefreshView();

            if (_engine.CurrentRound == TexasHoldemEngine.RoundState.Showdown) {
                EvaluateAndReportBestHand();
                _uiView?.SetRestartButtonEnabled(false);
                SetInteractionState(false);
                _uiView?.AllowResetButton(true);
            }
        }

        private void HandleFoldRequested() {
            if (!_isGameModeActive) return;
            _audioService?.PlayInvalidMove();
            _uiView?.DisplayOutcome("Folded. Select a new hand.");
            SetInteractionState(false);
            _uiView?.AllowResetButton(true);
        }

        private void SetInteractionState(bool isEnabled) {
            _uiView?.SetInteractionState(isEnabled);
            playButtonsAreActive = isEnabled;
        }



        private void RefreshView() {
            _uiView?.RenderRoundState(_engine.CurrentRound, _engine.PlayerHand, _engine.CommunityCards);

            for (int i = _spawnedPlayerCount; i < _engine.PlayerHand.Count; i++) {
                _uiView?.SpawnPhysicalCard(_engine.PlayerHand[i], true);
                PlayCardDrop();
            }
            _spawnedPlayerCount = _engine.PlayerHand.Count;

            if (!_housePlaceholdersSpawned && _engine.HouseHand.Count > 0) {
                _uiView?.SpawnHousePlaceholders(_engine.HouseHand.Count);
                _spawnedHouseCount = _engine.HouseHand.Count;
                _housePlaceholdersSpawned = true;
            }

            for (int i = _spawnedCommunityCount; i < _engine.CommunityCards.Count; i++) {
                _uiView?.SpawnPhysicalCard(_engine.CommunityCards[i], false, false);
                PlayCardDrop();
            }
            _spawnedCommunityCount = _engine.CommunityCards.Count;
        }

        private void EvaluateAndReportBestHand() {
            var playerBestResult = EvaluateBestFiveCardHand(_engine.PlayerHand, _engine.CommunityCards);
            var houseBestResult = EvaluateBestFiveCardHand(_engine.HouseHand, _engine.CommunityCards);

            string outcome;
            if ((int)playerBestResult.Rank > (int)houseBestResult.Rank) {
                outcome = $"Player Wins! \n  {FormatRank(playerBestResult.Rank)} with {FormatCardList(playerBestResult.Cards)}. \n House best: {FormatRank(houseBestResult.Rank)} with {FormatCardList(houseBestResult.Cards)}.";
            }
            else if ((int)houseBestResult.Rank > (int)playerBestResult.Rank) {
                outcome = $"House Wins! \n  {FormatRank(houseBestResult.Rank)} with {FormatCardList(houseBestResult.Cards)}. \n Player best: {FormatRank(playerBestResult.Rank)} with {FormatCardList(playerBestResult.Cards)}.";
            }
            else {
                outcome = $"Push on {FormatRank(playerBestResult.Rank)}. \n Player best: {FormatCardList(playerBestResult.Cards)}. \n House best: {FormatCardList(houseBestResult.Cards)}.";
            }

            _uiView?.RevealHouseHand(_engine.HouseHand);

            if (_currentWager > 0 && _economyService != null) {
                int payout = Mathf.FloorToInt(_currentWager * 1.5f);
                _economyService.CreditGold(payout);
            }

            _uiView?.DisplayOutcome(outcome);
        }
        #endregion
        #region Utility Methods
        private static (HandRank Rank, List<CardData> Cards) EvaluateBestFiveCardHand(List<CardData> holeCards, List<CardData> communityCards) {
            var combinedCards = new List<CardData>(holeCards);
            combinedCards.AddRange(communityCards);

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

            return (bestRank, bestHand);
        }

        private static string FormatCardList(List<CardData> cards) {
            if (cards == null || cards.Count == 0) {
                return "No cards";
            }

            return string.Join(", ", cards.Select(card => $"{card.CardRank} of {card.CardSuit}"));
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

        #endregion
    }
}
