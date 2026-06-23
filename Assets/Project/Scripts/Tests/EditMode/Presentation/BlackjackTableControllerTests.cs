using NUnit.Framework;
using System;
using CardFramework.Core.Engines;
using CardFramework.Presentation.Controllers;
using CardFramework.Presentation.Interfaces;

namespace CardFramework.Tests.EditMode.Presentation
{
    [TestFixture]
    public class BlackjackTableControllerTests
    {
        private BlackjackEngine _engine;
        private MockBlackjackView _mockView;
        private BlackjackTableController _controller;

        [SetUp]
        public void Setup()
        {
            // Injecting clean instances for isolated architectural evaluation
            _engine = new BlackjackEngine();
            _mockView = new MockBlackjackView();
            _controller = new BlackjackTableController(_engine, _mockView);
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }

        [Test]
        public void Controller_OnStart_InitializesTableAndDealsInitialHands()
        {
            // Act
            _controller.Start();

            // Assert
            Assert.IsTrue(_mockView.ClearTableCalled, "The controller must wipe the layout upon session startup.");
            Assert.IsTrue(_mockView.InteractionState, "Player input controls must be enabled during their active turn.");
            Assert.AreNotEqual(0, _mockView.PlayerScore, "Player hand evaluation must be forwarded to the visual interface.");
        }

        [Test]
        public void Controller_OnHitRequest_UpdatesPlayerScoreOnView()
        {
            // Arrange
            _controller.Start();
            int initialScore = _mockView.PlayerScore;

            // Act
            _mockView.SimulateHitRequest();

            // Assert
            Assert.GreaterOrEqual(_mockView.PlayerScore, initialScore, "Hitting must update or increase the player value signature.");
        }

        [Test]
        public void Controller_OnStandRequest_DisablesInteractionAndEvaluatesDealerTurn()
        {
            // Arrange
            _controller.Start();

            // Act
            _mockView.SimulateStandRequest();

            // Assert
            Assert.IsFalse(_mockView.InteractionState, "UI components must be disabled when processing dealer AI execution loops.");
            Assert.IsFalse(string.IsNullOrEmpty(_mockView.WinnerMessage), "A clear victor or tie match conclusion must be announced upon Standing.");
        }

        /// <summary>
        /// Controlled Mock implementation mimicking the UI Canvas layer boundaries.
        /// </summary>
        private class MockBlackjackView : IBlackjackView
        {
            public event Action OnHitRequested;
            public event Action OnStandRequested;
            public event Action OnRestartRequested;

            public int PlayerScore { get; private set; }
            public int DealerScore { get; private set; }
            public string WinnerMessage { get; private set; }
            public bool ClearTableCalled { get; private set; }
            public bool InteractionState { get; private set; }

            public void UpdatePlayerScore(int score) => PlayerScore = score;
            public void UpdateDealerScore(int score) => DealerScore = score;
            public void DisplayWinner(string winnerName) => WinnerMessage = winnerName;
            public void ClearTable() => ClearTableCalled = true;
            public void SetInteractionState(bool canInteract) => InteractionState = canInteract;

            public void SimulateHitRequest() => OnHitRequested?.Invoke();
            public void SimulateStandRequest() => OnStandRequested?.Invoke();
            public void SimulateRestartRequest() => OnRestartRequested?.Invoke();
        }
    }
}