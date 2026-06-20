using NUnit.Framework;
using CardFramework.Core.Engines;
using CardFramework.Core.Models;

namespace CardFramework.Tests.EditMode.Core {
    [TestFixture]
    public class SolitaireEngineTests {
        private SolitaireEngine engine;

        [SetUp]
        public void Setup() {
            engine = new SolitaireEngine();
        }

        [Test]
        public void Tableau_AllowsOnlyKingOnEmptyColumn() {
            var king = new CardData(CardData.Suit.Hearts, CardData.Rank.King);
            var queen = new CardData(CardData.Suit.Spades, CardData.Rank.Queen);

            Assert.IsTrue(engine.CanPlaceOnTableau(king, 0), "Empty columns must accept Kings.");
            Assert.IsFalse(engine.CanPlaceOnTableau(queen, 0), "Empty columns must reject any card that is not a King.");
        }

        [Test]
        public void Tableau_ValidatesAlternatingColorAndDescendingRank() {
            var targetColumn = 0;
            var columnCards = engine.GetTableau()[targetColumn];

            // Set up a Red 10 (Hearts) on top of the column
            columnCards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ten));

            var validCard = new CardData(CardData.Suit.Spades, CardData.Rank.Nine);   // Black 9 (Valid)
            var wrongColor = new CardData(CardData.Suit.Diamonds, CardData.Rank.Nine); // Red 9 (Invalid)
            var wrongRank = new CardData(CardData.Suit.Spades, CardData.Rank.Eight);  // Black 8 (Invalid)

            Assert.IsTrue(engine.CanPlaceOnTableau(validCard, targetColumn), "Should allow alternating color and sequential lower rank.");
            Assert.IsFalse(engine.CanPlaceOnTableau(wrongColor, targetColumn), "Should reject identical colors on top of each other.");
            Assert.IsFalse(engine.CanPlaceOnTableau(wrongRank, targetColumn), "Should reject non-sequential ranks.");
        }

        [Test]
        public void Foundation_AllowsOnlyAceOnEmptyStack() {
            int clubsIndex = (int)CardData.Suit.Clubs;
            var aceOfClubs = new CardData(CardData.Suit.Clubs, CardData.Rank.Ace);
            var twoOfClubs = new CardData(CardData.Suit.Clubs, CardData.Rank.Two);

            Assert.IsTrue(engine.CanPlaceOnFoundation(aceOfClubs, clubsIndex), "Foundations must start with an Ace.");
            Assert.IsFalse(engine.CanPlaceOnFoundation(twoOfClubs, clubsIndex), "Foundations must reject higher ranks if Ace is missing.");
        }
    }
}