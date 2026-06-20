using NUnit.Framework;
using CardFramework.Core.Engines;
using CardFramework.Core.Models;

namespace CardFramework.Tests.EditMode.Core {
    [TestFixture]
    public class BlackjackEngineTests {
        private BlackjackEngine.Hand testHand;

        [SetUp]
        public void Setup() {
            testHand = new BlackjackEngine.Hand();
        }

        [Test]
        public void HandValue_CalculatesFaceCardsAsTen() {
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Jack));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.King));

            Assert.AreEqual(20, testHand.GetHandValue(), "Jack and King together must equal 20 points.");
        }

        [Test]
        public void HandValue_AceSoftAndHardValueAdjustment() {
            // Initial soft 11 value
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Spades, CardData.Rank.Five));
            Assert.AreEqual(16, testHand.GetHandValue(), "Ace + 5 should evaluate to a soft 16.");

            // Adding a card that would bust causes the Ace to drop down to 1 point
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Ten));
            Assert.AreEqual(16, testHand.GetHandValue(), "Ace should change its value to 1 to prevent a bust, keeping total at 16.");
        }

        [Test]
        public void Hand_DetectsBustCorrectly() {
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Ten));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Jack));
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Five));

            Assert.IsTrue(testHand.IsBust, "A total value of 25 must flag the hand as a bust.");
        }

        [Test]
        public void Hand_DetectsNaturalBlackjack() {
            testHand.Cards.Add(new CardData(CardData.Suit.Spades, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Queen));

            Assert.IsTrue(testHand.IsBlackjack, "A starting hand of Ace and a Queen must be recognized as a natural Blackjack.");
        }
    }
}