using System;
using System.Collections.Generic;
using System.Linq;
using CardFramework.Core.Models;

namespace CardFramework.Core.Utils {
    public enum HandRank {
        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }

    /// <summary>
    /// Pure C# Texas Hold'em hand evaluator.
    /// Evaluates combinations of cards to determine exact poker rankings.
    /// </summary>
    public static class HandEvaluator {
        /// <summary>
        /// Evaluates a 5-card poker hand and determines its exact strength ranking.
        /// </summary>
        /// <param name="cards">A list containing exactly 5 cards to evaluate.</param>
        /// <returns>The highest <see cref="HandRank"/> achieved by the combination of cards.</returns>
        /// <exception cref="ArgumentException">Thrown when the input list does not contain exactly 5 cards.</exception>
        public static HandRank EvaluateFiveCardHand(List<CardData> cards) {
            // Guard Clause: Ensure game rules are met before processing.
            if (cards.Count != 5)
                throw new ArgumentException("Hand evaluator requires exactly 5 cards.");

            HandRank result;

            // 1. Pre-processing: Sort cards by rank descending to optimize straight and group matching.
            var orderedByRank = cards.OrderByDescending(c => c.CardRank).ToList();

            // 2. Structural Analysis: Check for Flush (all suits match) and Straight (sequential ranks).
            bool isFlush = cards.All(c => c.CardSuit == cards[0].CardSuit);
            bool isStraight = IsSequence(orderedByRank);

            // 3. Evaluation Pipeline: High-value combinations (Flushes & Straights)
            if (isFlush && isStraight) {
                // Special Case: If it's a straight flush and contains King down to Ace (Broadway order), it's a Royal Flush.
                if (orderedByRank[0].CardRank == CardData.Rank.King &&
                    orderedByRank[1].CardRank == CardData.Rank.Queen &&
                    orderedByRank[2].CardRank == CardData.Rank.Jack &&
                    orderedByRank[3].CardRank == CardData.Rank.Ten &&
                    orderedByRank[4].CardRank == CardData.Rank.Ace) {
                    result = HandRank.RoyalFlush;
                }
                else {
                    result = HandRank.StraightFlush;
                }
            }
            // 4. Evaluation Pipeline: Group-based combinations (Pairs, Trips, Full Houses, etc.)
            else {
                // Group identical card ranks and order groups by size (frequency) descending.
                var groups = cards.GroupBy(c => c.CardRank).OrderByDescending(g => g.Count()).ToList();

                // Four cards share the same rank.
                if (groups[0].Count() == 4)
                    result = HandRank.FourOfAKind;

                // Three of a kind combined with a distinct pair.
                else if (groups[0].Count() == 3 && groups[1].Count() == 2)
                    result = HandRank.FullHouse;

                // Five cards of the same suit, non-sequential.
                else if (isFlush)
                    result = HandRank.Flush;

                // Five sequential cards, mixed suits.
                else if (isStraight)
                    result = HandRank.Straight;

                // Three cards share the same rank.
                else if (groups[0].Count() == 3)
                    result = HandRank.ThreeOfAKind;

                // Two distinct pairs found.
                else if (groups[0].Count() == 2 && groups[1].Count() == 2)
                    result = HandRank.TwoPair;

                // One pair found.
                else if (groups[0].Count() == 2)
                    result = HandRank.OnePair;

                // No matching combinations; highest card wins.
                else
                    result = HandRank.HighCard;
            }

            // Single Exit Point
            return result;
        }

        private static bool IsSequence(List<CardData> orderedCards) {
            // Check for standard descending sequence (e.g., King, Queen, Jack, Ten, Nine...)
            bool isStandardSequence = true;
            for (int i = 0; i < orderedCards.Count - 1; i++) {
                if ((int)orderedCards[i].CardRank - (int)orderedCards[i + 1].CardRank != 1) {
                    isStandardSequence = false;
                    break;
                }
            }

            if (isStandardSequence) return true;

            // Special Case: Broadway Straight (Ace, King, Queen, Jack, Ten)
            // In our Rank enum, Ace is 1 and King is 13, so ordered by descending rank gives: King(13), Queen(12), Jack(11), Ten(10), Ace(1)
            if (orderedCards[0].CardRank == CardData.Rank.King &&
                orderedCards[1].CardRank == CardData.Rank.Queen &&
                orderedCards[2].CardRank == CardData.Rank.Jack &&
                orderedCards[3].CardRank == CardData.Rank.Ten &&
                orderedCards[4].CardRank == CardData.Rank.Ace) {
                return true;
            }

            return false;
        }
    }
}