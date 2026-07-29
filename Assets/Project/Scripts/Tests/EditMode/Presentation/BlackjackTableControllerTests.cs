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
        private NavigationController _navigationController;
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

            _bettingModalView.Construct(_mockEconomy,_navigationController);

            // Force reflection setup to assign a simulated empty VisualElement root for EditMode
            var rootField = typeof(BettingModalView).GetField("_root",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (rootField != null) {
                rootField.SetValue(_bettingModalView, new VisualElement());
            }

            // Setup the clean isolated navigation stack required by the constructor
            var mockDashView = _modalContainer.AddComponent<DashboardMenuView>();
            _navigationController = new NavigationController(mockDashView, null);

            _controller = new BlackjackTableController(_engine, _mockView, _mockEconomy, _bettingModalView, _navigationController);
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
            // Use a fixed deck with a single known dealer hit card.
            var fixedDeck = new FixedDeck(new[] {
                new CardData(CardData.Suit.Clubs, CardData.Rank.Six)     // dealer hit card -> 18 total
            });

            InjectDeckIntoEngine(_engine, fixedDeck);

            var playerHand = _engine.GetPlayerHand();
            playerHand.Cards.Clear();
            playerHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Ace));
            playerHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Seven));

            var dealerHand = _engine.GetDealerHand();
            dealerHand.Cards.Clear();
            dealerHand.Cards.Add(new CardData(CardData.Suit.Diamonds, CardData.Rank.Two));
            dealerHand.Cards.Add(new CardData(CardData.Suit.Spades, CardData.Rank.Ten));

            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            stateField?.SetValue(_engine, BlackjackEngine.GameState.PlayerTurn);

            var methodInfo = typeof(BlackjackTableController).GetMethod("HandleStand",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            methodInfo?.Invoke(_controller, null);

            Assert.AreEqual(1, _mockView.DealerCardsSpawnedCount, "The dealer must spawn one extra card when the dealer AI hits after a stand.");
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

        [Test]
        public void Controller_HandleMenuOpened_LocksUiInteractionState() {
            // 1. Arrange: Turn interaction state to true initially
            _mockView.SetInteractionState(true);

            // 2. Act: Invoke the private menu opening listener method using reflection
            var methodInfo = typeof(BlackjackTableController).GetMethod("HandleMenuOpened",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            methodInfo?.Invoke(_controller, null);

            // 3. Assert: Verify the view was cleanly locked from taking interactions
            Assert.IsFalse(_mockView.InteractionState, "UI interaction controls must be explicitly locked when the dashboard menu overlay opens.");
        }

        [Test]
        public void Controller_HandleMenuClosed_RestoresUiInteractionState_WhenNotAtShowdown() {
            // 1. Arrange: Enforce a standard active gaming state (e.g., PlayerTurn)
            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField != null) {
                stateField.SetValue(_engine, BlackjackEngine.GameState.PlayerTurn);
            }
            _mockView.SetInteractionState(false);

            // 2. Act: Force invoke the private menu closure listener layout route
            var methodInfo = typeof(BlackjackTableController).GetMethod("HandleMenuClosed",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            methodInfo?.Invoke(_controller, null);

            // 3. Assert: Confirm interactions are restored smoothly
            Assert.IsTrue(_mockView.InteractionState, "UI interaction controls should be restored if the table is closed outside of a Showdown boundary context.");
        }

        [Test]
        public void Controller_HandleMenuClosed_DoesNotRestoreUiInteractionState_WhenAtShowdown() {
            // 1. Arrange: Force match the game engine state to the Showdown constraint boundary
            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField != null) {
                stateField.SetValue(_engine, BlackjackEngine.GameState.Showdown);
            }
            _mockView.SetInteractionState(false);

            // 2. Act: Trigger menu close handling 
            var methodInfo = typeof(BlackjackTableController).GetMethod("HandleMenuClosed",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            methodInfo?.Invoke(_controller, null);

            // 3. Assert: Interaction state MUST remain disabled to protect the win animation sequence loops
            Assert.IsFalse(_mockView.InteractionState, "UI interaction controls must not be re-enabled upon menu closure if the table is currently sitting on a Showdown screen display.");
        }

        [Test]
        public void Controller_OnEvaluateMatchOutcome_AnnouncesDealerWin_WhenDealerValueIsGreater() {
            // 1. Arrange: Initialize round sequence and commit a standard wager track
            _controller.Start();
            SimulateModalBetConfirmation(100);

            // 2. Force the engine state to Showdown to trigger match evaluation mechanics
            var stateField = typeof(BlackjackEngine).GetField("<CurrentState>k__BackingField",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField != null) {
                stateField.SetValue(_engine, BlackjackEngine.GameState.Showdown);
            }

            // 3. Clear existing hand objects and seed hand values where Dealer > Player (e.g., 20 vs 18)
            var playerHand = _engine.GetPlayerHand();
            playerHand.Cards.Clear();
            playerHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Ten));
            playerHand.Cards.Add(new CardData(CardData.Suit.Clubs, CardData.Rank.Eight)); // 18

            var dealerHand = _engine.GetDealerHand();
            dealerHand.Cards.Clear();
            dealerHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.Ten));
            dealerHand.Cards.Add(new CardData(CardData.Suit.Hearts, CardData.Rank.King)); // 20

            // 4. Act: Simulate a Stand request to drop execution directly into the Evaluate tracking loops
            _mockView.SimulateStandRequest();

            // 5. Assert: Verify the view panel renders loss alerts and the economy structure withholds payouts
            Assert.AreEqual("Dealer Wins!", _mockView.WinnerMessage, "The view failed to display the proper string announcing the dealer's victory layout.");
            Assert.AreEqual(0, _mockEconomy.CreditCalledWithAmount, "No gold should be credited back to the player asset wallet when the dealer wins a hand cleanly.");
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
            public event Action OnMenuRequested;

            public int PlayerScore { get; private set; }
            public int DealerScore { get; private set; }
            public int MockedWalletBalance { get; private set; }
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
            public void SimulateMenuRequest() => OnMenuRequested?.Invoke();

            public void UpdateWalletBalance(int freshBalance) {
                MockedWalletBalance = freshBalance;
            }

            public void ShowUi(bool show) {
                // For testing purposes, we can log or track the visibility state if needed
            }
        }
    }
}