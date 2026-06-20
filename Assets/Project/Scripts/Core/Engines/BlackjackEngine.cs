using CardFramework.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace CardFramework.Core.Engines {
    /// <summary>
    /// Pure Blackjack (21) game logic engine
    /// Handles hand evaluation, dealer AI, bust detection
    /// </summary>
    public class BlackjackEngine {
        public class Hand {
            public List<CardData> Cards { get; } = new();

            public int GetHandValue() {
                int value = 0;
                int aces = 0;

                foreach (var card in Cards) {
                    if (card.CardRank == CardData.Rank.Ace) {
                        aces++;
                        value += 11;
                    }
                    else if (card.CardRank >= CardData.Rank.Jack)
                        value += 10;
                    else
                        value += (int)card.CardRank;
                }

                // Adjust for Aces if we busted
                while (value > 21 && aces > 0) {
                    value -= 10;
                    aces--;
                }

                return value;
            }

            public bool IsBust => GetHandValue() > 21;
            public bool IsBlackjack => Cards.Count == 2 && GetHandValue() == 21;
            public bool IsSoft17 => GetHandValue() == 17 && Cards.Any(c => c.CardRank == CardData.Rank.Ace);
        }

        public enum GameState { PlayerTurn, DealerTurn, PlayerBust, DealerBust, Showdown, GameOver }

        private Deck deck = new();
        private Hand playerHand = new();
        private Hand dealerHand = new();
        public GameState CurrentState { get; private set; } = GameState.PlayerTurn;

        public BlackjackEngine() {
            deck.Initialize();
            deck.Shuffle();
        }

        public void DealInitialHands() {
            playerHand.Cards.Clear();
            dealerHand.Cards.Clear();

            // Deal 2 cards to player and dealer (Standard casino practice)
            playerHand.Cards.Add(deck.Draw());
            dealerHand.Cards.Add(deck.Draw());
            playerHand.Cards.Add(deck.Draw());
            dealerHand.Cards.Add(deck.Draw());

            if (playerHand.IsBlackjack) {
                CurrentState = GameState.Showdown;
            }
            else {
                CurrentState = GameState.PlayerTurn;
            }
        }

        public void PlayerHit() {
            if (CurrentState != GameState.PlayerTurn)
                return;

            playerHand.Cards.Add(deck.Draw());

            if (playerHand.IsBust)
                CurrentState = GameState.PlayerBust;
        }

        public void PlayerStand() {
            if (CurrentState != GameState.PlayerTurn)
                return;

            CurrentState = GameState.DealerTurn;
            ExecuteDealerTurn();
        }

        private void ExecuteDealerTurn() {
            // Dealer must hit on soft 17
            while (dealerHand.GetHandValue() < 17 || dealerHand.IsSoft17) {
                dealerHand.Cards.Add(deck.Draw());
            }

            if (dealerHand.IsBust)
                CurrentState = GameState.DealerBust;
            else
                CurrentState = GameState.Showdown;
        }

        public int GetPlayerValue() => playerHand.GetHandValue();
        public int GetDealerValue() => dealerHand.GetHandValue();
        public Hand GetPlayerHand() => playerHand;
        public Hand GetDealerHand() => dealerHand;
    }
}