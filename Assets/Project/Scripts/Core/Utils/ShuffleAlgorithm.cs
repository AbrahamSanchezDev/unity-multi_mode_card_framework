using System;
using System.Collections.Generic;
using CardFramework.Core.Models;

namespace CardFramework.Core.Utils {
    public static class ShuffleAlgorithm {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Shuffles a list of cards in-place using the Fisher-Yates algorithm.
        /// O(n) Time Complexity / O(1) Space Complexity.
        /// </summary>
        public static void Shuffle(List<CardData> cards) {
            if (cards == null || cards.Count <= 1) return;

            int n = cards.Count;
            while (n > 1) {
                n--;
                int k = _random.Next(n + 1);
                CardData value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }
    }
}