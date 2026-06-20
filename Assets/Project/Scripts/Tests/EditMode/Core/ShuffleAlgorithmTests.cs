using System;
using System.Collections.Generic;
using NUnit.Framework;
using CardFramework.Core.Models;
using CardFramework.Core.Utils;

namespace CardFramework.Tests.EditMode.Core {
    [TestFixture]
    public class ShuffleAlgorithmTests {
        [Test]
        public void Shuffle_NullList_DoesNotThrow() {
            Assert.DoesNotThrow(() => ShuffleAlgorithm.Shuffle(null));
        }

        [Test]
        public void Shuffle_EmptyList_DoesNothing() {
            var cards = new List<CardData>();
            ShuffleAlgorithm.Shuffle(cards);
            Assert.AreEqual(0, cards.Count);
        }

        [Test]
        public void Shuffle_SingleItemList_DoesNothing() {
            var cards = new List<CardData> { new CardData(CardData.Suit.Clubs, CardData.Rank.Ace) };
            ShuffleAlgorithm.Shuffle(cards);
            Assert.AreEqual(1, cards.Count);
            Assert.AreEqual(CardData.Suit.Clubs, cards[0].CardSuit);
            Assert.AreEqual(CardData.Rank.Ace, cards[0].CardRank);
        }

        [Test]
        public void Shuffle_MultiItemList_RetainsSameElements() {
            var cards = new List<CardData>();
            for (int suit = 0; suit < 4; suit++) {
                for (int rank = 1; rank <= 5; rank++) {
                    cards.Add(new CardData((CardData.Suit)suit, (CardData.Rank)rank));
                }
            }

            var originalCards = new List<CardData>(cards);
            ShuffleAlgorithm.Shuffle(cards);

            Assert.AreEqual(originalCards.Count, cards.Count, "Shuffled list must keep the same number of cards.");
            CollectionAssert.AreEquivalent(originalCards, cards, "Shuffled list must contain the same cards as the original list.");
        }

        [Test]
        public void Shuffle_MultiItemList_ChangesOrder() {
            var cards = new List<CardData>();
            for (int rank = 1; rank <= 20; rank++) {
                cards.Add(new CardData(CardData.Suit.Clubs, (CardData.Rank)rank));
            }

            var originalCards = new List<CardData>(cards);
            ShuffleAlgorithm.Shuffle(cards);

            Assert.AreEqual(originalCards.Count, cards.Count);
            Assert.IsFalse(AreSequencesEqual(originalCards, cards), "Shuffle should change the card order for a multi-item list.");
        }

        private static bool AreSequencesEqual(IReadOnlyList<CardData> a, IReadOnlyList<CardData> b) {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++) {
                if (!a[i].Equals(b[i])) return false;
            }
            return true;
        }
    }
}
