using NUnit.Framework;
using CardFramework.Core.Models;
using System;

namespace CardFramework.Tests.EditMode.Core {
    [TestFixture]
    public class DeckTests {
        private Deck deck;

        [SetUp]
        public void Setup() {
            deck = new Deck();
        }

        [Test]
        public void Initialize_Creates52Cards() {
            deck.Initialize();
            Assert.AreEqual(52, deck.RemainingCount, "A standard deck must contain 52 cards upon initialization.");
        }

        [Test]
        public void Draw_DecrementsDeckCount() {
            deck.Initialize();
            _ = deck.Draw();
            Assert.AreEqual(51, deck.RemainingCount, "Drawing a card must decrement the remaining count by 1.");
        }

        [Test]
        public void IsEmpty_ReturnsTrueWhenDeckIsEmpty() {
            deck.Initialize();
            for (int i = 0; i < 52; i++) {
                deck.Draw();
            }

            Assert.IsTrue(deck.IsEmpty, "Deck must report empty when all cards have been drawn.");
        }

        [Test]
        public void Peek_ReturnsNextCardWithoutRemovingIt() {
            deck.Initialize();
            var firstCard = deck.Peek();
            Assert.AreEqual(52, deck.RemainingCount, "Peek must not remove a card from the deck.");
            Assert.AreEqual(firstCard, deck.Peek(), "Peek should return the same top-card on repeated calls.");
        }

        [Test]
        public void Peek_ThrowsWhenEmpty() {
            deck.Initialize();
            for (int i = 0; i < 52; i++) {
                deck.Draw();
            }

            Assert.Throws<InvalidOperationException>(() => deck.Peek(), "Peeking an empty deck must throw an InvalidOperationException.");
        }

        [Test]
        public void Draw_ThrowsWhenEmpty() {
            deck.Initialize();

            // Draw all 52 cards to empty the deck
            for (int i = 0; i < 52; i++) {
                deck.Draw();
            }

            Assert.Throws<System.InvalidOperationException>(() => deck.Draw(),
                "Drawing from an empty deck must throw an InvalidOperationException.");
        }

        [Test]
        public void Shuffle_RandomizesOrder() {
            deck.Initialize();

            // Capture the sequential un-shuffled order using Peek and Draw
            var originalOrder = new CardData[52];
            for (int i = 0; i < 52; i++) {
                originalOrder[i] = deck.Draw();
            }

            // Reset and shuffle with a specific seed for deterministic testing if needed,
            // or empty to let UnityEngine.Random handle it.
            deck.Reset();
            deck.Shuffle(42); // Using a fixed seed to ensure randomization consistency in tests

            int matchCount = 0;
            for (int i = 0; i < 52; i++) {
                if (originalOrder[i] == deck.Draw()) {
                    matchCount++;
                }
            }

            // Out of 52 cards, it is statistically highly improbable to get more than a few matches by pure coincidence.
            Assert.Less(matchCount, 10, "The shuffled deck order should significantly differ from the original sequential order.");
        }

        [Test]
        public void Reset_ResetsToFull() {
            deck.Initialize();
            for (int i = 0; i < 10; i++) {
                deck.Draw();
            }

            Assert.AreEqual(42, deck.RemainingCount);

            deck.Reset();
            Assert.AreEqual(52, deck.RemainingCount, "Resetting the deck must restore the count back to 52 cards.");
        }
    }
}