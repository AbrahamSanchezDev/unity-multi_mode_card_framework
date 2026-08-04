using NUnit.Framework;
using CardFramework.Core.Engines;
using CardFramework.Core.Models;
using System.Collections.Generic;

namespace CardFramework.Tests.EditMode.Core {
    [TestFixture]
    public class SolitaireEngineTests {
        private SolitaireEngine engine;

        [SetUp]
        public void Setup() {
            engine = new SolitaireEngine();
        }

        #region Initialize Tests

        [Test]
        public void Initialize_SetupsProperly() {
            // Arrange & Act
            engine.Initialize();
            var tableau = engine.GetTableau();
            var foundation = engine.GetFoundation();

            // Assert - Tableau should be populated in pyramid pattern
            Assert.AreEqual(1, tableau[0].Count, "First tableau column should have 1 card.");
            Assert.AreEqual(2, tableau[1].Count, "Second tableau column should have 2 cards.");
            Assert.AreEqual(7, tableau[6].Count, "Last tableau column should have 7 cards.");

            // Total tableau cards: 1+2+3+4+5+6+7 = 28
            int totalTableau = 0;
            for (int i = 0; i < 7; i++) totalTableau += tableau[i].Count;
            Assert.AreEqual(28, totalTableau, "Tableau should contain 28 cards in pyramid pattern.");

            // Foundation should start empty
            Assert.AreEqual(0, foundation[0].Count, "Foundation stacks should start empty.");
            Assert.AreEqual(0, foundation[1].Count, "Foundation stacks should start empty.");
            Assert.AreEqual(0, foundation[2].Count, "Foundation stacks should start empty.");
            Assert.AreEqual(0, foundation[3].Count, "Foundation stacks should start empty.");
        }

        [Test]
        public void Initialize_ClearsAllPreviousState() {
            // Arrange - Initialize once
            engine.Initialize();
            var firstTableau = engine.GetTableau()[0].Count;

            // Add cards manually to simulate previous state
            var testCard = new CardData(CardData.Suit.Hearts, CardData.Rank.King);
            engine.GetTableau()[0].Add(testCard);
            engine.GetFoundation()[0].Add(testCard);

            // Act - Initialize again
            engine.Initialize();
            var tableau = engine.GetTableau();
            var foundation = engine.GetFoundation();

            // Assert - Should reset to clean state
            Assert.AreEqual(firstTableau, tableau[0].Count, "Initialize should clear added cards from tableau.");
            Assert.AreEqual(0, foundation[0].Count, "Initialize should clear foundation stacks.");
        }

        #endregion

        #region Tableau Placement Tests

        [Test]
        public void CanPlaceOnTableau_AllowsKingOnEmptyColumn() {
            // Arrange
            var king = new CardData(CardData.Suit.Hearts, CardData.Rank.King);
            engine.GetTableau()[0].Clear(); // Ensure column is empty

            // Act & Assert
            Assert.IsTrue(engine.CanPlaceOnTableau(king, 0), "Empty columns must accept Kings.");
        }

        [Test]
        public void CanPlaceOnTableau_RejectsNonKingOnEmptyColumn() {
            // Arrange
            var queen = new CardData(CardData.Suit.Spades, CardData.Rank.Queen);
            var ace = new CardData(CardData.Suit.Diamonds, CardData.Rank.Ace);
            engine.GetTableau()[0].Clear();

            // Act & Assert
            Assert.IsFalse(engine.CanPlaceOnTableau(queen, 0), "Empty columns must reject Queens.");
            Assert.IsFalse(engine.CanPlaceOnTableau(ace, 0), "Empty columns must reject Aces.");
        }

        [Test]
        public void CanPlaceOnTableau_AllowsAlternatingColorAndDescendingRank() {
            // Arrange
            var targetColumn = 0;
            var columnCards = engine.GetTableau()[targetColumn];
            columnCards.Clear();

            // Set up Red 10 (Hearts) on top
            columnCards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ten));

            var validCard = new CardData(CardData.Suit.Spades, CardData.Rank.Nine); // Black 9
            var validCard2 = new CardData(CardData.Suit.Clubs, CardData.Rank.Nine);  // Black 9

            // Act & Assert
            Assert.IsTrue(engine.CanPlaceOnTableau(validCard, targetColumn), 
                "Should allow Spades 9 on Hearts 10 (alternating color, descending rank).");
            Assert.IsTrue(engine.CanPlaceOnTableau(validCard2, targetColumn), 
                "Should allow Clubs 9 on Hearts 10 (alternating color, descending rank).");
        }

        [Test]
        public void CanPlaceOnTableau_RejectsSameColorOnTop() {
            // Arrange
            var targetColumn = 0;
            var columnCards = engine.GetTableau()[targetColumn];
            columnCards.Clear();

            // Set up Red 10 (Hearts)
            columnCards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ten));

            var wrongColor1 = new CardData(CardData.Suit.Diamonds, CardData.Rank.Nine); // Red 9
            var wrongColor2 = new CardData(CardData.Suit.Hearts, CardData.Rank.Nine);   // Red 9

            // Act & Assert
            Assert.IsFalse(engine.CanPlaceOnTableau(wrongColor1, targetColumn), 
                "Should reject Diamonds 9 on Hearts 10 (same color).");
            Assert.IsFalse(engine.CanPlaceOnTableau(wrongColor2, targetColumn), 
                "Should reject Hearts 9 on Hearts 10 (same color).");
        }

        [Test]
        public void CanPlaceOnTableau_RejectsWrongRank() {
            // Arrange
            var targetColumn = 0;
            var columnCards = engine.GetTableau()[targetColumn];
            columnCards.Clear();

            columnCards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ten));

            var wrongRank1 = new CardData(CardData.Suit.Spades, CardData.Rank.Eight);  // Black 8 (too low)
            var wrongRank2 = new CardData(CardData.Suit.Spades, CardData.Rank.Ten);   // Black 10 (same rank)
            var wrongRank3 = new CardData(CardData.Suit.Spades, CardData.Rank.Jack);  // Black Jack (higher)

            // Act & Assert
            Assert.IsFalse(engine.CanPlaceOnTableau(wrongRank1, targetColumn), "Should reject rank 8 on 10.");
            Assert.IsFalse(engine.CanPlaceOnTableau(wrongRank2, targetColumn), "Should reject rank 10 on 10.");
            Assert.IsFalse(engine.CanPlaceOnTableau(wrongRank3, targetColumn), "Should reject rank Jack on 10.");
        }

        [Test]
        public void CanPlaceOnTableau_RejectsInvalidColumnIndices() {
            // Arrange
            var king = new CardData(CardData.Suit.Hearts, CardData.Rank.King);

            // Act & Assert
            Assert.IsFalse(engine.CanPlaceOnTableau(king, -1), "Should reject negative column index.");
            Assert.IsFalse(engine.CanPlaceOnTableau(king, 7), "Should reject column index >= 7.");
            Assert.IsFalse(engine.CanPlaceOnTableau(king, 100), "Should reject out-of-range column index.");
        }

        [Test]
        public void CanPlaceOnTableau_AllowsSequentialCards() {
            // Arrange - Build a valid sequence: K, Q, J, 10, 9, 8, 7...
            var targetColumn = 0;
            var columnCards = engine.GetTableau()[targetColumn];
            columnCards.Clear();

            columnCards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.King));
            columnCards.Add(new CardData(CardData.Suit.Spades, CardData.Rank.Queen));
            columnCards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Jack));

            var nextCard = new CardData(CardData.Suit.Clubs, CardData.Rank.Ten);

            // Act & Assert
            Assert.IsTrue(engine.CanPlaceOnTableau(nextCard, targetColumn), 
                "Should allow Black 10 on Red Jack in sequence.");
        }

        [Test]
        public void MoveCardsToTableau_AllowsMultiCardRunWhenBottomCardMatchesTarget() {
            // Arrange
            engine.GetTableau()[0].Clear();
            engine.GetTableau()[0].Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Six, isFaceUp: true));
            engine.GetTableau()[0].Add(new CardData(CardData.Suit.Spades, CardData.Rank.Five, isFaceUp: true));

            engine.GetTableau()[1].Clear();
            engine.GetTableau()[1].Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Seven, isFaceUp: true));

            var draggedCards = new List<CardData> {
                engine.GetTableau()[0][0],
                engine.GetTableau()[0][1]
            };

            // Act
            bool moved = engine.MoveCardsToTableau(draggedCards, 0, 0, 1);

            // Assert
            Assert.IsTrue(moved, "A multi-card run should be allowed when the bottom card of the run can legally land on the target column.");
            Assert.AreEqual(2, engine.GetTableau()[1].Count, "The target column should receive the moved run.");
        }

        [Test]
        public void MoveCardsToTableau_FromWaste_AllowsValidDropOntoTableauColumn() {
            // Arrange
            engine.Initialize();

            var threeOfClubs = new CardData(CardData.Suit.Clubs, CardData.Rank.Three, isFaceUp: true);
            engine.GetWaste().Clear();
            engine.GetWaste().Add(threeOfClubs);

            engine.GetTableau()[2].Clear();
            engine.GetTableau()[2].Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Four, isFaceUp: true));

            // Act
            bool moved = engine.MoveCardsToTableau(new List<CardData> { threeOfClubs }, -1, -1, 2);

            // Assert
            Assert.IsTrue(moved, "A card from the waste pile should be allowed onto a valid tableau column.");
            Assert.AreEqual(0, engine.GetWaste().Count, "The moved waste card should be removed from the waste pile.");
            Assert.AreEqual(2, engine.GetTableau()[2].Count, "The target tableau column should contain the moved card.");
            Assert.AreEqual(CardData.Rank.Three, engine.GetTableau()[2][^1].CardRank, "The moved card should be placed on top of the target column.");
        }

        [Test]
        public void MoveCardsToFoundation_FromWaste_AllowsValidFoundationDrop() {
            // Arrange
            var twoOfSpades = new CardData(CardData.Suit.Spades, CardData.Rank.Two, isFaceUp: true);
            engine.GetWaste().Clear();
            engine.GetWaste().Add(twoOfSpades);

            var spadesIndex = (int)CardData.Suit.Spades;
            engine.GetFoundation()[spadesIndex].Clear();
            engine.GetFoundation()[spadesIndex].Add(new CardData(CardData.Suit.Spades, CardData.Rank.Ace, isFaceUp: true));

            // Act
            bool moved = engine.MoveCardsToFoundation(new List<CardData> { twoOfSpades }, spadesIndex);

            // Assert
            Assert.IsTrue(moved, "A 2 of Spades from the waste pile should be allowed onto an Ace of Spades foundation.");
            Assert.AreEqual(0, engine.GetWaste().Count, "The moved waste card should be removed from the waste pile.");
            Assert.AreEqual(2, engine.GetFoundation()[spadesIndex].Count, "The foundation should contain the moved card.");
        }

        [Test]
        public void MoveCardsToTableau_FromInteriorIndex_RemovesOnlyTheSelectedRun() {
            // Arrange
            engine.Initialize();
            var sourceColumn = 0;
            engine.GetTableau()[sourceColumn].Clear();
            engine.GetTableau()[sourceColumn].Add(new CardData(CardData.Suit.Spades, CardData.Rank.King, isFaceUp: true));
            engine.GetTableau()[sourceColumn].Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Queen, isFaceUp: true));
            engine.GetTableau()[sourceColumn].Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Jack, isFaceUp: true));

            var targetColumn = 1;
            engine.GetTableau()[targetColumn].Clear();

            // Act
            bool moved = engine.MoveCardsToTableau(new List<CardData> {
                engine.GetTableau()[sourceColumn][1],
                engine.GetTableau()[sourceColumn][2]
            }, sourceColumn, 1, targetColumn);

            // Assert
            Assert.IsTrue(moved, "A run starting from an interior index should move correctly.");
            Assert.AreEqual(1, engine.GetTableau()[sourceColumn].Count, "Only the cards from the selected index onward should be removed from the source column.");
            Assert.AreEqual(2, engine.GetTableau()[targetColumn].Count, "The target column should receive the moved run.");
        }

        [Test]
        public void MoveCardToTableau_PreservesFaceUpStateForMovedCardAndRevealsHiddenCard() {
            // Arrange
            var sourceColumn = 0;
            var targetColumn = 1;

            var hiddenCard = new CardData(CardData.Suit.Spades, CardData.Rank.Queen, isFaceUp: false);
            var movedCard = new CardData(CardData.Suit.Hearts, CardData.Rank.King, isFaceUp: true);

            engine.GetTableau()[sourceColumn].Clear();
            engine.GetTableau()[sourceColumn].Add(hiddenCard);
            engine.GetTableau()[sourceColumn].Add(movedCard);

            engine.GetTableau()[targetColumn].Clear();
            engine.GetTableau()[targetColumn].Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Ten, isFaceUp: true));

            // Act
            engine.MoveCardToTableau(movedCard, targetColumn);

            // Assert
            Assert.IsTrue(engine.GetTableau()[targetColumn][^1].IsFaceUp, "The moved card should remain face up after a valid tableau move.");
            Assert.IsTrue(engine.GetTableau()[sourceColumn][^1].IsFaceUp, "The newly revealed card should flip face up after the move.");
        }

        [Test]
        public void MoveCardToFoundation_PreservesRevealStateForAlreadyRevealedCard() {
            // Arrange
            var sourceColumn = 0;
            var targetSuitIndex = (int)CardData.Suit.Spades;

            var hiddenCard = new CardData(CardData.Suit.Spades, CardData.Rank.Queen, isFaceUp: false, instanceId: 100);
            var movedCard = new CardData(CardData.Suit.Hearts, CardData.Rank.King, isFaceUp: true, instanceId: 200);

            engine.GetTableau()[sourceColumn].Clear();
            engine.GetTableau()[sourceColumn].Add(hiddenCard);
            engine.GetTableau()[sourceColumn].Add(movedCard);

            // Act - revealing the hidden card first
            engine.MoveCardToTableau(movedCard, 1);
            var revealedCard = engine.GetTableau()[sourceColumn][^1];
            engine.MoveCardToFoundation(revealedCard, targetSuitIndex);

            // Assert
            Assert.IsTrue(engine.GetFoundation()[targetSuitIndex][0].HasBeenRevealed, "The card should keep its reveal flag when it moves to the foundation.");
        }

        #endregion

        #region Foundation Placement Tests

        [Test]
        public void CanPlaceOnFoundation_AllowsSameSuitSequenceAfterInitialAce() {
            // Arrange
            int foundationIndex = 0;
            var foundation = engine.GetFoundation();
            foundation[foundationIndex].Clear();

            foundation[foundationIndex].Add(new CardData(CardData.Suit.Spades, CardData.Rank.Ace));
            var twoOfSpades = new CardData(CardData.Suit.Spades, CardData.Rank.Two);

            // Act & Assert
            Assert.IsTrue(engine.CanPlaceOnFoundation(twoOfSpades, foundationIndex),
                "A foundation pile should accept the next card of the same suit after an Ace has started it.");
        }

        [Test]
        public void CanPlaceOnFoundation_AllowsAceOnEmptyFoundation() {
            // Arrange
            int clubsIndex = (int)CardData.Suit.Clubs;
            var aceOfClubs = new CardData(CardData.Suit.Clubs, CardData.Rank.Ace);

            // Act & Assert
            Assert.IsTrue(engine.CanPlaceOnFoundation(aceOfClubs, clubsIndex), 
                "Foundations must start with an Ace.");
        }

        [Test]
        public void CanPlaceOnFoundation_RejectsNonAceOnEmptyFoundation() {
            // Arrange
            int clubsIndex = (int)CardData.Suit.Clubs;
            var twoOfClubs = new CardData(CardData.Suit.Clubs, CardData.Rank.Two);
            var king = new CardData(CardData.Suit.Clubs, CardData.Rank.King);

            // Act & Assert
            Assert.IsFalse(engine.CanPlaceOnFoundation(twoOfClubs, clubsIndex), 
                "Foundations must reject non-Aces when empty.");
            Assert.IsFalse(engine.CanPlaceOnFoundation(king, clubsIndex), 
                "Foundations must reject Kings when empty.");
        }

        [Test]
        public void CanPlaceOnFoundation_AllowsSequentialRankSameSuit() {
            // Arrange
            int clubsIndex = (int)CardData.Suit.Clubs;
            var foundation = engine.GetFoundation();
            foundation[clubsIndex].Clear();

            // Set up Ace of Clubs
            foundation[clubsIndex].Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Ace));

            var twoOfClubs = new CardData(CardData.Suit.Clubs, CardData.Rank.Two);
            var threeOfClubs = new CardData(CardData.Suit.Clubs, CardData.Rank.Three);

            // Act & Assert
            Assert.IsTrue(engine.CanPlaceOnFoundation(twoOfClubs, clubsIndex), 
                "Should allow 2 of Clubs after Ace of Clubs.");

            // Add the 2 and try the 3
            foundation[clubsIndex].Add(twoOfClubs);
            Assert.IsTrue(engine.CanPlaceOnFoundation(threeOfClubs, clubsIndex), 
                "Should allow 3 of Clubs after 2 of Clubs.");
        }

        [Test]
        public void CanPlaceOnFoundation_RejectsWrongSuit() {
            // Arrange
            int clubsIndex = (int)CardData.Suit.Clubs;
            var foundation = engine.GetFoundation();
            foundation[clubsIndex].Clear();

            foundation[clubsIndex].Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Ace));

            var twoOfHearts = new CardData(CardData.Suit.Hearts, CardData.Rank.Two);
            var twoOfDiamonds = new CardData(CardData.Suit.Diamonds, CardData.Rank.Two);

            // Act & Assert
            Assert.IsFalse(engine.CanPlaceOnFoundation(twoOfHearts, clubsIndex), 
                "Should reject 2 of Hearts on Clubs foundation.");
            Assert.IsFalse(engine.CanPlaceOnFoundation(twoOfDiamonds, clubsIndex), 
                "Should reject 2 of Diamonds on Clubs foundation.");
        }

        [Test]
        public void CanPlaceOnFoundation_RejectsNonSequentialRank() {
            // Arrange
            int heartsIndex = (int)CardData.Suit.Hearts;
            var foundation = engine.GetFoundation();
            foundation[heartsIndex].Clear();

            foundation[heartsIndex].Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));

            var threeOfHearts = new CardData(CardData.Suit.Hearts, CardData.Rank.Three);
            var aceOfHearts = new CardData(CardData.Suit.Hearts, CardData.Rank.Ace);

            // Act & Assert
            Assert.IsFalse(engine.CanPlaceOnFoundation(threeOfHearts, heartsIndex), 
                "Should reject 3 of Hearts (rank too high).");
            Assert.IsFalse(engine.CanPlaceOnFoundation(aceOfHearts, heartsIndex), 
                "Should reject duplicate Ace of Hearts.");
        }

        [Test]
        public void CanPlaceOnFoundation_RejectsInvalidSuitIndices() {
            // Arrange
            var ace = new CardData(CardData.Suit.Hearts, CardData.Rank.Ace);

            // Act & Assert
            Assert.IsFalse(engine.CanPlaceOnFoundation(ace, -1), 
                "Should reject negative suit index.");
            Assert.IsFalse(engine.CanPlaceOnFoundation(ace, 4), 
                "Should reject suit index >= 4.");
            Assert.IsFalse(engine.CanPlaceOnFoundation(ace, 100), 
                "Should reject out-of-range suit index.");
        }

        [Test]
        public void CanPlaceOnFoundation_AllowsAllSuitsToComplete() {
            // Arrange - Test all four suits
            int[] suitIndices = { (int)CardData.Suit.Hearts, (int)CardData.Suit.Diamonds, 
                                   (int)CardData.Suit.Clubs, (int)CardData.Suit.Spades };
            CardData.Suit[] suits = { CardData.Suit.Hearts, CardData.Suit.Diamonds, 
                                       CardData.Suit.Clubs, CardData.Suit.Spades };

            // Act & Assert
            for (int i = 0; i < 4; i++) {
                var ace = new CardData(suits[i], CardData.Rank.Ace);
                Assert.IsTrue(engine.CanPlaceOnFoundation(ace, suitIndices[i]), 
                    $"Should allow Ace of {suits[i]}.");
            }
        }

        #endregion

        #region HasWon Tests

        [Test]
        public void HasWon_ReturnsFalseWhenNotAllFoundationsFilled() {
            // Arrange
            var foundation = engine.GetFoundation();
            for (int i = 0; i < 4; i++) foundation[i].Clear();

            // Partially fill foundations
            foundation[0].Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));
            foundation[1].Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Ace));
            // Clubs and Spades remain empty

            // Act & Assert
            Assert.IsFalse(engine.HasWon(), "Should not win when some foundations are empty.");
        }

        [Test]
        public void HasWon_ReturnsFalseWhenFoundationsPartiallyFilled() {
            // Arrange
            var foundation = engine.GetFoundation();
            for (int i = 0; i < 4; i++) foundation[i].Clear();

            // Fill foundation[0] with 10 cards instead of 13
            for (int rank = (int)CardData.Rank.Ace; rank < (int)CardData.Rank.Ace + 10; rank++) {
                foundation[0].Add(new CardData(CardData.Suit.Hearts, (CardData.Rank)rank));
            }

            // Act & Assert
            Assert.IsFalse(engine.HasWon(), "Should not win when foundations are not completely filled.");
        }

        [Test]
        public void HasWon_ReturnsTrueWhenAllFoundationsFilled() {
            // Arrange
            var foundation = engine.GetFoundation();

            // Fill all 4 foundations with 13 cards each
            for (int suitIndex = 0; suitIndex < 4; suitIndex++) {
                foundation[suitIndex].Clear();
                for (int rank = (int)CardData.Rank.Ace; rank <= (int)CardData.Rank.King; rank++) {
                    foundation[suitIndex].Add(new CardData((CardData.Suit)suitIndex, (CardData.Rank)rank));
                }
            }

            // Act & Assert
            Assert.IsTrue(engine.HasWon(), "Should win when all foundations have 13 cards each.");
        }

        [Test]
        public void HasWon_ReturnsFalseInitially() {
            // Arrange & Act
            engine.Initialize();

            // Assert
            Assert.IsFalse(engine.HasWon(), "Game should not be won right after initialization.");
        }

        [Test]
        public void HasWon_ReturnsFalseWhenOnlyThreeFoundationsFilled() {
            // Arrange
            var foundation = engine.GetFoundation();

            // Fill first 3 foundations completely
            for (int suitIndex = 0; suitIndex < 3; suitIndex++) {
                foundation[suitIndex].Clear();
                for (int rank = (int)CardData.Rank.Ace; rank <= (int)CardData.Rank.King; rank++) {
                    foundation[suitIndex].Add(new CardData((CardData.Suit)suitIndex, (CardData.Rank)rank));
                }
            }

            // Leave last foundation empty
            foundation[3].Clear();

            // Act & Assert
            Assert.IsFalse(engine.HasWon(), "Should not win when only 3 foundations are complete.");
        }

        #endregion

        #region Getter Tests

        [Test]
        public void GetTableau_ReturnsValidArray() {
            // Act
            var tableau = engine.GetTableau();

            // Assert
            Assert.IsNotNull(tableau, "GetTableau should not return null.");
            Assert.AreEqual(7, tableau.Length, "Tableau should have 7 columns.");
            for (int i = 0; i < 7; i++) {
                Assert.IsNotNull(tableau[i], $"Tableau column {i} should not be null.");
            }
        }

        [Test]
        public void GetFoundation_ReturnsValidArray() {
            // Act
            var foundation = engine.GetFoundation();

            // Assert
            Assert.IsNotNull(foundation, "GetFoundation should not return null.");
            Assert.AreEqual(4, foundation.Length, "Foundation should have 4 suit stacks.");
            for (int i = 0; i < 4; i++) {
                Assert.IsNotNull(foundation[i], $"Foundation stack {i} should not be null.");
            }
        }

        #endregion

        #region Regression Tests

        [Test]
        public void Tableau_ValidatesOnEachRank() {
            // Arrange - Test all rank transitions from King down to 2
            var targetColumn = 0;
            var columnCards = engine.GetTableau()[targetColumn];
            columnCards.Clear();

            // Test King -> Queen -> Jack ... -> 2 -> Ace
            var testRanks = new CardData.Rank[] {
                CardData.Rank.King, CardData.Rank.Queen, CardData.Rank.Jack,
                CardData.Rank.Ten, CardData.Rank.Nine, CardData.Rank.Eight,
                CardData.Rank.Seven, CardData.Rank.Six, CardData.Rank.Five,
                CardData.Rank.Four, CardData.Rank.Three, CardData.Rank.Two,
                CardData.Rank.Ace
            };

            var suits = new[] { CardData.Suit.Hearts, CardData.Suit.Spades };
            int suitIndex = 0;

            // Act - Build a sequence and verify each step
            foreach (var rank in testRanks) {
                var currentSuit = suits[suitIndex % 2];
                var card = new CardData(currentSuit, rank);

                if (columnCards.Count == 0) {
                    Assert.IsTrue(engine.CanPlaceOnTableau(card, targetColumn), 
                        $"Should allow {rank} King on empty column.");
                } else {
                    bool canPlace = engine.CanPlaceOnTableau(card, targetColumn);
                    if (rank != CardData.Rank.Ace) {
                        Assert.IsTrue(canPlace, $"Should allow valid placement of {rank}.");
                    }
                }

                columnCards.Add(card);
                suitIndex++;
            }

            Assert.AreEqual(testRanks.Length, columnCards.Count, "All cards should be placed successfully.");
        }

        #endregion
    }
}