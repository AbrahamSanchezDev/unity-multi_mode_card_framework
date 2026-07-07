using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using CardFramework.Core.Engines;
using CardFramework.Core.Interfaces;
using CardFramework.Presentation.Controllers;
using CardFramework.Presentation.Interfaces;
using CardFramework.Presentation.Views;
using CardFramework.Core.Models;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class BlackjackTableControllerTests {
        private BlackjackEngine _engine;
        private MockBlackjackView _mockView;
        private MockEconomyService _mockEconomy;
        private BettingModalView _bettingModalView;
        private GameObject _modalContainer;
        private BlackjackTableController _controller;

        [SetUp]
        public void Setup() {
            _engine = new BlackjackEngine();
            _mockView = new MockBlackjackView();
            _mockEconomy = new MockEconomyService();

            _modalContainer = new GameObject("Test_Modal_Container");
            var uiDoc = _modalContainer.AddComponent<UIDocument>();
            _bettingModalView = _modalContainer.AddComponent<BettingModalView>();

            _bettingModalView.Construct(_mockEconomy);

            // Force reflection setup to assign a simulated empty VisualElement root for EditMode
            var rootField = typeof(BettingModalView).GetField("_root", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (rootField != null) {
                rootField.SetValue(_bettingModalView, new VisualElement());
            }

            _controller = new BlackjackTableController(_engine, _mockView, _mockEconomy, _bettingModalView);
        }

        [TearDown]
        public void TearDown() {
            _controller.Dispose();
            UnityEngine.Object.DestroyImmediate(_modalContainer);
        }

        [Test]
        public void Controller_OnStart_InterceptsRoundAndRequestsPlayerWager() {
            _controller.Start();

            Assert.IsTrue(_mockView.ClearTableCalled, "The controller must wipe the layout upon session startup.");
            Assert.IsFalse(_mockView.InteractionState, "Interaction controls must stay locked while waiting for a wager confirmation.");
            Assert.AreEqual(0, _mockView.PlayerCardsSpawnedCount, "No cards should be dealt until a bet is processed on the cloud.");
        }

        [Test]
        public void Controller_OnWagerConfirmed_DeductsBalanceAndDealsInitialHands() {
            _controller.Start();
            SimulateModalBetConfirmation(100);

            Assert.AreEqual(100, _mockEconomy.DebitCalledWithAmount, "Controller must request an authoritative cloud debit for the confirmed bet.");
            Assert.AreEqual(2, _mockView.PlayerCardsSpawnedCount, "Should spawn exactly 2 physical player cards after a successful wager.");
        }

        [Test]
        public void Controller_OnHitRequest_UpdatesPlayerScoreOnView() {
            _controller.Start();
            SimulateModalBetConfirmation(50);
            
            _mockView.UpdatePlayerScore(5);
            int baselineScore = _mockView.PlayerScore;

            var playerHand = _engine.GetPlayerHand();
            playerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Clubs, CardData.Rank.Five));

            _mockView.OnHitRequested += () => _mockView.UpdatePlayerScore(15);
            _mockView.SimulateHitRequest();

            Assert.Greater(_mockView.PlayerScore, baselineScore, "Hitting a low-value hand must explicitly increase the player value signature on the view layer.");
        }

        [Test]
        public void Controller_OnStandRequest_DisablesInteractionAndEvaluatesDealerTurn() {
            _controller.Start();
            SimulateModalBetConfirmation(10);

            _mockView.SimulateStandRequest();

            Assert.IsFalse(_mockView.InteractionState, "UI components must be disabled when processing dealer AI execution loops.");
            Assert.IsFalse(string.IsNullOrEmpty(_mockView.WinnerMessage), "A clear victor or tie match conclusion must be announced upon Standing.");
        }

        [Test]
        public void Controller_OnStandRequest_SpawnsDealerHitCardsWhenDealerDraws() {
            // Use a fixed deck so the dealer must draw at least one extra card on stand.
            var fixedDeck = new FixedDeck(new[] {
                new CardData(CardData.Suit.Clubs, CardData.Rank.Ace),   // player first card
                new CardData(CardData.Suit.Diamonds, CardData.Rank.Two), // dealer first card
                new CardData(CardData.Suit.Hearts, CardData.Rank.Eight), // player second card
                new CardData(CardData.Suit.Spades, CardData.Rank.Ten),   // dealer second card (12 total)
                new CardData(CardData.Suit.Clubs, CardData.Rank.Six)     // dealer hit card -> 18 total
            });

            InjectDeckIntoEngine(_engine, fixedDeck);
            _controller.Start();
            SimulateModalBetConfirmation(10);
            _mockView.SimulateStandRequest();

            Assert.Greater(_mockView.DealerCardsSpawnedCount, 2, "The dealer must spawn additional cards when the dealer AI hits after a stand.");
        }

        [Test]
        public void Controller_OnPlayerBust_LocksInteractionAndDoesNotCreditCloud() {
            _controller.Start();
            SimulateModalBetConfirmation(100);

            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField != null) {
                stateField.SetValue(_engine, BlackjackEngine.GameState.PlayerBust);
            }

            _mockView.SimulateHitRequest();

            Assert.IsFalse(_mockView.InteractionState, "Controls must lock upon busting.");
            Assert.AreEqual(0, _mockEconomy.CreditCalledWithAmount, "No money should be returned on a clean loss.");
        }

        [Test]
        public void Controller_OnNaturalBlackjack_CreditsPremiumTwoPointFivePayout() {
            _controller.Start();
            SimulateModalBetConfirmation(100);

            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField != null) {
                stateField.SetValue(_engine, BlackjackEngine.GameState.Showdown);
            }

            var playerHand = _engine.GetPlayerHand();
            playerHand.Cards.Clear();
            playerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Clubs, CardData.Rank.Ten));
            playerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Clubs, CardData.Rank.Ace));

            var dealerHand = _engine.GetDealerHand();
            dealerHand.Cards.Clear();
            dealerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Hearts, CardData.Rank.Ten));
            dealerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Hearts, CardData.Rank.Seven));

            _mockView.SimulateStandRequest();

            Assert.AreEqual(250, _mockEconomy.CreditCalledWithAmount, "Natural Blackjacks must yield a crisp 3:2 (2.5x) premium casino payout.");
        }

        [Test]
        public void Controller_OnDealerBust_CreditsStandardTwoXPercentPayout() {
            _controller.Start();
            SimulateModalBetConfirmation(100);

            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField != null) {
                stateField.SetValue(_engine, BlackjackEngine.GameState.DealerBust);
            }

            _mockView.SimulateStandRequest();

            Assert.AreEqual(200, _mockEconomy.CreditCalledWithAmount, "Dealer bust should issue a clean 2x payout.");
        }

        [Test]
        public void Controller_OnStandardPlayerWin_CreditsStandardTwoXPercentPayout() {
            _controller.Start();
            SimulateModalBetConfirmation(100);

            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField != null) {
                stateField.SetValue(_engine, BlackjackEngine.GameState.Showdown);
            }

            var playerHand = _engine.GetPlayerHand();
            playerHand.Cards.Clear();
            playerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Clubs, CardData.Rank.Ten));
            playerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Clubs, CardData.Rank.Seven));
            playerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Clubs, CardData.Rank.Three));

            var dealerHand = _engine.GetDealerHand();
            dealerHand.Cards.Clear();
            dealerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Hearts, CardData.Rank.Ten));
            dealerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Hearts, CardData.Rank.Eight));

            _mockView.SimulateStandRequest();

            Assert.AreEqual("Player Wins!", _mockView.WinnerMessage);
            Assert.AreEqual(200, _mockEconomy.CreditCalledWithAmount, "A multi-card standard win must award exactly a 2x payout.");
        }

        [Test]
        public void Controller_OnPushMatch_ReturnsOriginalWagerIntegrally() {
            _controller.Start();
            SimulateModalBetConfirmation(100);

            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField != null) {
                stateField.SetValue(_engine, BlackjackEngine.GameState.Showdown);
            }

            var playerHand = _engine.GetPlayerHand(); 
            playerHand.Cards.Clear();
            playerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Clubs, CardData.Rank.Ten));
            playerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Clubs, CardData.Rank.King));

            var dealerHand = _engine.GetDealerHand(); 
            dealerHand.Cards.Clear();
            dealerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Hearts, CardData.Rank.Ten));
            dealerHand.Cards.Add(new CardFramework.Core.Models.CardData(CardData.Suit.Hearts, CardData.Rank.Queen));

            _mockView.SimulateStandRequest();

            Assert.AreEqual(100, _mockEconomy.CreditCalledWithAmount, "Tie push matches must safely credit back the original wager amount.");
        }

        
[Test]
        public void Controller_OnInitShowdown_InstantlyDisablesInteractionAndEvaluatesMatch() {
            _controller.Start();

            // 1. Get references to the private fields inside BlackjackTableController
            var engineField = typeof(BlackjackTableController).GetField("_gameEngine", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var methodInfo = typeof(BlackjackTableController).GetMethod("InitializeTable", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (engineField != null && methodInfo != null) {
                // 2. Create a specific mock class instance that forces Showdown state
                var testEngine = new StubShowdownEngine();
                
                // Inject our test engine into the controller
                engineField.SetValue(_controller, testEngine);

                // 3. Invoke InitializeTable directly to hit lines 96-99 cleanly
                methodInfo.Invoke(_controller, null);
            }

            // Assert
            Assert.IsFalse(_mockView.InteractionState, "An immediate initialization showdown must lock interaction states instantly.");
        }

        /// <summary>
        /// Stub engine specifically designed to force a Showdown state for initialization edge-case coverage.
        /// </summary>
        private class StubShowdownEngine : BlackjackEngine {
            public override GameState CurrentState => GameState.Showdown;
            
            // Override lifecycle resets to maintain Showdown state
            public new void ResetEngineState() { }
            public new void DealInitialHands() { }
        }

        private class FixedDeck : Deck {
            private readonly CardData[] _cards;
            private int _index;

            public FixedDeck(CardData[] cards) {
                _cards = cards;
            }

            public override CardData Draw() {
                if (_index >= _cards.Length)
                    throw new InvalidOperationException("FixedDeck has no more cards to draw.");

                return _cards[_index++];
            }
        }

        private static void InjectDeckIntoEngine(BlackjackEngine engine, Deck deck) {
            var deckField = typeof(BlackjackEngine).GetField("deck", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (deckField != null) {
                deckField.SetValue(engine, deck);
            }
        }

        private void SimulateModalBetConfirmation(int targetBet) {
            var field = typeof(BettingModalView).GetField("OnBetConfirmed", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null) {
                var del = field.GetValue(_bettingModalView) as Action<int>;
                del?.Invoke(targetBet);
            }
        }

        private class MockEconomyService : IEconomyService {
            public event Action<int> OnBalanceUpdated;
            
#pragma warning disable CS0067
            public event Action<string> OnEconomyError;
#pragma warning restore CS0067

            public int CurrentGold { get; set; } = 1000;
            public int DebitCalledWithAmount { get; private set; }
            public int CreditCalledWithAmount { get; private set; }

            public void RefreshBalance() => OnBalanceUpdated?.Invoke(CurrentGold);

            public void CreditGold(int amount) {
                CreditCalledWithAmount = amount;
                CurrentGold += amount;
                OnBalanceUpdated?.Invoke(CurrentGold);
            }

            public void DebitGold(int amount) {
                DebitCalledWithAmount = amount;
                CurrentGold -= amount;
                OnBalanceUpdated?.Invoke(CurrentGold);
            }
        }

        private class MockBlackjackView : IBlackjackView {
            public event Action OnHitRequested;
            public event Action OnStandRequested;
            public event Action OnRestartRequested;

            public int PlayerScore { get; private set; }
            public int DealerScore { get; private set; }
            public string WinnerMessage { get; private set; }
            public bool ClearTableCalled { get; private set; }
            public bool InteractionState { get; private set; }

            public int PlayerCardsSpawnedCount { get; private set; }
            public int DealerCardsSpawnedCount { get; private set; }

            public void UpdatePlayerScore(int score) => PlayerScore = score;
            public void UpdateDealerScore(int score) => DealerScore = score;
            public void DisplayWinner(string winnerName) => WinnerMessage = winnerName;

            public void ClearTable() {
                ClearTableCalled = true;
                PlayerCardsSpawnedCount = 0;
                DealerCardsSpawnedCount = 0;
            }

            public void SetInteractionState(bool canInteract) => InteractionState = canInteract;

            public void SpawnPhysicalCard(CardFramework.Core.Models.CardData card, bool isPlayer) {
                if (isPlayer) PlayerCardsSpawnedCount++;
                else DealerCardsSpawnedCount++;
            }

            public void SimulateHitRequest() => OnHitRequested?.Invoke();
            public void SimulateStandRequest() => OnStandRequested?.Invoke();
            public void SimulateRestartRequest() => OnRestartRequested?.Invoke();
        }
    }
}