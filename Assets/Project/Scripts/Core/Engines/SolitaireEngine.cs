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
        private readonly List<(int ColumnIndex, int CardIndex)> _lastRevealedCards = new();

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
            _lastRevealedCards.Clear();
            for (int i = 0; i < 7; i++) tableau[i].Clear();
            for (int i = 0; i < 4; i++) foundation[i].Clear();

            // Deal tableau (pyramid pattern)
            for (int col = 0; col < 7; col++) {
                for (int i = col; i < 7; i++) {
                    tableau[i].Add(deck.Draw());
                }
            }

            // Only the top card in each tableau column starts face up.
            for (int col = 0; col < 7; col++) {
                if (tableau[col].Count > 0) {
                    var topCard = tableau[col][^1];
                    var faceUpCard = Deck.CreateCardWithRevealState(topCard, isFaceUp: true);
                    tableau[col][^1] = faceUpCard;
                }
            }

            // Remaining cards go to stock draw pile
            while (!deck.IsEmpty) {
                stock.Add(deck.Draw());
            }
        }

        /// <summary>
        /// Draws a card from stock into waste, or recycles waste back into stock if empty.
        /// </summary>
        public void DrawCard() {
            if (stock.Count > 0) {
                CardData drawnCard = stock[^1];
                stock.RemoveAt(stock.Count - 1);
                waste.Add(Deck.CreateCardWithRevealState(drawnCard, isFaceUp: true));
            } else if (waste.Count > 0) {
                // Recycle waste back to stock
                for (int i = waste.Count - 1; i >= 0; i--) {
                    stock.Add(Deck.CreateCardWithRevealState(waste[i], isFaceUp: false));
                }
                waste.Clear();
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

            // Empty foundations can only accept Aces, and the Ace determines the suit for that pile.
            if (foundation[suitIndex].Count == 0)
                return card.CardRank == CardData.Rank.Ace;

            var topCard = foundation[suitIndex][foundation[suitIndex].Count - 1];
            return card.CardSuit == topCard.CardSuit && card.CardRank == topCard.CardRank + 1;
        }

        public void MoveCardToTableau(CardData card, int column) {
            if (column < 0 || column >= 7)
                return;

            RemoveCardFromTableau(card);
            waste.Remove(card);
            tableau[column].Add(Deck.CreateCardWithRevealState(card, isFaceUp: true));

            if (tableau[column].Count > 0) {
                var topCard = tableau[column][^1];
                tableau[column][^1] = Deck.CreateCardWithRevealState(topCard, isFaceUp: true);
            }
        }

        public bool MoveCardsToTableau(List<CardData> cards, int sourceColumn, int startIndex, int targetColumn) {
            if (cards == null || cards.Count == 0 || targetColumn < 0 || targetColumn >= 7)
                return false;

            if (!CanPlaceOnTableau(cards[0], targetColumn))
                return false;

            var movedCards = new List<CardData>(cards.Count);
            for (int i = 0; i < cards.Count; i++) {
                movedCards.Add(Deck.CreateCardWithRevealState(cards[i], isFaceUp: true));
            }

            _lastRevealedCards.Clear();
            if (sourceColumn >= 0 && sourceColumn < 7) {
                if (sourceColumn == targetColumn)
                    return false;

                if (startIndex < 0 || startIndex >= tableau[sourceColumn].Count)
                    return false;

                var sourceColumnCards = tableau[sourceColumn];
                int removeCount = sourceColumnCards.Count - startIndex;
                sourceColumnCards.RemoveRange(startIndex, removeCount);
                if (sourceColumnCards.Count > 0) {
                    var revealedCard = sourceColumnCards[^1];
                    var faceUpCard = Deck.CreateCardWithRevealState(revealedCard, isFaceUp: true);
                    sourceColumnCards[^1] = faceUpCard;
                    if (faceUpCard.HasBeenRevealed) {
                        _lastRevealedCards.Add((sourceColumn, sourceColumnCards.Count - 1));
                    }
                }
            }
            else {
                // Waste-originated cards are not removed from a tableau column; they are simply removed from the waste pile.
                for (int i = 0; i < cards.Count; i++) {
                    waste.Remove(cards[i]);
                }
            }

            tableau[targetColumn].AddRange(movedCards);
            return true;
        }

        public void MoveCardToFoundation(CardData card, int suitIndex) {
            if (suitIndex < 0 || suitIndex >= 4)
                return;

            RemoveCardFromTableau(card);
            waste.Remove(card);
            foundation[suitIndex].Add(Deck.CreateCardWithRevealState(card, isFaceUp: true));
        }

        public bool MoveCardsToFoundation(List<CardData> cards, int suitIndex) {
            if (cards == null || cards.Count == 0 || suitIndex < 0 || suitIndex >= 4)
                return false;

            if (!CanPlaceOnFoundation(cards[^1], suitIndex))
                return false;

            var movedCard = cards[^1];
            RemoveCardFromTableau(movedCard);
            waste.Remove(movedCard);
            foundation[suitIndex].Add(Deck.CreateCardWithRevealState(movedCard, isFaceUp: true));
            return true;
        }

        private void RemoveCardFromTableau(CardData card) {
            _lastRevealedCards.Clear();
            for (int i = 0; i < 7; i++) {
                var column = tableau[i];
                for (int j = 0; j < column.Count; j++) {
                    if (column[j].Equals(card)) {
                        column.RemoveAt(j);
                        if (column.Count > 0) {
                            var revealedCard = column[^1];
                            var faceUpCard = Deck.CreateCardWithRevealState(revealedCard, isFaceUp: true);
                            column[^1] = faceUpCard;
                            if (faceUpCard.HasBeenRevealed) {
                                _lastRevealedCards.Add((i, column.Count - 1));
                            }
                        }
                        return;
                    }
                }
            }
        }

        public bool HasWon() {
            return foundation[0].Count == 13 && foundation[1].Count == 13 &&
                   foundation[2].Count == 13 && foundation[3].Count == 13;
        }

        private int GetColor(CardData.Suit suit) {
            // Red: Diamonds, Hearts; Black: Clubs, Spades
            return suit == CardData.Suit.Diamonds || suit == CardData.Suit.Hearts ? 0 : 1;
        }

        // Getters exposed for unit tests, controllers, and view rendering
        public List<CardData>[] GetTableau() => tableau;
        public List<CardData>[] GetFoundation() => foundation;
        public List<CardData> GetStock() => stock;
        public List<CardData> GetWaste() => waste;
        public List<(int ColumnIndex, int CardIndex)> ConsumeLastRevealedCards() {
            var revealedCards = new List<(int ColumnIndex, int CardIndex)>(_lastRevealedCards);
            _lastRevealedCards.Clear();
            return revealedCards;
        }

    }
}