using NUnit.Framework;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;
using CardFramework.Core.Models;
using CardFramework.Presentation.Views;

namespace CardFramework.Tests.EditMode.Presentation {
    [TestFixture]
    public class BlackjackViewTests : ViewClassForTests {
        private GameObject _testGo;
        private BlackjackView _view;
        private UIDocument _uiDocument;

        private VisualElement _rootElement;
        private Button _hitButton;
        private Button _standButton;
        private Button _restartButton;
        private Label _playerScoreLabel;
        private Label _dealerScoreLabel;
        private VisualElement _outcomeMessageVisualElement;
        private Label _outcomeMessageLabel;

        private GameObject _cardPrefab;
        private GameObject _playerAnchorGo;
        private GameObject _dealerAnchorGo;

        [SetUp]
        public void Setup() {
            // 1. Create the primary GameObject in an inactive state.
            // This prevents Unity from instantly running lifecycle stages (like automatic OnEnable)
            // before we have injected dependencies or constructed the visual layout tree structure.
            _testGo = new GameObject("Test_BlackjackView");
            _testGo.SetActive(false);

            _uiDocument = _testGo.AddComponent<UIDocument>();
            _view = _testGo.AddComponent<BlackjackView>();

            // 2. Configure 3D spatial spawning hierarchies and mock prefab definitions
            _cardPrefab = new GameObject("Mock_CardPrefab");
            _cardPrefab.AddComponent<CardFaceGenerator>(); // Attached to ensure face generator workflows don't return early

            _playerAnchorGo = new GameObject("PlayerAnchor");
            _dealerAnchorGo = new GameObject("DealerAnchor");
            _playerAnchorGo.transform.SetParent(_testGo.transform);
            _dealerAnchorGo.transform.SetParent(_testGo.transform);

            SetPrivateField("cardPrefab", _cardPrefab);
            SetPrivateField("playerSpawnAnchor", _playerAnchorGo.transform);
            SetPrivateField("dealerSpawnAnchor", _dealerAnchorGo.transform);

            // 3. Setup an empty mock asset layout within UI Toolkit to isolate root visual trees safely
            var mockAsset = ScriptableObject.CreateInstance<VisualTreeAsset>();
            _uiDocument.visualTreeAsset = mockAsset;
            _rootElement = _uiDocument.rootVisualElement;
            _rootElement.Clear();

            // 4. Instantiate and inject every UI element exactly matching the target production query markers (Q<T>)
            _hitButton = new Button() { name = "hit-button" };
            _standButton = new Button() { name = "stand-button" };
            _restartButton = new Button() { name = "restart-button" };
            _playerScoreLabel = new Label() { name = "player-score-label" };
            _dealerScoreLabel = new Label() { name = "dealer-score-label" };

            _outcomeMessageVisualElement = new VisualElement() { name = "outcome-message-label" };
            _outcomeMessageLabel = new Label(); // Attached as direct child so visual tree queries retrieve it successfully
            _outcomeMessageVisualElement.Add(_outcomeMessageLabel);

            _rootElement.Add(_hitButton);
            _rootElement.Add(_standButton);
            _rootElement.Add(_restartButton);
            _rootElement.Add(_playerScoreLabel);
            _rootElement.Add(_dealerScoreLabel);
            _rootElement.Add(_outcomeMessageVisualElement);

            // 5. Explicitly invoke the internal OnEnable method using reflection.
            // This ensures that binding confirmation validations run safely against a perfectly constructed visual layout.
            var enableMethod = typeof(BlackjackView).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
            enableMethod.Invoke(_view, null);
        }

        [TearDown]
        public void TearDown() {
            if (_testGo != null) {
                UnityEngine.Object.DestroyImmediate(_testGo);
            }
            if (_cardPrefab != null) {
                UnityEngine.Object.DestroyImmediate(_cardPrefab);
            }
        }

        [Test]
        public void View_OnEnableLifecycle_SuccessfulBindingSetsHasAllTrue() {
            // Evaluates if all essential components are mapped and configured within production bounds
            Assert.IsTrue(_view.HasAll, "HasAll should evaluate to true when all required UXML query links are found inside the tree.");
        }

        [Test]
        public void View_OnEnableLifecycle_MissingElementsSetsHasAllFalse() {
            GameObject genericGo = new GameObject("IncompleteViewGo");
            genericGo.SetActive(false);

            var document = genericGo.AddComponent<UIDocument>();
            var viewComponent = genericGo.AddComponent<BlackjackView>();

            var mockAsset = ScriptableObject.CreateInstance<VisualTreeAsset>();
            document.visualTreeAsset = mockAsset;
            document.rootVisualElement.Clear();

            LogAssert.Expect(LogType.Error, $"[{genericGo.name}]: Missing critical VisualElements inside the UXML tree hierarchy. Verify element Names.");
            LogAssert.Expect(LogType.Error, $"[{genericGo.name}]: Missing 'hit-button' VisualElement.");
            LogAssert.Expect(LogType.Error, $"[{genericGo.name}]: Missing 'stand-button' VisualElement.");
            LogAssert.Expect(LogType.Error, $"[{genericGo.name}]: Missing 'restart-button' VisualElement.");
            LogAssert.Expect(LogType.Error, $"[{genericGo.name}]: Missing 'player-score-label' VisualElement.");
            LogAssert.Expect(LogType.Error, $"[{genericGo.name}]: Missing 'dealer-score-label' VisualElement.");
            LogAssert.Expect(LogType.Error, $"[{genericGo.name}]: Missing 'outcome-message-label' VisualElement.");

            var enableMethod = typeof(BlackjackView).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);

            // Wrap in try/catch because OnEnable natively crashes downstream when registering null callbacks
            try {
                enableMethod.Invoke(viewComponent, null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is NullReferenceException) {
                // Expected downstream crash on missing element event assignment loops
            }

            Assert.IsFalse(viewComponent.HasAll);
            UnityEngine.Object.DestroyImmediate(genericGo);
        }

        [Test]
        public void View_OnDisableLifecycle_CleansCallbacksSuccessfully() {
            var disableMethod = typeof(BlackjackView).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(disableMethod);

            Assert.DoesNotThrow(() => {
                disableMethod.Invoke(_view, null);
            }, "Disabling component path should unhook callbacks without throwing exceptions on valid UI configurations.");
        }

        [Test]
        public void View_OnDisableLifecycle_HandlesNullElementsGracefully() {
            // Nullify bound references via reflection to verify safety checks on components that were never fully bound
            SetPrivateField("_hitButton", null);
            SetPrivateField("_standButton", null);
            SetPrivateField("_restartButton", null);

            var disableMethod = typeof(BlackjackView).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(disableMethod);

            Assert.DoesNotThrow(() => {
                disableMethod.Invoke(_view, null);
            }, "Should process null safety evaluations gracefully before unhooking click callback hooks.");
        }

        [Test]
        public void View_ButtonClicks_DispatchArchitecturalEvents() {
            bool hitCalled = false;
            bool standCalled = false;
            bool restartCalled = false;

            _view.OnHitRequested += () => hitCalled = true;
            _view.OnStandRequested += () => standCalled = true;
            _view.OnRestartRequested += () => restartCalled = true;

            SimulateButtonClick(_hitButton);
            SimulateButtonClick(_standButton);
            SimulateButtonClick(_restartButton);

            Assert.IsTrue(hitCalled, "The OnHitRequested event failed to dispatch.");
            Assert.IsTrue(standCalled, "The OnStandRequested event failed to dispatch.");
            Assert.IsTrue(restartCalled, "The OnRestartRequested event failed to dispatch.");
        }



        [Test]
        public void View_UpdateScores_ModifiesLabelTextLines() {
            _view.UpdatePlayerScore(21);
            _view.UpdateDealerScore(17);

            Assert.AreEqual("Player: 21", _playerScoreLabel.text);
            Assert.AreEqual("Dealer: 17", _dealerScoreLabel.text);
        }

        [Test]
        public void View_DisplayWinner_UpdatesLabelAndSetsDisplayStyleFlex() {
            _view.DisplayWinner("Player Wins!");

            Assert.AreEqual(DisplayStyle.Flex, _outcomeMessageVisualElement.style.display.value);
            Assert.AreEqual("Player Wins!", _outcomeMessageLabel.text);
        }

        [Test]
        public void View_SetInteractionState_TogglesEnabledSelfFlags() {
            _view.SetInteractionState(false);
            Assert.IsFalse(_hitButton.enabledSelf);
            Assert.IsFalse(_standButton.enabledSelf);

            _view.SetInteractionState(true);
            Assert.IsTrue(_hitButton.enabledSelf);
            Assert.IsTrue(_standButton.enabledSelf);
        }

        [Test]
        public void View_ClearTable_ResetsLabelsHidesContainerAndClearsActiveTransforms() {
            var mockTransform1 = new GameObject("PlayerCard_0").transform;
            var mockTransform2 = new GameObject("DealerCard_0").transform;
            mockTransform1.SetParent(_playerAnchorGo.transform);
            mockTransform2.SetParent(_dealerAnchorGo.transform);

            var playerList = (List<Transform>)GetPrivateField("_playerCardTransforms");
            var dealerList = (List<Transform>)GetPrivateField("_dealerCardTransforms");

            playerList.Add(mockTransform1);
            dealerList.Add(mockTransform2);

            // Tell the test engine to expect and allow Unity's native Edit Mode Destroy warnings
            LogAssert.Expect(LogType.Error, "Destroy may not be called from edit mode! Use DestroyImmediate instead.\nDestroying an object in edit mode destroys it permanently.");
            LogAssert.Expect(LogType.Error, "Destroy may not be called from edit mode! Use DestroyImmediate instead.\nDestroying an object in edit mode destroys it permanently.");

            _view.ClearTable();

            Assert.AreEqual(string.Empty, _outcomeMessageLabel.text);
            Assert.AreEqual(DisplayStyle.None, _outcomeMessageVisualElement.style.display.value);
            Assert.AreEqual(0, playerList.Count);
            Assert.AreEqual(0, dealerList.Count);
        }

        [Test]
        public void View_SpawnPhysicalCard_CalculatesHandOffsetsAndGeneratesFaces() {
            CardData playerCard = new CardData(CardData.Suit.Spades, CardData.Rank.Ace);
            CardData dealerCard = new CardData(CardData.Suit.Hearts, CardData.Rank.King);

            // 1. Evaluate single card deployment mechanics for player hand alignment
            _view.SpawnPhysicalCard(playerCard, isPlayer: true);
            var playerList = (List<Transform>)GetPrivateField("_playerCardTransforms");
            Assert.AreEqual(1, playerList.Count);
            Assert.AreEqual(0f, playerList[0].localPosition.x, 0.001f); // A single card should sit exactly at the center (0)

            // 2. Add a second card to verify layout math centering algorithms
            CardData playerCard2 = new CardData(CardData.Suit.Clubs, CardData.Rank.Ten);
            _view.SpawnPhysicalCard(playerCard2, isPlayer: true);
            Assert.AreEqual(2, playerList.Count);

            // Total width offset calculation for 2 cards = 0.07f. Origins should shift to -0.035f and 0.035f respectively.
            Assert.AreEqual(-0.035f, playerList[0].localPosition.x, 0.001f);
            Assert.AreEqual(0.035f, playerList[1].localPosition.x, 0.001f);

            // 3. Evaluate dealer track spawning boundaries
            _view.SpawnPhysicalCard(dealerCard, isPlayer: false);
            var dealerList = (List<Transform>)GetPrivateField("_dealerCardTransforms");
            Assert.AreEqual(1, dealerList.Count);
            Assert.AreEqual(0f, dealerList[0].localPosition.x, 0.001f);
        }

        private void SetPrivateField(string fieldName, object value) {
            var field = typeof(BlackjackView).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) {
                Assert.Fail($"Field '{fieldName}' could not be resolved on BlackjackView.");
            }
            field.SetValue(_view, value);
        }

        private object GetPrivateField(string fieldName) {
            var field = typeof(BlackjackView).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) {
                Assert.Fail($"Field '{fieldName}' could not be resolved on BlackjackView.");
            }
            return field.GetValue(_view);
        }
    }
}