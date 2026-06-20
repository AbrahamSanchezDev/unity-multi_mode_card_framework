// File: Assets/_Project/Scripts/Core/Engines/SolitaireEngine.cs
using CardFramework.Core.Models;
using System.Collections.Generic;

namespace CardFramework.Core.Engines {
    /// <summary>
    /// Klondike Solitaire game logic
    /// Validates moves, manages tableau and foundation stacks
    /// </summary>
    public class SolitaireEngine {
        private List<CardData>[] tableau = new List<CardData>[7];        // 7 columns
        private List<CardData>[] foundation = new List<CardData>[4];     // 4 suits
        private List<CardData> stock = new();                            // Draw pile
        private List<CardData> waste = new();                            // Discarded pile

        public SolitaireEngine() {
            for (int i = 0; i < 7; i++)
                tableau[i] = new List<CardData>();

            for (int i = 0; i < 4; i++)
                foundation[i] = new List<CardData>();
        }

        public void Initialize() {
            var deck = new Deck();
            deck.Initialize();
            deck.Shuffle();

            // Clear previous game states
            stock.Clear();
            waste.Clear();
            for (int i = 0; i < 7; i++) tableau[i].Clear();
            for (int i = 0; i < 4; i++) foundation[i].Clear();

            // Deal tableau (pyramid pattern)
            for (int col = 0; col < 7; col++) {
                for (int i = col; i < 7; i++) {
                    tableau[i].Add(deck.Draw());
                }
            }

            // Remaining cards go to stock draw pile
            while (!deck.IsEmpty) {
                stock.Add(deck.Draw());
            }
        }

        public bool CanPlaceOnTableau(CardData card, int column) {
            if (column < 0 || column >= 7)
                return false;

            // Empty columns can only accept Kings
            if (tableau[column].Count == 0)
                return card.CardRank == CardData.Rank.King;

            var topCard = tableau[column][tableau[column].Count - 1];

            // Alternating color and descending rank
            bool differentColor = GetColor(card.CardSuit) != GetColor(topCard.CardSuit);
            bool descendingRank = card.CardRank == topCard.CardRank - 1;

            return differentColor && descendingRank;
        }

        public bool CanPlaceOnFoundation(CardData card, int suitIndex) {
            if (suitIndex < 0 || suitIndex >= 4)
                return false;

            // Empty foundations can only accept Aces
            if (foundation[suitIndex].Count == 0)
                return card.CardRank == CardData.Rank.Ace;

            var topCard = foundation[suitIndex][foundation[suitIndex].Count - 1];
            return (int)card.CardSuit == suitIndex && card.CardRank == topCard.CardRank + 1;
        }

        public bool HasWon() {
            return foundation[0].Count == 13 && foundation[1].Count == 13 &&
                   foundation[2].Count == 13 && foundation[3].Count == 13;
        }

        private int GetColor(CardData.Suit suit) {
            // Red: Diamonds, Hearts; Black: Clubs, Spades
            // Red: 0 (Diamonds, Hearts) | Black: 1 (Clubs, Spades)
            return suit == CardData.Suit.Diamonds || suit == CardData.Suit.Hearts ? 0 : 1;
        }

        // getters exposed for the unit testing suite and view controllers
        public List<CardData>[] GetTableau() => tableau;
        public List<CardData>[] GetFoundation() => foundation;
    }
}