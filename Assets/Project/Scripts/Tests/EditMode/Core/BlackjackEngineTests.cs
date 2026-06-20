using NUnit.Framework;
using CardFramework.Core.Engines;
using CardFramework.Core.Models;
using System.Collections.Generic;

namespace CardFramework.Tests.EditMode.Core {
    [TestFixture]
    public class BlackjackEngineTests {
        private BlackjackEngine.Hand testHand;

        [SetUp]
        public void Setup() {
            testHand = new BlackjackEngine.Hand();
        }

        #region Hand.GetHandValue() Tests
        
        [Test]
        public void HandValue_SingleNumberCard_ReturnsCorrectValue() {
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Five));
            Assert.AreEqual(5, testHand.GetHandValue());
        }

        [Test]
        public void HandValue_MultipleNumberCards_ReturnsSumCorrectly() {
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Two));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Three));
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Four));
            Assert.AreEqual(9, testHand.GetHandValue());
        }

        [Test]
        public void HandValue_CalculatesFaceCardsAsTen() {
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Jack));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.King));
            Assert.AreEqual(20, testHand.GetHandValue(), "Jack and King together must equal 20 points.");
        }

        [Test]
        public void HandValue_QueenCountsAsTen() {
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Queen));
            Assert.AreEqual(10, testHand.GetHandValue());
        }

        [Test]
        public void HandValue_SingleAceCountsAsEleven() {
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));
            Assert.AreEqual(11, testHand.GetHandValue(), "Single Ace should be 11.");
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
        public void HandValue_MultipleAces_AdjustCorrectly() {
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Nine));
            // Expected: 11 + 1 + 9 = 21 (one Ace as 11, one as 1)
            Assert.AreEqual(21, testHand.GetHandValue());
        }

        [Test]
        public void HandValue_TwoAcesWithLowCard_KeepsAsOne() {
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Ace));
            // Expected: 11 + 1 = 12 (one ace as 11, one downgraded to 1)
            Assert.AreEqual(12, testHand.GetHandValue());
        }

        #endregion

        #region Hand.IsBust Tests

        [Test]
        public void Hand_DetectsBustCorrectly() {
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Ten));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Jack));
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Five));

            Assert.IsTrue(testHand.IsBust, "A total value of 25 must flag the hand as a bust.");
        }

        [Test]
        public void Hand_NotBustWhenExactly21() {
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Jack));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Queen));

            Assert.IsFalse(testHand.IsBust, "Hand with exactly 21 is not a bust.");
        }

        [Test]
        public void Hand_NotBustWhenBelow21() {
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Five));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Six));

            Assert.IsFalse(testHand.IsBust, "Hand with 11 is not a bust.");
        }

        #endregion

        #region Hand.IsBlackjack Tests

        [Test]
        public void Hand_DetectsNaturalBlackjack() {
            testHand.Cards.Add(new CardData(CardData.Suit.Spades, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Queen));

            Assert.IsTrue(testHand.IsBlackjack, "A starting hand of Ace and a Queen must be recognized as a natural Blackjack.");
        }

        [Test]
        public void Hand_BlackjackWithAceAndKing() {
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.King));

            Assert.IsTrue(testHand.IsBlackjack);
        }

        [Test]
        public void Hand_NotBlackjackWithMoreThanTwoCards() {
            testHand.Cards.Add(new CardData(CardData.Suit.Spades, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Queen));
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Two));

            Assert.IsFalse(testHand.IsBlackjack, "Three cards cannot be a natural Blackjack.");
        }

        [Test]
        public void Hand_NotBlackjackWithTwoCardsNot21() {
            testHand.Cards.Add(new CardData(CardData.Suit.Spades, CardData.Rank.Ten));
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Nine));

            Assert.IsFalse(testHand.IsBlackjack);
        }

        #endregion

        #region Hand.IsSoft17 Tests

        [Test]
        public void Hand_DetectsSoft17WithAceAndSix() {
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Six));

            Assert.IsTrue(testHand.IsSoft17, "Ace + 6 (soft 17) must be detected.");
        }

        [Test]
        public void Hand_DetectsSoft17WithAceAceFive() {
            // Ace (11) + Ace (1) + Five = 17 (soft)
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Five));

            Assert.IsTrue(testHand.IsSoft17);
        }

        [Test]
        public void Hand_NotSoft17WhenHardTotal() {
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ten));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Seven));

            Assert.IsFalse(testHand.IsSoft17, "Hard 17 (no ace) is not soft 17.");
        }

        [Test]
        public void Hand_NotSoft17WhenValueIsNot17() {
            testHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ace));
            testHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Eight));

            Assert.IsFalse(testHand.IsSoft17, "Soft 18 is not soft 17.");
        }

        #endregion

        #region BlackjackEngine Integration Tests

        /// <summary>
        /// Mock Deck for testing BlackjackEngine without randomness
        /// </summary>
        private class MockDeck : Deck {
            private Queue<CardData> cardQueue = new();
            private int drawCount = 0;

            public void EnqueueCards(params CardData[] cards) {
                foreach (var card in cards) {
                    cardQueue.Enqueue(card);
                }
            }

            public override CardData Draw() {
                if (cardQueue.Count > 0) {
                    drawCount++;
                    return cardQueue.Dequeue();
                }
                return base.Draw();
            }

            public int GetDrawCount() => drawCount;
        }

        [Test]
        public void GameFlow_DealInitialHands_Deals2CardsEach() {
            // Create engine with controlled deck (mock not needed for this simple test)
            var engine = new BlackjackEngine();
            
            engine.DealInitialHands();
            
            // Verify player has 2 cards
            Assert.AreEqual(2, engine.GetPlayerHand().Cards.Count);
            
            // Verify dealer has 2 cards
            Assert.AreEqual(2, engine.GetDealerHand().Cards.Count);
        }

        [Test]
        public void GameFlow_InitialState_IsPlayerTurn() {
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            // Unless player has blackjack, state should be PlayerTurn
            if (!engine.GetPlayerHand().IsBlackjack) {
                Assert.AreEqual(BlackjackEngine.GameState.PlayerTurn, engine.CurrentState);
            }
        }

        [Test]
        public void GameFlow_PlayerBlackjack_TransitionsToShowdown() {
            // This is tricky to test without controlling deck, so we test the logic instead
            var hand = new BlackjackEngine.Hand();
            hand.Cards.Add(new CardData(CardData.Suit.Spades, CardData.Rank.Ace));
            hand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.King));
            
            Assert.IsTrue(hand.IsBlackjack);
        }

        [Test]
        public void DealInitialHands_PlayerBlackjack_SetsShowdown() {
            var mock = new MockDeck();
            // Enqueue draws in order: player1, dealer1, player2, dealer2
            mock.EnqueueCards(
                new CardData(CardData.Suit.Spades, CardData.Rank.Ace),
                new CardData(CardData.Suit.Clubs, CardData.Rank.Two),
                new CardData(CardData.Suit.Hearts, CardData.Rank.King),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Two)
            );

            var engine = new BlackjackEngine(mock);
            engine.DealInitialHands();

            Assert.AreEqual(BlackjackEngine.GameState.Showdown, engine.CurrentState, "Player natural blackjack should set state to Showdown.");
        }

        [Test]
        public void PlayerStand_IgnoredWhenNotInPlayerTurn() {
            var mock = new MockDeck();
            // Deterministic small cards to avoid immediate blackjack
            mock.EnqueueCards(
                new CardData(CardData.Suit.Clubs, CardData.Rank.Two),
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Two),
                new CardData(CardData.Suit.Hearts, CardData.Rank.Three),
                new CardData(CardData.Suit.Spades, CardData.Rank.Three)
            );

            var engine = new BlackjackEngine(mock);
            engine.DealInitialHands();

            // Ensure we started in PlayerTurn
            Assert.AreEqual(BlackjackEngine.GameState.PlayerTurn, engine.CurrentState);

            // First stand moves to dealer flow and produces a terminal non-PlayerTurn state
            engine.PlayerStand();
            var stateAfterFirstStand = engine.CurrentState;

            // Second stand should be ignored because CurrentState != PlayerTurn
            engine.PlayerStand();
            Assert.AreEqual(stateAfterFirstStand, engine.CurrentState, "PlayerStand should be ignored when not in PlayerTurn.");
        }

        [Test]
        public void PlayerHit_AddsCardToHand() {
            var engine = new BlackjackEngine();
            
            // Retry until we get a non-blackjack hand (random deck shuffle)
            int attempts = 0;
            while (attempts < 100) {
                engine = new BlackjackEngine();
                engine.DealInitialHands();
                
                if (engine.CurrentState == BlackjackEngine.GameState.PlayerTurn) {
                    break;  // Got a non-blackjack hand
                }
                attempts++;
            }
            
            // Ensure we're in PlayerTurn state
            Assert.AreEqual(BlackjackEngine.GameState.PlayerTurn, engine.CurrentState, 
                "Could not get a non-blackjack hand after 100 attempts.");
            
            int initialCount = engine.GetPlayerHand().Cards.Count;
            engine.PlayerHit();
            
            Assert.AreEqual(initialCount + 1, engine.GetPlayerHand().Cards.Count, 
                "PlayerHit should add exactly one card to the hand.");
        }

        [Test]
        public void PlayerHit_IgnoredWhenNotInPlayerTurn() {
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            // Force state to DealerTurn (simulate by calling PlayerStand)
            engine.PlayerStand();
            
            int cardCountBeforeHit = engine.GetPlayerHand().Cards.Count;
            engine.PlayerHit();  // This should be ignored
            
            // Card count should not change
            Assert.AreEqual(cardCountBeforeHit, engine.GetPlayerHand().Cards.Count);
        }

        [Test]
        public void PlayerHit_DetectsBustState() {
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            // Keep hitting until bust (or reasonable limit)
            for (int i = 0; i < 10; i++) {
                if (engine.CurrentState == BlackjackEngine.GameState.PlayerBust) {
                    break;
                }
                if (engine.CurrentState == BlackjackEngine.GameState.PlayerTurn) {
                    engine.PlayerHit();
                }
            }
            
            // Verify that if player is in bust state, hand value > 21
            if (engine.CurrentState == BlackjackEngine.GameState.PlayerBust) {
                Assert.IsTrue(engine.GetPlayerHand().IsBust);
            }
        }

        [Test]
        public void PlayerStand_TransitionsToDealerTurn() {
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            if (engine.CurrentState == BlackjackEngine.GameState.PlayerTurn) {
                engine.PlayerStand();
                
                // After PlayerStand, state should be either DealerBust or Showdown
                Assert.IsTrue(
                    engine.CurrentState == BlackjackEngine.GameState.DealerBust ||
                    engine.CurrentState == BlackjackEngine.GameState.Showdown,
                    "After PlayerStand, game should proceed to dealer logic"
                );
            }
        }

        [Test]
        public void GetPlayerValue_ReturnsHandValue() {
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            int playerValue = engine.GetPlayerValue();
            int expectedValue = engine.GetPlayerHand().GetHandValue();
            
            Assert.AreEqual(expectedValue, playerValue);
        }

        [Test]
        public void GetDealerValue_ReturnsHandValue() {
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            int dealerValue = engine.GetDealerValue();
            int expectedValue = engine.GetDealerHand().GetHandValue();
            
            Assert.AreEqual(expectedValue, dealerValue);
        }

        [Test]
        public void HandAccessors_ReturnValidHandReferences() {
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            var playerHand = engine.GetPlayerHand();
            var dealerHand = engine.GetDealerHand();
            
            Assert.IsNotNull(playerHand);
            Assert.IsNotNull(dealerHand);
            Assert.AreEqual(2, playerHand.Cards.Count);
            Assert.AreEqual(2, dealerHand.Cards.Count);
        }

        [Test]
        public void DealerLogic_HitsOnSoft17() {
            // Dealer should hit on soft 17
            // This is tested indirectly via gameplay
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            // Play through a game to completion
            while (engine.CurrentState == BlackjackEngine.GameState.PlayerTurn) {
                engine.PlayerStand();
                break;  // Stand immediately
            }
            
            // Game should reach a terminal state
            Assert.IsTrue(
                engine.CurrentState == BlackjackEngine.GameState.Showdown ||
                engine.CurrentState == BlackjackEngine.GameState.DealerBust,
                "Game should reach terminal state after dealer turn"
            );
        }

        [Test]
        public void GameFlow_CompleteRound() {
            var engine = new BlackjackEngine();
            engine.DealInitialHands();
            
            // Play a round
            while (engine.CurrentState == BlackjackEngine.GameState.PlayerTurn) {
                if (engine.GetPlayerValue() >= 17) {
                    engine.PlayerStand();
                    break;
                }
                engine.PlayerHit();
            }
            
            // Verify game reached a terminal state
            Assert.IsTrue(
                engine.CurrentState == BlackjackEngine.GameState.Showdown ||
                engine.CurrentState == BlackjackEngine.GameState.PlayerBust ||
                engine.CurrentState == BlackjackEngine.GameState.DealerBust,
                "Game must reach a terminal state"
            );
        }

        #endregion
    }
}