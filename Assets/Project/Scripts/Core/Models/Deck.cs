using System;
using System.Collections.Generic;

namespace CardFramework.Core.Models {
    /// <summary>
    /// Deck management with Fisher-Yates shuffling.
    /// Pure C# - no Unity dependencies except Random for seeding.
    /// </summary>
    public class Deck {
        // Exposing the cards list internally or via a property if needed for testing, 
        // keeping compliance with the implementation plan's test structure.
        internal List<CardData> cards = new();
        private int drawIndex = 0;

        public int RemainingCount => cards.Count - drawIndex;
        public bool IsEmpty => RemainingCount <= 0;

        /// <summary>
        /// Initialize a standard 52-card deck
        /// </summary>
        public void Initialize() {
            cards.Clear();
            drawIndex = 0;

            for (int suit = 0; suit < 4; suit++) {
                for (int rank = 1; rank <= 13; rank++) {
                    var card = new CardData((CardData.Suit)suit, (CardData.Rank)rank);
                    cards.Add(card);
                }
            }
        }

        /// <summary>
        /// Fisher-Yates shuffle algorithm - O(n) guaranteed.
        /// 100% Pure C# implementation using standard System.Random.
        /// </summary>
        public void Shuffle(int seed = -1) {
            // If no external seed is provided, generate a non-deterministic seed using System.Random
            if (seed < 0) {
                var localRng = new System.Random();
                seed = localRng.Next(0, int.MaxValue);
            }

            var rng = new System.Random(seed);

            for (int i = cards.Count - 1; i > 0; i--) {
                int randomIndex = rng.Next(0, i + 1);
                (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
            }

            drawIndex = 0;
        }

        /// <summary>
        /// Draw a card from the deck
        /// </summary>
        public CardData Draw() {
            if (IsEmpty)
                throw new InvalidOperationException("Cannot draw from empty deck");

            return cards[drawIndex++];
        }

        /// <summary>
        /// Peek at next card without drawing
        /// </summary>
        public CardData Peek() {
            if (IsEmpty)
                throw new InvalidOperationException("Deck is empty");

            return cards[drawIndex];
        }

        /// <summary>
        /// Reset deck to full state
        /// </summary>
        public void Reset() {
            Initialize();
        }
    }
}