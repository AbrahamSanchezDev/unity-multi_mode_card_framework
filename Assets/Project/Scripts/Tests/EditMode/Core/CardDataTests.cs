using NUnit.Framework;
using CardFramework.Core.Models;

namespace CardFramework.Tests.EditMode.Core {
    [TestFixture]
    public class CardDataTests {
        [Test]
        public void Constructor_SetsSuitAndRankCorrectly() {
            var card = new CardData(CardData.Suit.Hearts, CardData.Rank.Queen);

            Assert.AreEqual(CardData.Suit.Hearts, card.CardSuit, "Constructor must set the suit.");
            Assert.AreEqual(CardData.Rank.Queen, card.CardRank, "Constructor must set the rank.");
        }

        [Test]
        public void Equals_WithSameCardValues_ReturnsTrue() {
            var left = new CardData(CardData.Suit.Spades, CardData.Rank.Ace, instanceId: 1);
            var right = new CardData(CardData.Suit.Spades, CardData.Rank.Ace, instanceId: 1);

            Assert.IsTrue(left.Equals(right), "Equals(CardData) must return true for identical cards.");
            Assert.IsTrue(left == right, "Operator == must return true for identical cards.");
            Assert.IsFalse(left != right, "Operator != must return false for identical cards.");
        }

        [Test]
        public void Equals_WithDifferentCardValues_ReturnsFalse() {
            var left = new CardData(CardData.Suit.Hearts, CardData.Rank.Ten, instanceId: 1);
            var right = new CardData(CardData.Suit.Clubs, CardData.Rank.Ten, instanceId: 2);

            Assert.IsFalse(left.Equals(right), "Equals(CardData) must return false for different suits.");
            Assert.IsFalse(left == right, "Operator == must return false for different cards.");
            Assert.IsTrue(left != right, "Operator != must return true for different cards.");
        }

        [Test]
        public void Equals_ObjectWithDifferentType_ReturnsFalse() {
            var card = new CardData(CardData.Suit.Diamonds, CardData.Rank.Seven);
            var other = "NotACard";

            Assert.IsFalse(card.Equals(other), "Equals(object) must return false for a non-CardData object.");
        }

        [Test]
        public void Equals_ObjectNull_ReturnsFalse() {
            var card = new CardData(CardData.Suit.Diamonds, CardData.Rank.Four);
            object obj = null;

            Assert.IsFalse(card.Equals(obj), "Equals(object) must return false when passed null.");
        }

        [Test]
        public void Constructor_WithFaceUpChange_PreservesInstanceId() {
            var originalCard = new CardData(CardData.Suit.Hearts, CardData.Rank.Ten, instanceId: 42);
            var flippedCard = new CardData(originalCard.CardSuit, originalCard.CardRank, true, originalCard.InstanceId);

            Assert.AreEqual(42, flippedCard.InstanceId, "A card should preserve its identity when its face-up state changes.");
        }

        [Test]
        public void GetHashCode_ProducesConsistentValueForSameCard() {
            var cardA = new CardData(CardData.Suit.Hearts, CardData.Rank.Ten, instanceId: 1);
            var cardB = new CardData(CardData.Suit.Hearts, CardData.Rank.Ten, instanceId: 1);

            Assert.AreEqual(cardA.GetHashCode(), cardB.GetHashCode(), "Equal cards must produce the same hash code.");
        }

        [Test]
        public void GetHashCode_EncodesSuitAndRank() {
            var card = new CardData(CardData.Suit.Hearts, CardData.Rank.Ten);
            var expectedHash = ((int)CardData.Suit.Hearts << 4) | (int)CardData.Rank.Ten;

            Assert.AreEqual(expectedHash, card.GetHashCode(), "GetHashCode must encode the suit and rank bits consistently.");
        }

        [Test]
        public void ToString_ReturnsRankAndSuit() {
            var card = new CardData(CardData.Suit.Spades, CardData.Rank.King);
            Assert.AreEqual("KingSpades", card.ToString(), "ToString must concatenate rank and suit.");
        }
    }
}
