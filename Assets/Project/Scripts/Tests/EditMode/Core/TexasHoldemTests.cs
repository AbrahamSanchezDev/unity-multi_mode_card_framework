using NUnit.Framework;
using System;
using System.Collections.Generic;
using CardFramework.Core.Utils;
using CardFramework.Core.Models;
using CardFramework.Core.Engines;

namespace CardFramework.Tests.EditMode.Core
{
    [TestFixture]
    public class TexasHoldemTests
    {
        // ==========================================
        // 1. HAND EVALUATOR TESTS (Targeting 100% Coverage)
        // ==========================================

        [Test]
        public void Evaluator_ThrowsException_WhenHandIsNotExactlyFiveCards()
        {
            var invalidHand = new List<CardData>
            {
                new CardData(CardData.Suit.Hearts, CardData.Rank.Two),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Three)
            };

            Assert.Throws<ArgumentException>(() => HandEvaluator.EvaluateFiveCardHand(invalidHand),
                "Evaluator must throw an ArgumentException if card count is not exactly 5.");
        }

        [Test]
        public void Evaluator_DetectsRoyalFlush()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Spades, CardData.Rank.Ace),
                new CardData(CardData.Suit.Spades, CardData.Rank.King),
                new CardData(CardData.Suit.Spades, CardData.Rank.Queen),
                new CardData(CardData.Suit.Spades, CardData.Rank.Jack),
                new CardData(CardData.Suit.Spades, CardData.Rank.Ten)
            };
            Assert.AreEqual(HandRank.RoyalFlush, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsStraightFlush()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Hearts, CardData.Rank.Nine),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Eight),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Seven),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Six),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Five)
            };
            Assert.AreEqual(HandRank.StraightFlush, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsFourOfAKind()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Clubs, CardData.Rank.Ace),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Ace),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Ace),
                new CardData(CardData.Suit.Spades, CardData.Rank.Ace),
                new CardData(CardData.Suit.Clubs, CardData.Rank.Two)
            };
            Assert.AreEqual(HandRank.FourOfAKind, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsFullHouse()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Clubs, CardData.Rank.King),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.King),
                new CardData(CardData.Suit.Hearts, CardData.Rank.King),
                new CardData(CardData.Suit.Spades, CardData.Rank.Four),
                new CardData(CardData.Suit.Clubs, CardData.Rank.Four)
            };
            Assert.AreEqual(HandRank.FullHouse, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsFlush()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Two),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Five),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Eight),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Jack),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.King)
            };
            Assert.AreEqual(HandRank.Flush, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsStraight()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Clubs, CardData.Rank.Eight),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Seven),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Six),
                new CardData(CardData.Suit.Spades, CardData.Rank.Five),
                new CardData(CardData.Suit.Clubs, CardData.Rank.Four)
            };
            Assert.AreEqual(HandRank.Straight, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsThreeOfAKind()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Clubs, CardData.Rank.Queen),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Queen),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Queen),
                new CardData(CardData.Suit.Spades, CardData.Rank.Two),
                new CardData(CardData.Suit.Clubs, CardData.Rank.Five)
            };
            Assert.AreEqual(HandRank.ThreeOfAKind, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsTwoPair()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Clubs, CardData.Rank.Jack),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Jack),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Three),
                new CardData(CardData.Suit.Spades, CardData.Rank.Three),
                new CardData(CardData.Suit.Clubs, CardData.Rank.Seven)
            };
            Assert.AreEqual(HandRank.TwoPair, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsOnePair()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Clubs, CardData.Rank.Ten),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Ten),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Two),
                new CardData(CardData.Suit.Spades, CardData.Rank.Five),
                new CardData(CardData.Suit.Clubs, CardData.Rank.King)
            };
            Assert.AreEqual(HandRank.OnePair, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        [Test]
        public void Evaluator_DetectsHighCard()
        {
            var hand = new List<CardData>
            {
                new CardData(CardData.Suit.Clubs, CardData.Rank.Ace),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.King),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Eight),
                new CardData(CardData.Suit.Spades, CardData.Rank.Four),
                new CardData(CardData.Suit.Clubs, CardData.Rank.Two)
            };
            Assert.AreEqual(HandRank.HighCard, HandEvaluator.EvaluateFiveCardHand(hand));
        }

        // ==========================================
        // 2. TEXAS HOLDEM ENGINE TESTS (Targeting 100% Coverage)
        // ==========================================

        [Test]
        public void Engine_StartNewHand_InitializesCorrectly()
        {
            var engine = new TexasHoldemEngine();
            engine.StartNewHand();

            Assert.AreEqual(2, engine.PlayerHand.Count, "Player must receive exactly 2 hole cards.");
            Assert.AreEqual(0, engine.CommunityCards.Count, "Community cards must be empty at PreFlop.");
            Assert.AreEqual(TexasHoldemEngine.RoundState.PreFlop, engine.CurrentRound);
        }

        [Test]
        public void Engine_AdvanceRound_CyclesThroughAllStatesCorrectly()
        {
            var engine = new TexasHoldemEngine();
            engine.StartNewHand();

            // PreFlop -> Flop
            engine.AdvanceRound();
            Assert.AreEqual(TexasHoldemEngine.RoundState.Flop, engine.CurrentRound);
            Assert.AreEqual(3, engine.CommunityCards.Count, "Flop must deal exactly 3 community cards.");

            // Flop -> Turn
            engine.AdvanceRound();
            Assert.AreEqual(TexasHoldemEngine.RoundState.Turn, engine.CurrentRound);
            Assert.AreEqual(4, engine.CommunityCards.Count, "Turn must add exactly 1 community card (Total 4).");

            // Turn -> River
            engine.AdvanceRound();
            Assert.AreEqual(TexasHoldemEngine.RoundState.River, engine.CurrentRound);
            Assert.AreEqual(5, engine.CommunityCards.Count, "River must add exactly 1 community card (Total 5).");

            // River -> Showdown
            engine.AdvanceRound();
            Assert.AreEqual(TexasHoldemEngine.RoundState.Showdown, engine.CurrentRound);
            Assert.AreEqual(5, engine.CommunityCards.Count, "Showdown keeps the 5 community cards intact.");
        }
    }
}