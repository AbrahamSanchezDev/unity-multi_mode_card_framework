using NUnit.Framework;
using CardFramework.Core.Models;

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